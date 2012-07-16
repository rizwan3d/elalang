using System;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
    public sealed class CellModule : ForeignModule
    {
        public CellModule()
        {

        }
        
        public override void Initialize()
        {
            Add<ElaValue,ElaUserType,ElaUserType>("mutate", Mutate);
        }

        public ElaUserType Mutate(ElaValue value, ElaUserType obj)
        {
            var arr = (ElaValue[])obj.GetValues();
            arr[0] = value;
            return obj;
        }
    }
}
