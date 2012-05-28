
using System;
using System.IO;
using System.Collections;
using System.Text;


namespace Elide.EilCode.Lexer {

internal sealed class Token {
	public int kind;    // token kind
	public int pos;     // token position in the source text (starting at 0)
	public int col;     // token column (starting at 1)
	public int line;    // token line (starting at 1)
	public string val;  // token value
	public Token next;  // ML 2005-03-11 Tokens are kept in linked list
}

//-----------------------------------------------------------------------------------
// Buffer
//-----------------------------------------------------------------------------------
internal class Buffer {
	// This Buffer supports the following cases:
	// 1) seekable stream (file)
	//    a) whole stream in buffer
	//    b) part of stream in buffer
	// 2) non seekable stream (network, console)

	public const int EOF = char.MaxValue + 1;
	const int MIN_BUFFER_LENGTH = 1024; // 1KB
	const int MAX_BUFFER_LENGTH = MIN_BUFFER_LENGTH * 64; // 64KB
	byte[] buf;         // input buffer
	int bufStart;       // position of first byte in buffer relative to input stream
	int bufLen;         // length of buffer
	int fileLen;        // length of input stream (may change if the stream is no file)
	int bufPos;         // current position in buffer
	Stream stream;      // input stream (seekable)
	bool isUserStream;  // was the stream opened by the user?
	
	public Buffer (Stream s, bool isUserStream) {
		stream = s; this.isUserStream = isUserStream;
		
		if (stream.CanSeek) {
			fileLen = (int) stream.Length;
			bufLen = Math.Min(fileLen, MAX_BUFFER_LENGTH);
			bufStart = Int32.MaxValue; // nothing in the buffer so far
		} else {
			fileLen = bufLen = bufStart = 0;
		}

		buf = new byte[(bufLen>0) ? bufLen : MIN_BUFFER_LENGTH];
		if (fileLen > 0) Pos = 0; // setup buffer to position 0 (start)
		else bufPos = 0; // index 0 is already after the file, thus Pos = 0 is invalid
		if (bufLen == fileLen && stream.CanSeek) Close();
	}
		
	protected Buffer(Buffer b) { // called in UTF8Buffer constructor
		buf = b.buf;
		bufStart = b.bufStart;
		bufLen = b.bufLen;
		fileLen = b.fileLen;
		bufPos = b.bufPos;
		stream = b.stream;
		// keep destructor from closing the stream
		b.stream = null;
		isUserStream = b.isUserStream;
	}

	~Buffer() { Close(); }
	
	protected void Close() {
		if (!isUserStream && stream != null) {
			stream.Close();
			stream = null;
		}
	}
	
	public virtual int Read () {
		if (bufPos < bufLen) {
			return buf[bufPos++];
		} else if (Pos < fileLen) {
			Pos = Pos; // shift buffer start to Pos
			return buf[bufPos++];
		} else if (stream != null && !stream.CanSeek && ReadNextStreamChunk() > 0) {
			return buf[bufPos++];
		} else {
			return EOF;
		}
	}

	public int Peek () {
		int curPos = Pos;
		int ch = Read();
		Pos = curPos;
		return ch;
	}
	
	public string GetString (int beg, int end) {
		int len = 0;
		char[] buf = new char[end - beg];
		int oldPos = Pos;
		Pos = beg;
		while (Pos < end) buf[len++] = (char) Read();
		Pos = oldPos;
		return new String(buf, 0, len);
	}

	public int Pos {
		get { return bufPos + bufStart; }
		set {
			if (value >= fileLen && stream != null && !stream.CanSeek) {
				// Wanted position is after buffer and the stream
				// is not seek-able e.g. network or console,
				// thus we have to read the stream manually till
				// the wanted position is in sight.
				while (value >= fileLen && ReadNextStreamChunk() > 0);
			}

			if (value < 0 || value > fileLen) {
				throw new FatalError("buffer out of bounds access, position: " + value);
			}

			if (value >= bufStart && value < bufStart + bufLen) { // already in buffer
				bufPos = value - bufStart;
			} else if (stream != null) { // must be swapped in
				stream.Seek(value, SeekOrigin.Begin);
				bufLen = stream.Read(buf, 0, buf.Length);
				bufStart = value; bufPos = 0;
			} else {
				// set the position to the end of the file, Pos will return fileLen.
				bufPos = fileLen - bufStart;
			}
		}
	}
	
	// Read the next chunk of bytes from the stream, increases the buffer
	// if needed and updates the fields fileLen and bufLen.
	// Returns the number of bytes read.
	private int ReadNextStreamChunk() {
		int free = buf.Length - bufLen;
		if (free == 0) {
			// in the case of a growing input stream
			// we can neither seek in the stream, nor can we
			// foresee the maximum length, thus we must adapt
			// the buffer size on demand.
			byte[] newBuf = new byte[bufLen * 2];
			Array.Copy(buf, newBuf, bufLen);
			buf = newBuf;
			free = bufLen;
		}
		int read = stream.Read(buf, bufLen, free);
		if (read > 0) {
			fileLen = bufLen = (bufLen + read);
			return read;
		}
		// end of stream reached
		return 0;
	}
}

//-----------------------------------------------------------------------------------
// UTF8Buffer
//-----------------------------------------------------------------------------------
internal sealed class UTF8Buffer: Buffer {
	public UTF8Buffer(Buffer b): base(b) {}

	public override int Read() {
		int ch;
		do {
			ch = base.Read();
			// until we find a utf8 start (0xxxxxxx or 11xxxxxx)
		} while ((ch >= 128) && ((ch & 0xC0) != 0xC0) && (ch != EOF));
		if (ch < 128 || ch == EOF) {
			// nothing to do, first 127 chars are the same in ascii and utf8
			// 0xxxxxxx or end of file character
		} else if ((ch & 0xF0) == 0xF0) {
			// 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
			int c1 = ch & 0x07; ch = base.Read();
			int c2 = ch & 0x3F; ch = base.Read();
			int c3 = ch & 0x3F; ch = base.Read();
			int c4 = ch & 0x3F;
			ch = (((((c1 << 6) | c2) << 6) | c3) << 6) | c4;
		} else if ((ch & 0xE0) == 0xE0) {
			// 1110xxxx 10xxxxxx 10xxxxxx
			int c1 = ch & 0x0F; ch = base.Read();
			int c2 = ch & 0x3F; ch = base.Read();
			int c3 = ch & 0x3F;
			ch = (((c1 << 6) | c2) << 6) | c3;
		} else if ((ch & 0xC0) == 0xC0) {
			// 110xxxxx 10xxxxxx
			int c1 = ch & 0x1F; ch = base.Read();
			int c2 = ch & 0x3F;
			ch = (c1 << 6) | c2;
		}
		return ch;
	}
}

//-----------------------------------------------------------------------------------
// Scanner
//-----------------------------------------------------------------------------------
internal sealed class Scanner {
	const char EOL = '\n';
	const int eofSym = 0; /* pdt */
	const int maxT = 110;
	const int noSym = 110;


	public Buffer buffer; // scanner buffer
	
	Token t;          // current token
	int ch;           // current input character
	int pos;          // byte position of current character
	int col;          // column number of current character
	int line;         // line number of current character
	int oldEols;      // EOLs that appeared in a comment;
	static readonly Hashtable start; // maps first token character to start state

	Token tokens;     // list of tokens already peeked (first token is a dummy)
	Token pt;         // current peek token
	
	char[] tval = new char[128]; // text of current token
	int tlen;         // length of current token
	
	static Scanner() {
		start = new Hashtable(128);
		for (int i = 65; i <= 90; ++i) start[i] = 1;
		for (int i = 97; i <= 122; ++i) start[i] = 1;
		for (int i = 49; i <= 57; ++i) start[i] = 26;
		for (int i = 33; i <= 33; ++i) start[i] = 23;
		for (int i = 37; i <= 38; ++i) start[i] = 23;
		for (int i = 42; i <= 44; ++i) start[i] = 23;
		for (int i = 46; i <= 47; ++i) start[i] = 23;
		for (int i = 58; i <= 58; ++i) start[i] = 23;
		for (int i = 60; i <= 64; ++i) start[i] = 23;
		for (int i = 92; i <= 92; ++i) start[i] = 23;
		for (int i = 94; i <= 94; ++i) start[i] = 23;
		for (int i = 96; i <= 96; ++i) start[i] = 23;
		for (int i = 124; i <= 124; ++i) start[i] = 23;
		for (int i = 126; i <= 126; ++i) start[i] = 23;
		for (int i = 40; i <= 41; ++i) start[i] = 24;
		for (int i = 91; i <= 91; ++i) start[i] = 24;
		for (int i = 93; i <= 93; ++i) start[i] = 24;
		for (int i = 123; i <= 123; ++i) start[i] = 24;
		for (int i = 125; i <= 125; ++i) start[i] = 24;
		for (int i = 10; i <= 10; ++i) start[i] = 25;
		for (int i = 13; i <= 13; ++i) start[i] = 25;
		start[95] = 27; 
		start[39] = 2; 
		start[36] = 28; 
		start[48] = 29; 
		start[45] = 30; 
		start[35] = 11; 
		start[34] = 17; 
		start[Buffer.EOF] = -1;

	}
	
	public Scanner (string fileName) {
		try {
			Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
			buffer = new Buffer(stream, false);
			Init();
		} catch (IOException) {
			throw new FatalError("Cannot open file " + fileName);
		}
	}
		
	
	public Scanner (Stream s) {
		buffer = new Buffer(s, true);
		Init();
	}
	
	void Init() {
		pos = -1; line = 1; col = 0;
		oldEols = 0;
		NextCh();
		if (ch == 0xEF) { // check optional byte order mark for UTF-8
			NextCh(); int ch1 = ch;
			NextCh(); int ch2 = ch;
			if (ch1 != 0xBB || ch2 != 0xBF) {
				throw new FatalError(String.Format("illegal byte order mark: EF {0,2:X} {1,2:X}", ch1, ch2));
			}
			buffer = new UTF8Buffer(buffer); col = 0;
			NextCh();
		}
		pt = tokens = new Token();  // first token is a dummy
	}
	
	void NextCh() {
		if (oldEols > 0) { ch = EOL; oldEols--; } 
		else {
			pos = buffer.Pos;
			ch = buffer.Read(); col++;
			// replace isolated '\r' by '\n' in order to make
			// eol handling uniform across Windows, Unix and Mac
			if (ch == '\r' && buffer.Peek() != '\n') ch = EOL;
			if (ch == EOL) { line++; col = 0; }
		}

	}

	void AddCh() {
		if (tlen >= tval.Length) {
			char[] newBuf = new char[2 * tval.Length];
			Array.Copy(tval, 0, newBuf, 0, tval.Length);
			tval = newBuf;
		}
		if (ch != Buffer.EOF) {
			tval[tlen++] = (char) ch;
			NextCh();
		}
	}




	void CheckLiteral() {
		switch (t.val) {
			case "//": t.kind = 8; break;
			case "[": t.kind = 9; break;
			case "]": t.kind = 10; break;
			case "Nop": t.kind = 11; break;
			case "Len": t.kind = 12; break;
			case "Pushunit": t.kind = 13; break;
			case "Pushelem": t.kind = 14; break;
			case "PushI4_0": t.kind = 15; break;
			case "PushI1_0": t.kind = 16; break;
			case "PushI1_1": t.kind = 17; break;
			case "Pop": t.kind = 18; break;
			case "Popelem": t.kind = 19; break;
			case "Pushstr_0": t.kind = 20; break;
			case "Reccons": t.kind = 21; break;
			case "Genfin": t.kind = 22; break;
			case "Cons": t.kind = 23; break;
			case "Gen": t.kind = 24; break;
			case "Tail": t.kind = 25; break;
			case "Head": t.kind = 26; break;
			case "Ret": t.kind = 27; break;
			case "Concat": t.kind = 28; break;
			case "Add": t.kind = 29; break;
			case "Mul": t.kind = 30; break;
			case "Div": t.kind = 31; break;
			case "Rem": t.kind = 32; break;
			case "Pow": t.kind = 33; break;
			case "Sub": t.kind = 34; break;
			case "Shr": t.kind = 35; break;
			case "Shl": t.kind = 36; break;
			case "Ceq": t.kind = 37; break;
			case "Cneq": t.kind = 38; break;
			case "Clt": t.kind = 39; break;
			case "Cgt": t.kind = 40; break;
			case "Cgteq": t.kind = 41; break;
			case "Clteq": t.kind = 42; break;
			case "AndBw": t.kind = 43; break;
			case "OrBw": t.kind = 44; break;
			case "Xor": t.kind = 45; break;
			case "Not": t.kind = 46; break;
			case "Neg": t.kind = 47; break;
			case "NotBw": t.kind = 48; break;
			case "Dup": t.kind = 49; break;
			case "Swap": t.kind = 50; break;
			case "Newlazy": t.kind = 51; break;
			case "Newlist": t.kind = 52; break;
			case "Newmod": t.kind = 53; break;
			case "Newtup_2": t.kind = 54; break;
			case "Typeid": t.kind = 55; break;
			case "Stop": t.kind = 56; break;
			case "NewI8": t.kind = 57; break;
			case "NewR8": t.kind = 58; break;
			case "Leave": t.kind = 59; break;
			case "Flip": t.kind = 60; break;
			case "LazyCall": t.kind = 61; break;
			case "Call": t.kind = 62; break;
			case "Callt": t.kind = 63; break;
			case "Throw": t.kind = 64; break;
			case "Rethrow": t.kind = 65; break;
			case "Force": t.kind = 66; break;
			case "Type": t.kind = 67; break;
			case "Isnil": t.kind = 68; break;
			case "Succ": t.kind = 69; break;
			case "Pred": t.kind = 70; break;
			case "Max": t.kind = 71; break;
			case "Min": t.kind = 72; break;
			case "Show": t.kind = 73; break;
			case "Untag": t.kind = 74; break;
			case "Skiphtag": t.kind = 75; break;
			case "Nil": t.kind = 76; break;
			case "Gettag": t.kind = 77; break;
			case "Conv": t.kind = 78; break;
			case "Pushext": t.kind = 79; break;
			case "Elem": t.kind = 80; break;
			case "Skiptag": t.kind = 81; break;
			case "Newvar": t.kind = 82; break;
			case "Newrec": t.kind = 83; break;
			case "Newtup": t.kind = 84; break;
			case "Skiptn": t.kind = 85; break;
			case "Skiptl": t.kind = 86; break;
			case "Pat": t.kind = 87; break;
			case "Tupex": t.kind = 88; break;
			case "Failwith": t.kind = 89; break;
			case "Start": t.kind = 90; break;
			case "Pushstr": t.kind = 91; break;
			case "PushCh": t.kind = 92; break;
			case "PushI4": t.kind = 93; break;
			case "PushR4": t.kind = 94; break;
			case "Pushvar": t.kind = 95; break;
			case "Pushfld": t.kind = 96; break;
			case "Popvar": t.kind = 97; break;
			case "Popfld": t.kind = 98; break;
			case "Runmod": t.kind = 99; break;
			case "Has": t.kind = 100; break;
			case "Br": t.kind = 101; break;
			case "Brtrue": t.kind = 102; break;
			case "Brfalse": t.kind = 103; break;
			case "Br_lt": t.kind = 104; break;
			case "Br_gt": t.kind = 105; break;
			case "Br_eq": t.kind = 106; break;
			case "Br_neq": t.kind = 107; break;
			case "Brnil": t.kind = 108; break;
			case "Newfun": t.kind = 109; break;
			default: break;
		}
	}

	Token NextToken() {
		while (ch == ' ' ||
			ch == 9
		) NextCh();

		int apx = 0;
		int recKind = noSym;
		int recEnd = pos;
		t = new Token();
		t.pos = pos; t.col = col; t.line = line; 
		int state;
		if (start.ContainsKey(ch)) { state = (int) start[ch]; }
		else { state = 0; }
		tlen = 0; AddCh();
		
		switch (state) {
			case -1: { t.kind = eofSym; break; } // NextCh already done
			case 0: {
				if (recKind != noSym) {
					tlen = recEnd - t.pos;
					SetScannerBehindT();
				}
				t.kind = recKind; break;
			} // NextCh already done
			case 1:
				recEnd = pos; recKind = 1;
				if (ch == 39 || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 2:
				if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 3;}
				else {goto case 0;}
			case 3:
				recEnd = pos; recKind = 1;
				if (ch == 39 || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 4;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 4:
				if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 31;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 32;}
				else if (ch == 39) {AddCh(); goto case 4;}
				else {goto case 0;}
			case 5:
				{t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 6:
				{
					tlen -= apx;
					SetScannerBehindT();
					t.kind = 2; break;}
			case 7:
				if (ch <= '/' || ch >= ':' && ch <= 65535) {apx++; AddCh(); goto case 6;}
				else {goto case 0;}
			case 8:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 9;}
				else {goto case 0;}
			case 9:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 9;}
				else if (ch == 'L' || ch == 'l') {AddCh(); goto case 10;}
				else {t.kind = 2; break;}
			case 10:
				{t.kind = 2; break;}
			case 11:
				if (ch >= '1' && ch <= '9') {AddCh(); goto case 33;}
				else if (ch == '0') {AddCh(); goto case 34;}
				else {goto case 0;}
			case 12:
				{
					tlen -= apx;
					SetScannerBehindT();
					t.kind = 3; break;}
			case 13:
				if (ch <= '/' || ch >= ':' && ch <= 65535) {apx++; AddCh(); goto case 12;}
				else {goto case 0;}
			case 14:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 15;}
				else {goto case 0;}
			case 15:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 15;}
				else if (ch == 'L' || ch == 'l') {AddCh(); goto case 16;}
				else {t.kind = 3; break;}
			case 16:
				{t.kind = 3; break;}
			case 17:
				if (ch <= 9 || ch >= 11 && ch <= 12 || ch >= 14 && ch <= '!' || ch >= '#' && ch <= '[' || ch >= ']' && ch <= 65535) {AddCh(); goto case 17;}
				else if (ch == '"') {AddCh(); goto case 22;}
				else if (ch == 92) {AddCh(); goto case 35;}
				else {goto case 0;}
			case 18:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 19;}
				else {goto case 0;}
			case 19:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 20;}
				else {goto case 0;}
			case 20:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 21;}
				else {goto case 0;}
			case 21:
				if (ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'F' || ch >= 'a' && ch <= 'f') {AddCh(); goto case 17;}
				else {goto case 0;}
			case 22:
				{t.kind = 4; break;}
			case 23:
				recEnd = pos; recKind = 5;
				if (ch == '!' || ch >= '$' && ch <= '&' || ch >= '*' && ch <= '/' || ch == ':' || ch >= '<' && ch <= '@' || ch == 92 || ch == '^' || ch == '`' || ch == '|' || ch == '~') {AddCh(); goto case 23;}
				else {t.kind = 5; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 24:
				recEnd = pos; recKind = 6;
				if (ch >= '(' && ch <= ')' || ch == '[' || ch == ']' || ch == '{' || ch == '}') {AddCh(); goto case 24;}
				else {t.kind = 6; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 25:
				{t.kind = 7; break;}
			case 26:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 26;}
				else if (ch == 'L' || ch == 'l') {AddCh(); goto case 10;}
				else if (ch == '.') {apx++; AddCh(); goto case 7;}
				else {t.kind = 2; break;}
			case 27:
				recEnd = pos; recKind = 1;
				if (ch == 39 || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 36;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 1;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 28:
				recEnd = pos; recKind = 5;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 5;}
				else if (ch == '!' || ch >= '$' && ch <= '&' || ch >= '*' && ch <= '/' || ch == ':' || ch >= '<' && ch <= '@' || ch == 92 || ch == '^' || ch == '`' || ch == '|' || ch == '~') {AddCh(); goto case 23;}
				else {t.kind = 5; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 29:
				recEnd = pos; recKind = 2;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 26;}
				else if (ch == 'L' || ch == 'l') {AddCh(); goto case 10;}
				else if (ch == '.') {apx++; AddCh(); goto case 7;}
				else if (ch == 'X' || ch == 'x') {AddCh(); goto case 8;}
				else {t.kind = 2; break;}
			case 30:
				recEnd = pos; recKind = 5;
				if (ch >= '1' && ch <= '9') {AddCh(); goto case 26;}
				else if (ch == '!' || ch >= '$' && ch <= '&' || ch >= '*' && ch <= '/' || ch == ':' || ch >= '<' && ch <= '@' || ch == 92 || ch == '^' || ch == '`' || ch == '|' || ch == '~') {AddCh(); goto case 23;}
				else if (ch == '0') {AddCh(); goto case 29;}
				else {t.kind = 5; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 31:
				recEnd = pos; recKind = 1;
				if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 31;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 32;}
				else if (ch == 39) {AddCh(); goto case 4;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 32:
				recEnd = pos; recKind = 1;
				if (ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 31;}
				else if (ch >= '0' && ch <= '9') {AddCh(); goto case 32;}
				else if (ch == 39) {AddCh(); goto case 4;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}
			case 33:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 33;}
				else if (ch == 'L' || ch == 'l') {AddCh(); goto case 16;}
				else if (ch == '.') {apx++; AddCh(); goto case 13;}
				else {t.kind = 3; break;}
			case 34:
				recEnd = pos; recKind = 3;
				if (ch >= '0' && ch <= '9') {AddCh(); goto case 33;}
				else if (ch == 'L' || ch == 'l') {AddCh(); goto case 16;}
				else if (ch == '.') {apx++; AddCh(); goto case 13;}
				else if (ch == 'X' || ch == 'x') {AddCh(); goto case 14;}
				else {t.kind = 3; break;}
			case 35:
				if (ch == '"' || ch == 39 || ch == '0' || ch == 92 || ch == 'b' || ch == 'n' || ch == 'r' || ch == 't') {AddCh(); goto case 17;}
				else if (ch == 'u') {AddCh(); goto case 18;}
				else {goto case 0;}
			case 36:
				recEnd = pos; recKind = 1;
				if (ch == 39 || ch >= '0' && ch <= '9' || ch >= 'A' && ch <= 'Z' || ch == '_' || ch >= 'a' && ch <= 'z') {AddCh(); goto case 36;}
				else {t.kind = 1; t.val = new String(tval, 0, tlen); CheckLiteral(); return t;}

		}
		t.val = new String(tval, 0, tlen);
		return t;
	}
	
	private void SetScannerBehindT() {
		buffer.Pos = t.pos;
		NextCh();
		line = t.line; col = t.col;
		for (int i = 0; i < tlen; i++) NextCh();
	}
	
	// get the next token (possibly a token already seen during peeking)
	public Token Scan () {
		if (tokens.next == null) {
			return NextToken();
		} else {
			pt = tokens = tokens.next;
			return tokens;
		}
	}

	// peek for the next token, ignore pragmas
	public Token Peek () {
		do {
			if (pt.next == null) {
				pt.next = NextToken();
			}
			pt = pt.next;
		} while (pt.kind > maxT); // skip pragmas
	
		return pt;
	}

	// make sure that peeking starts at the current scan position
	public void ResetPeek () { pt = tokens; }

} // end Scanner

}

