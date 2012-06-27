using System;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
    public sealed class CellModule : ForeignModule
    {
        #region Construction
        public CellModule()
        {

        }

        private sealed class ElaCell : ElaObject
        {
            public ElaCell()
            {

            }

            internal ElaValue Value { get; set; }
        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add<ElaValue,ElaObject>("cell", Cell);
            Add<ElaValue,ElaObject,ElaObject>("mutate", Mutate);
            Add<ElaObject,ElaValue>("valueof", ValueOf);
        }


        public ElaObject Cell(ElaValue value)
        {
            return new ElaCell { Value = value };
        }


        public ElaObject Mutate(ElaValue value, ElaObject obj)
        {
            var cell = obj as ElaCell;

            if (cell == null)
                throw new InvalidCastException();
            
            cell.Value = value;
            return cell;
        }


        public ElaValue ValueOf(ElaObject obj)
        {
            var cell = obj as ElaCell;

            if (cell == null)
                throw new InvalidCastException();

            return cell.Value;
        }
        #endregion
    }
}
