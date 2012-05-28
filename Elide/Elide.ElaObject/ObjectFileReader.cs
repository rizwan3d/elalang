using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ela.CodeModel;
using Ela.Compilation;
using Elide.ElaObject.ObjectModel;

namespace Elide.ElaObject
{
    internal sealed class ObjectFileReader
    {
        public ElaObjectFile Read(BinaryReader reader)
        {
            return new ElaObjectFile(
                ReadHeader(reader),
                ReadReferences(reader).ToList(),
                ReadGlobals(reader).ToList(),
                ReadLateBounds(reader).ToList(),
                ReadLayouts(reader).ToList(),
                ReadStrings(reader).ToList(),
                ReadCode(reader).ToList());
        }

        private IEnumerable<Reference> ReadReferences(BinaryReader br)
        {
            var c = br.ReadInt32();

            for (var i = 0; i < c; i++)
            {
                var alias = br.ReadString();
                var modName = br.ReadString();
                var dllName = br.ReadString();
                dllName = dllName.Length == 0 ? null : dllName;
                var qual = br.ReadBoolean();
                var pl = br.ReadInt32();
                var list = new string[pl];

                for (var j = 0; j < pl; j++)
                    list[j] = br.ReadString();

                var hld = br.ReadInt32(); //Module handle

                if (!modName.StartsWith("$__"))
                    yield return new Reference(modName, alias, dllName,
                        list.Length > 0 ? String.Join("\\", list) : String.Empty, qual);
            }
        }

        private IEnumerable<LateBound> ReadLateBounds(BinaryReader br)
        {
            var c = br.ReadInt32();

            for (var i = 0; i < c; i++)
            {
                var name = br.ReadString();
                var address = br.ReadInt32();
                var data = br.ReadInt32();
                var line = br.ReadInt32();
                var col = br.ReadInt32();

                yield return new LateBound(name, address, data, line, col);
            }
        }

        private IEnumerable<Global> ReadGlobals(BinaryReader br)
        {
            var c = br.ReadInt32();

            for (var i = 0; i < c; i++)
            {
                var name = br.ReadString();
                var flags = (ElaVariableFlags)br.ReadInt32();
                var address = br.ReadInt32();
                var data = br.ReadInt32();

                if ((flags & ElaVariableFlags.SpecialName) != ElaVariableFlags.SpecialName)
                    yield return new Global(name, flags, address, data);
            }
        }

        private IEnumerable<Layout> ReadLayouts(BinaryReader br)
        {
            var c = br.ReadInt32();

            for (var i = 0; i < c; i++)
                yield return new Layout(br.ReadInt32(), br.ReadInt32(), br.ReadInt32());
        }

        private IEnumerable<String> ReadStrings(BinaryReader br)
        {
            var c = br.ReadInt32();

            for (var i = 0; i < c; i++)
                yield return br.ReadString();
        }

        private IEnumerable<OpCode> ReadCode(BinaryReader br)
        {
            var c = br.ReadInt32();
            
            for (var i = 0; i < c; i++)
            {
                var opCode = (Op)br.ReadByte();
                var arg = ElaCompiler.GetOpCodeSize(opCode) > 1 ? (int?)br.ReadInt32() : null;
                yield return new OpCode(i, opCode, arg);
            }
        }

        private Header ReadHeader(BinaryReader br)
        {
            return new Header(br.ReadInt32(), new Version(br.ReadInt32(), br.ReadInt32(), br.ReadInt32(), br.ReadInt32()), new DateTime(br.ReadInt64()));
        }
    }
}
