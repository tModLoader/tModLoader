using System;
using System.Reflection;

namespace Terraria.ModLoader.Config.UI;

public class PropertyFieldWrapper
{
	private readonly FieldInfo fieldInfo;
	private readonly PropertyInfo propertyInfo;

	public bool IsField => fieldInfo != null;
	public bool IsProperty => propertyInfo != null;
	public MemberInfo MemberInfo => fieldInfo ?? (MemberInfo)propertyInfo;
	public string Name => fieldInfo?.Name ?? propertyInfo.Name;
	public Type Type => fieldInfo?.FieldType ?? propertyInfo.PropertyType;
	public bool IsFieldOfAnEnum => Type.IsEnum && (fieldInfo?.DeclaringType.IsEnum ?? false);

	public PropertyFieldWrapper(FieldInfo fieldInfo)
	{
		this.fieldInfo = fieldInfo;
	}

	public PropertyFieldWrapper(PropertyInfo propertyInfo)
	{
		this.propertyInfo = propertyInfo;
	}

	public object GetValue(object obj)
	{
		if (fieldInfo != null)
			return fieldInfo.GetValue(obj);
		else
			return propertyInfo.GetValue(obj, null);
	}

	public void SetValue(object obj, object value)
	{
		if (fieldInfo != null)
			fieldInfo.SetValue(obj, value);
		else {
			if (propertyInfo.CanWrite) // TODO: Grey out?
				propertyInfo.SetValue(obj, value, null);
		}
	}

	public bool CanWrite => fieldInfo != null || propertyInfo.CanWrite;
}
