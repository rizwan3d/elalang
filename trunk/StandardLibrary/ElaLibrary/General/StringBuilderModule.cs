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
        public StringBuilderModule()
        {

        }
        #endregion


        #region Nested Classes
        public sealed class ElaStringBuilder : ElaObject
        {
            #region Construction
            internal ElaStringBuilder(StringBuilder builder) : base(ElaTraits.Eq | ElaTraits.Show | ElaTraits.Len | ElaTraits.Convert)
            {
                Builder = builder;
            }
            #endregion


            #region Methods
            protected override ElaValue Equals(ElaValue left, ElaValue right, ExecutionContext ctx)
            {
                return new ElaValue(left.ReferenceEquals(right));
            }


            protected override ElaValue NotEquals(ElaValue left, ElaValue right, ExecutionContext ctx)
            {
                return new ElaValue(left.ReferenceEquals(right));
            }


            protected override string Show(ExecutionContext ctx, ShowInfo info)
            {
                return Builder.ToString();
            }


            protected override ElaValue Convert(ElaTypeCode type, ExecutionContext ctx)
            {
                if (type == ElaTypeCode.String)
                    return new ElaValue(Builder.ToString());
                
                ctx.ConversionFailed(new ElaValue(this), type);
                return Default();
            }


            protected override ElaValue GetLength(ExecutionContext ctx)
            {
                return new ElaValue(Builder.Length);
            }
            #endregion


            #region Properties
            internal StringBuilder Builder { get; private set; }
            #endregion
        }
        #endregion


        #region Methods
        public override void Initialize()
        {
            Add<ElaValue,ElaObject>("builder", CreateBuilder);
            Add<String,ElaStringBuilder,ElaStringBuilder>("append", Append);
            Add<Char,ElaStringBuilder,ElaStringBuilder>("appendChar", AppendChar);
            Add<String,ElaStringBuilder,ElaStringBuilder>("appendLine", Append);
            Add<Int32,String,ElaStringBuilder,ElaStringBuilder>("insert", Insert);
            Add<Int32,Char,ElaStringBuilder,ElaStringBuilder>("insertChar", InsertChar);
            Add<String,String,ElaStringBuilder,ElaStringBuilder>("Replace", Replace);
            Add<Char,Char,ElaStringBuilder,ElaStringBuilder>("replaceChar", ReplaceChar);
            Add<Int32,Int32,ElaStringBuilder,ElaStringBuilder>("remove", Remove);
        }


        public ElaStringBuilder CreateBuilder(ElaValue val)
        {
            var init = val.TypeCode != ElaTypeCode.Unit ?
                val.ToString() : String.Empty;
            return new ElaStringBuilder(new StringBuilder(init));
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
