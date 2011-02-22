using System;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.StandardLibrary
{
    public sealed class ConModule : ForeignModule
    {
        #region Construction
        public ConModule()
        {

        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add<ElaValue,ElaUnit>("write", Write);
            Add<ElaValue,ElaUnit>("writen", WriteLine);
            Add<String>("readn", ReadLine);
            Add<ElaUnit>("cls", Clear);
            Add<ElaUnit>("beep", Beep);
            Add<ElaFunction,ElaUnit>("onCancel", SetOnCancel);
        }


        public ElaUnit Write(ElaValue val)
        {
            Console.Write(val.ToString());
            return ElaUnit.Instance;
        }


        public ElaUnit WriteLine(ElaValue val)
        {
            Console.WriteLine(val.ToString());
            return ElaUnit.Instance;
        }


        public string ReadLine()
        {
            return Console.ReadLine();
        }


        public ElaUnit Clear()
        {
            Console.Clear();
            return ElaUnit.Instance;
        }


        public ElaUnit Beep()
        {
            Console.Beep();
            return ElaUnit.Instance;
        }


        public ElaUnit SetOnCancel(ElaFunction fun)
        {
            Console.CancelKeyPress += (o, e) => {
                var vr = e.SpecialKey == ConsoleSpecialKey.ControlBreak ?
                    new ElaVariant("CtrlBreak") : new ElaVariant("CtrlC");
                e.Cancel = fun.Call(new ElaValue(vr)).AsBoolean();
            };
            return ElaUnit.Instance;
        }
        #endregion
    }
}
