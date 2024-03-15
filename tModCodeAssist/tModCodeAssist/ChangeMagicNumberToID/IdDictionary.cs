using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ReLogic.Reflection;

public sealed class IdDictionary
{
	private readonly Dictionary<int, List<string>> _idToNames = [];
	public readonly int Count;

	private IdDictionary(int count)
	{
		Count = count;
	}

	public bool TryGetNames(int id, out List<string> names) => _idToNames.TryGetValue(id, out names);

	public static IdDictionary Create(Type idClass, Type idType)
	{
		int num = int.MaxValue;
		FieldInfo fieldInfo = idClass.GetField("Count");
		if (fieldInfo != null) {
			num = Convert.ToInt32(fieldInfo.GetValue(null));
			if (num == 0)
				throw new Exception("IdDictionary cannot be created before Count field is initialized. Move to bottom of static class");
		}

		IdDictionary dictionary = new IdDictionary(num);
		(from f in idClass.GetFields(BindingFlags.Static | BindingFlags.Public)
		 where f.FieldType == idType
		 where f.GetCustomAttribute<ObsoleteAttribute>() == null
		 select f).ToList().ForEach(delegate (FieldInfo field) {
			 int num2 = Convert.ToInt32(field.GetValue(null));
			 if (num2 < dictionary.Count) {
				 if (dictionary._idToNames.TryGetValue(num2, out var names))
					 names.Add(field.Name);
				 else
					 dictionary._idToNames[num2] = [field.Name];
			 }
		 });

		return dictionary;
	}

	public static IdDictionary Create<IdClass, IdType>() => Create(typeof(IdClass), typeof(IdType));
}
