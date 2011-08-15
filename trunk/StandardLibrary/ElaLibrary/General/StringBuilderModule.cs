using System;
using System.Text;
using Ela.Linking;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;

namespace Ela.Library.General
{
    public sealed class StringBuilderModule : ForeignModule
    {
        #region Construction
        private TypeId stringBuilderTypeId;

        public StringBuilderModule()
        {

        }
        #endregion


        #region Nested Classes
        public sealed class ElaStringBuilder : ElaObject
        {
            #region Construction
            private const string TAG = "StringBuilder#";

            public ElaStringBuilder(StringBuilder builder, ElaMachine vm)
                : this(builder, vm.GetTypeId(TAG))
            {

            }


            internal ElaStringBuilder(StringBuilder builder, TypeId typeId) : base(typeId)
            {
                Builder = builder;
            }
            #endregion


            #region Methods
            public override string GetTag()
            {
                return TAG;
            }


            public override int GetHashCode()
            {
                return Builder.GetHashCode();
            }
            #endregion


            #region Properties
            internal StringBuilder Builder { get; private set; }
            #endregion
        }
        #endregion


        #region Methods
        public override void RegisterTypes(TypeRegistrator registrator)
        {
            stringBuilderTypeId = registrator.ObtainTypeId("StringBuilder#");
        }


        public override void Initialize()
        {
            Add<ElaValue,ElaObject>("builder", CreateBuilder);
            Add<String,ElaStringBuilder,ElaStringBuilder>("append", Append);
            Add<Char,ElaStringBuilder,ElaStringBuilder>("appendChar", AppendChar);
            Add<String,ElaStringBuilder,ElaStringBuilder>("appendLine", Append);
            Add<Int32,String,ElaStringBuilder,ElaStringBuilder>("insert", Insert);
            Add<Int32,Char,ElaStringBuilder,ElaStringBuilder>("insertChar", InsertChar);
            Add<String,String,ElaStringBuilder,ElaStringBuilder>("replace", Replace);
            Add<Char,Char,ElaStringBuilder,ElaStringBuilder>("replaceChar", ReplaceChar);
            Add<Int32,Int32,ElaStringBuilder,ElaStringBuilder>("remove", Remove);

            Add<ElaStringBuilder,Int32>("stringBuilderLength", b => b.Builder.Length);
            Add<ElaStringBuilder,String>("stringBuilderToString", b => b.Builder.ToString());
        }


        public ElaStringBuilder CreateBuilder(ElaValue val)
        {
            var init = val.TypeCode != ElaTypeCode.Unit ?
                val.ToString() : String.Empty;
            return new ElaStringBuilder(new StringBuilder(init), stringBuilderTypeId);
        }


        public ElaStringBuilder Append(string str, ElaStringBuilder bo)
        {
            bo.Builder.Append(str);
            return bo;
        }


        public ElaStringBuilder AppendChar(char ch, ElaStringBuilder bo)
        {
            bo.Builder.Append(ch);
            return bo;
        }


        public ElaStringBuilder AppendLine(string str, ElaStringBuilder bo)
        {
            bo.Builder.AppendLine(str);
            return bo;
        }


        public ElaStringBuilder Insert(int startIndex, string str, ElaStringBuilder bo)
        {
            bo.Builder.Insert(startIndex, str);
            return bo;
        }


        public ElaStringBuilder InsertChar(int startIndex, char ch, ElaStringBuilder bo)
        {
            bo.Builder.Insert(startIndex, ch);
            return bo;
        }


        public ElaStringBuilder Replace(string oldValue, string newValue, ElaStringBuilder bo)
        {
            bo.Builder.Replace(oldValue, newValue);
            return bo;
        }


        public ElaStringBuilder ReplaceChar(char oldValue, char newValue, ElaStringBuilder bo)
        {
            bo.Builder.Replace(oldValue, newValue);
            return bo;
        }


        public ElaStringBuilder Remove(int startIndex, int length, ElaStringBuilder bo)
        {
            bo.Builder.Remove(startIndex, length);
            return bo;
        }
        #endregion
    }
}
