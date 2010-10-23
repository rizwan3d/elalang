
using System;
using Ela;
using Ela.Runtime;
using Ela.Runtime.ObjectModel;
		
namespace Ela.StandardLibrary.Modules 
{
	
partial class CollectionModule : Ela.Linking.ForeignModule
{
	public override void Initialize()
	{
	
		Add("where", new _elafunc_Where(this));
	
		Add("arrayToList", new _elafunc_ArrayToList(this));
	
		Add("distinct", new _elafunc_Distinct(this));
	
		Add("length", new _elafunc_Length(this));
	
		Add("listItem", new _elafunc_ListItem(this));
	
		Add("listLength", new _elafunc_ListLength(this));
	
		Add("listToArray", new _elafunc_ListToArray(this));
	
		Add("reverse", new _elafunc_Reverse(this));
	
		Add("reverseList", new _elafunc_ReverseList(this));
	
		Add("reverseArray", new _elafunc_ReverseArray(this));
	
		Add("sortArray", new _elafunc_SortArray(this));
	
		Add("sortList", new _elafunc_SortList(this));
	
	}

	
	class _elafunc_Where : ElaFunction
	{
		private CollectionModule obj;
		internal _elafunc_Where(CollectionModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Where(
					
								args[0].ToSequence()
							,
								args[1].ToFunction()
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_ArrayToList : ElaFunction
	{
		private CollectionModule obj;
		internal _elafunc_ArrayToList(CollectionModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.ArrayToList(
					
								args[0].ToArray()
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Distinct : ElaFunction
	{
		private CollectionModule obj;
		internal _elafunc_Distinct(CollectionModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Distinct(
					
								args[0].ToSequence()
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Length : ElaFunction
	{
		private CollectionModule obj;
		internal _elafunc_Length(CollectionModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Length(
					
								args[0].ToSequence()
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_ListItem : ElaFunction
	{
		private CollectionModule obj;
		internal _elafunc_ListItem(CollectionModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.ListItem(
					
								args[0].ToList()
							,
								args[1].ToInt32(null)
							
					);					
					
         return
       	ret;
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_ListLength : ElaFunction
	{
		private CollectionModule obj;
		internal _elafunc_ListLength(CollectionModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.ListLength(
					
								args[0].ToList()
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_ListToArray : ElaFunction
	{
		private CollectionModule obj;
		internal _elafunc_ListToArray(CollectionModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.ListToArray(
					
								args[0].ToList()
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Reverse : ElaFunction
	{
		private CollectionModule obj;
		internal _elafunc_Reverse(CollectionModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Reverse(
					
								(ElaObject)args[0].ToObject()
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_ReverseList : ElaFunction
	{
		private CollectionModule obj;
		internal _elafunc_ReverseList(CollectionModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.ReverseList(
					
								args[0].ToList()
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_ReverseArray : ElaFunction
	{
		private CollectionModule obj;
		internal _elafunc_ReverseArray(CollectionModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.ReverseArray(
					
								args[0].ToArray()
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_SortArray : ElaFunction
	{
		private CollectionModule obj;
		internal _elafunc_SortArray(CollectionModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.SortArray(
					
								args[0].ToArray()
							,
								args[1].ToFunction()
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_SortList : ElaFunction
	{
		private CollectionModule obj;
		internal _elafunc_SortList(CollectionModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.SortList(
					
								args[0].ToList()
							,
								args[1].ToFunction()
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
}
}
			
namespace Ela.StandardLibrary.Modules 
{
	
partial class StringModule : Ela.Linking.ForeignModule
{
	public override void Initialize()
	{
	
		Add("format", new _elafunc_Format(this));
	
		Add("upper", new _elafunc_ToUpper(this));
	
		Add("lower", new _elafunc_ToLower(this));
	
		Add("indexOf", new _elafunc_IndexOf(this));
	
		Add("indexOfFrom", new _elafunc_IndexOf2(this));
	
		Add("lastIndexOf", new _elafunc_LastIndexOf(this));
	
		Add("indexOfAny", new _elafunc_IndexOfAny(this));
	
		Add("indexOfAnyFrom", new _elafunc_IndexOfAnyFrom(this));
	
		Add("trim", new _elafunc_Trim(this));
	
		Add("trimStart", new _elafunc_TrimStart(this));
	
		Add("trimEnd", new _elafunc_TrimEnd(this));
	
		Add("trimChars", new _elafunc_TrimChars(this));
	
		Add("trimStartChars", new _elafunc_TrimStartChars(this));
	
		Add("trimEndChars", new _elafunc_TrimEndChars(this));
	
		Add("startsWith", new _elafunc_StartsWith(this));
	
		Add("endsWith", new _elafunc_EndsWith(this));
	
		Add("replace", new _elafunc_Replace(this));
	
		Add("remove", new _elafunc_Remove(this));
	
		Add("split", new _elafunc_Split(this));
	
		Add("substr", new _elafunc_Substring(this));
	
		Add("splitToList", new _elafunc_SplitToList(this));
	
		Add("insert", new _elafunc_Insert(this));
	
		Add("padRight", new _elafunc_PadRight(this));
	
		Add("padLeft", new _elafunc_PadLeft(this));
	
	}

	
	class _elafunc_Format : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_Format(StringModule obj) : base(-1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Format(
					
                args
              
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_ToUpper : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_ToUpper(StringModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.ToUpper(
					
								args[0].ToString(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_ToLower : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_ToLower(StringModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.ToLower(
					
								args[0].ToString(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_IndexOf : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_IndexOf(StringModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.IndexOf(
					
								args[0].ToString(null)
							,
								args[1].ToString(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_IndexOf2 : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_IndexOf2(StringModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.IndexOf2(
					
								args[0].ToInt32(null)
							,
								args[1].ToString(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_LastIndexOf : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_LastIndexOf(StringModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.LastIndexOf(
					
								args[0].ToString(null)
							,
								args[1].ToString(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_IndexOfAny : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_IndexOfAny(StringModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		private Char[] ConvertTo_2(ElaArray array)
		{
			var arr = new Char[array.Length];

			for (var i = 0; i < array.Length; i++)
          arr[i] = array[i].ToChar(null);
        
    return arr;
    }
  
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.IndexOfAny(
					
								args[0].ToString(null)
							,
								ConvertTo_2(args[1].ToArray())
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_IndexOfAnyFrom : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_IndexOfAnyFrom(StringModule obj) : base(3)
		{
			this.obj = obj;
		}
		
		
		private Char[] ConvertTo_3(ElaArray array)
		{
			var arr = new Char[array.Length];

			for (var i = 0; i < array.Length; i++)
          arr[i] = array[i].ToChar(null);
        
    return arr;
    }
  
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.IndexOfAnyFrom(
					
								args[0].ToString(null)
							,
								args[1].ToInt32(null)
							,
								ConvertTo_3(args[2].ToArray())
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Trim : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_Trim(StringModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Trim(
					
								args[0].ToString(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_TrimStart : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_TrimStart(StringModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.TrimStart(
					
								args[0].ToString(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_TrimEnd : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_TrimEnd(StringModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.TrimEnd(
					
								args[0].ToString(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_TrimChars : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_TrimChars(StringModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		private Char[] ConvertTo_2(ElaArray array)
		{
			var arr = new Char[array.Length];

			for (var i = 0; i < array.Length; i++)
          arr[i] = array[i].ToChar(null);
        
    return arr;
    }
  
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.TrimChars(
					
								args[0].ToString(null)
							,
								ConvertTo_2(args[1].ToArray())
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_TrimStartChars : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_TrimStartChars(StringModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		private Char[] ConvertTo_2(ElaArray array)
		{
			var arr = new Char[array.Length];

			for (var i = 0; i < array.Length; i++)
          arr[i] = array[i].ToChar(null);
        
    return arr;
    }
  
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.TrimStartChars(
					
								args[0].ToString(null)
							,
								ConvertTo_2(args[1].ToArray())
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_TrimEndChars : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_TrimEndChars(StringModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		private Char[] ConvertTo_2(ElaArray array)
		{
			var arr = new Char[array.Length];

			for (var i = 0; i < array.Length; i++)
          arr[i] = array[i].ToChar(null);
        
    return arr;
    }
  
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.TrimEndChars(
					
								args[0].ToString(null)
							,
								ConvertTo_2(args[1].ToArray())
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_StartsWith : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_StartsWith(StringModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.StartsWith(
					
								args[0].ToString(null)
							,
								args[1].ToString(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_EndsWith : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_EndsWith(StringModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.EndsWith(
					
								args[0].ToString(null)
							,
								args[1].ToString(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Replace : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_Replace(StringModule obj) : base(3)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Replace(
					
								args[0].ToString(null)
							,
								args[1].ToString(null)
							,
								args[2].ToString(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Remove : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_Remove(StringModule obj) : base(3)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Remove(
					
								args[0].ToString(null)
							,
								args[1].ToInt32(null)
							,
								args[2].ToInt32(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Split : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_Split(StringModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		private RuntimeValue ConvertFromArray(String[] arr)
		{
			var array = new ElaArray(arr.Length);

			for (var i = 0; i < arr.Length; i++)
				array.Add(new RuntimeValue(arr[i]));

			return new RuntimeValue(array);
		}		
	
		private String[] ConvertTo_2(ElaArray array)
		{
			var arr = new String[array.Length];

			for (var i = 0; i < array.Length; i++)
          arr[i] = array[i].ToString(null);
        
    return arr;
    }
  
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Split(
					
								args[0].ToString(null)
							,
								ConvertTo_2(args[1].ToArray())
							
					);					
					
         return
       	ConvertFromArray(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Substring : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_Substring(StringModule obj) : base(3)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Substring(
					
								args[0].ToString(null)
							,
								args[1].ToInt32(null)
							,
								args[2].ToInt32(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_SplitToList : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_SplitToList(StringModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		private String[] ConvertTo_2(ElaArray array)
		{
			var arr = new String[array.Length];

			for (var i = 0; i < array.Length; i++)
          arr[i] = array[i].ToString(null);
        
    return arr;
    }
  
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.SplitToList(
					
								args[0].ToString(null)
							,
								ConvertTo_2(args[1].ToArray())
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Insert : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_Insert(StringModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Insert(
					
								args[0].ToString(null)
							,
								args[1].ToInt32(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_PadRight : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_PadRight(StringModule obj) : base(3)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.PadRight(
					
								args[0].ToString(null)
							,
								args[1].ToChar(null)
							,
								args[2].ToInt32(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_PadLeft : ElaFunction
	{
		private StringModule obj;
		internal _elafunc_PadLeft(StringModule obj) : base(3)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.PadLeft(
					
								args[0].ToString(null)
							,
								args[1].ToChar(null)
							,
								args[2].ToInt32(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
}
}
			
namespace Ela.StandardLibrary.Modules 
{
	
partial class CharModule : Ela.Linking.ForeignModule
{
	public override void Initialize()
	{
	
		Add("isLower", new _elafunc_IsLower(this));
	
		Add("isUpper", new _elafunc_IsUpper(this));
	
		Add("upper", new _elafunc_ToUpper(this));
	
		Add("lower", new _elafunc_ToLower(this));
	
	}

	
	class _elafunc_IsLower : ElaFunction
	{
		private CharModule obj;
		internal _elafunc_IsLower(CharModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.IsLower(
					
								args[0].ToChar(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_IsUpper : ElaFunction
	{
		private CharModule obj;
		internal _elafunc_IsUpper(CharModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.IsUpper(
					
								args[0].ToChar(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_ToUpper : ElaFunction
	{
		private CharModule obj;
		internal _elafunc_ToUpper(CharModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.ToUpper(
					
								args[0].ToChar(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_ToLower : ElaFunction
	{
		private CharModule obj;
		internal _elafunc_ToLower(CharModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.ToLower(
					
								args[0].ToChar(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
}
}
			
namespace Ela.StandardLibrary.Modules 
{
	
partial class DebugModule : Ela.Linking.ForeignModule
{
	public override void Initialize()
	{
	
		Add("print", new _elafunc_Print(this));
	
		Add("getArray", new _elafunc_GetArray(this));
	
		Add("getList", new _elafunc_GetList(this));
	
		Add("startClock", new _elafunc_StartClock(this));
	
		Add("stopClock", new _elafunc_StopClock(this));
	
	}

	
	class _elafunc_Print : ElaFunction
	{
		private DebugModule obj;
		internal _elafunc_Print(DebugModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
        
          obj.Print(
					args[0]
					);					
					
         return
       	new RuntimeValue(ElaObject.Unit);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_GetArray : ElaFunction
	{
		private DebugModule obj;
		internal _elafunc_GetArray(DebugModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.GetArray(
					
								args[0].ToInt32(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_GetList : ElaFunction
	{
		private DebugModule obj;
		internal _elafunc_GetList(DebugModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.GetList(
					
								args[0].ToInt32(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_StartClock : ElaFunction
	{
		private DebugModule obj;
		internal _elafunc_StartClock(DebugModule obj) : base(0)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.StartClock(
					
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_StopClock : ElaFunction
	{
		private DebugModule obj;
		internal _elafunc_StopClock(DebugModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.StopClock(
					
								args[0].ToInt64(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
}
}
			
namespace Ela.StandardLibrary.Modules 
{
	
partial class MathModule : Ela.Linking.ForeignModule
{
	public override void Initialize()
	{
	
		Add("rnd", new _elafunc_Randomize(this));
	
		Add("acos", new _elafunc_Acos(this));
	
		Add("asin", new _elafunc_Asin(this));
	
		Add("atan", new _elafunc_Atan(this));
	
		Add("atan2", new _elafunc_Atan2(this));
	
		Add("ceil", new _elafunc_Ceil(this));
	
		Add("cos", new _elafunc_Cos(this));
	
		Add("cosh", new _elafunc_Cosh(this));
	
		Add("exp", new _elafunc_Exp(this));
	
		Add("floor", new _elafunc_Floor(this));
	
		Add("log", new _elafunc_Log(this));
	
		Add("log2", new _elafunc_Log2(this));
	
		Add("log10", new _elafunc_Log10(this));
	
		Add("round", new _elafunc_Round(this));
	
		Add("sin", new _elafunc_Sin(this));
	
		Add("sinh", new _elafunc_Sinh(this));
	
		Add("sqrt", new _elafunc_Sqrt(this));
	
		Add("tan", new _elafunc_Tan(this));
	
		Add("tanh", new _elafunc_Tanh(this));
	
		Add("truc", new _elafunc_Truncate(this));
	
	}

	
	class _elafunc_Randomize : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Randomize(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Randomize(
					
								args[0].ToInt32(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Acos : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Acos(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Acos(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Asin : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Asin(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Asin(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Atan : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Atan(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Atan(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Atan2 : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Atan2(MathModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Atan2(
					
								args[0].ToDouble(null)
							,
								args[1].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Ceil : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Ceil(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Ceil(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Cos : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Cos(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Cos(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Cosh : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Cosh(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Cosh(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Exp : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Exp(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Exp(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Floor : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Floor(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Floor(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Log : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Log(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Log(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Log2 : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Log2(MathModule obj) : base(2)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Log2(
					
								args[0].ToDouble(null)
							,
								args[1].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Log10 : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Log10(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Log10(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Round : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Round(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Round(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Sin : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Sin(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Sin(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Sinh : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Sinh(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Sinh(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Sqrt : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Sqrt(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Sqrt(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Tan : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Tan(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Tan(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Tanh : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Tanh(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Tanh(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
	class _elafunc_Truncate : ElaFunction
	{
		private MathModule obj;
		internal _elafunc_Truncate(MathModule obj) : base(1)
		{
			this.obj = obj;
		}
		
		
		
		public override RuntimeValue Call(params RuntimeValue[] args)
		{
			try
			{
          
            var ret =
          
        
          obj.Truncate(
					
								args[0].ToDouble(null)
							
					);					
					
         return
       	new RuntimeValue(ret);
			}
			catch (ElaCastException ex)
			{
				if (ex.ExpectedType != ObjectType.None)
					throw new ElaParameterTypeException(ex.ExpectedType, ex.InvalidType);
				else
					throw new ElaParameterTypeException(ex.InvalidType);
			}
		}
	}
	
}
}
			