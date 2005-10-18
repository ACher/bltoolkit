using System;
using System.Reflection;

namespace BLToolkit.TypeBuilder
{
	class FakeGetter : FakeMethodInfo
	{
		public FakeGetter(PropertyInfo propertyInfo) 
			: base(propertyInfo, propertyInfo.GetSetMethod(true))
		{
		}

		public override ParameterInfo[] GetParameters()
		{
			return _property.GetIndexParameters();
		}

		public override string Name
		{
			get { return "get_" + _property.Name; }
		}

		public override Type ReturnType
		{
			get { return typeof(void); }
		}
	}
}
