using System;
using System.Reflection;

namespace Terraria.ModLoader.Config.UI
{
	public class PropertyFieldWrapper
	{
		private FieldInfo fieldInfo;
		private PropertyInfo propertyInfo;

		public PropertyFieldWrapper(FieldInfo fieldInfo)
		{
			this.fieldInfo = fieldInfo;
		}

		public PropertyFieldWrapper(PropertyInfo propertyInfo)
		{
			this.propertyInfo = propertyInfo;
		}

		public bool isField => fieldInfo != null;
		public bool isProperty => propertyInfo != null;

		public MemberInfo MemberInfo => fieldInfo != null ? fieldInfo : (MemberInfo)propertyInfo;

		public string Name => fieldInfo?.Name ?? propertyInfo.Name;

		public Type Type => fieldInfo?.FieldType ?? propertyInfo.PropertyType;

		public object GetValue(Object obj)
		{
			if (fieldInfo != null)
				return fieldInfo.GetValue(obj);
			else
				return propertyInfo.GetValue(obj, null);
		}

		public void SetValue(Object obj, object value)
		{
			if (fieldInfo != null)
				fieldInfo.SetValue(obj, value);
			else
			{
				if (propertyInfo.CanWrite) // TODO: Grey out?
					propertyInfo.SetValue(obj, value, null);
			}
		}

		public bool CanWrite => fieldInfo != null ? true : propertyInfo.CanWrite;
	}
}
