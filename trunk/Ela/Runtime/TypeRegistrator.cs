using System;

namespace Ela.Runtime
{
	public sealed class TypeRegistrator
	{
		private readonly ElaMachine vm;

		internal TypeRegistrator(ElaMachine vm)
		{
			this.vm = vm;
		}

		public TypeId ObtainTypeId(string tag)
		{
			var ret = new TypeId(vm.typeCount);
			
			try
			{
				vm.typeIdMap.Add(tag, vm.typeCount);
			}
			catch (Exception)
			{
				throw new Exception("Conflicting implicit tag " + tag);
			}
			
			vm.typeCount++;
			return ret;
		}
	}
}
