using System;
using System.IO;
using Ela.CodeModel;

namespace Ela
{
	public class ElaMessage
	{
		#region Construction
		private const string ERR_FORMAT = "({0},{1}): {2} ELA{3}: {4}";
		private const string LONG_ERR_FORMAT = "{0}({1},{2}): {3} ELA{4}: {5}";

		internal ElaMessage(string errorMessage, MessageType type, int code, int line, int column)
		{
			Message = errorMessage;
			Code = code;
			Line = line;
			Column = column;
			Type = type;
		}
		#endregion


		#region Methods
		public override string ToString()
		{
			return File != null ?
				String.Format(LONG_ERR_FORMAT, File, Line, Column, Type.ToString(), Code, Message) :
				String.Format(ERR_FORMAT, Line, Column, Type.ToString(), Code, Message);
		}
		#endregion


		#region Properties
		public FileInfo File { get; internal set; }

		public MessageType Type { get; private set; }
		
		public int Code { get; private set; }

		public string Message { get; private set; }

		public int Line { get; private set; }

		public int Column { get; private set; }
		#endregion
	}
}
