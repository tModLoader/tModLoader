using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Terraria.ModLoader.Setup.Formatting
{
	// Copied from vanilla, slightly changed to more easily work with.
	public class IdDictionary
	{
		private Dictionary<string, int> _nameToId = new Dictionary<string, int>();

		private Dictionary<int, string> _idToName = new Dictionary<int, string>(); // added

		public readonly int Count;

		internal IdDictionary(int count) { // made internal 
			Count = count;
		}

		public bool TryGetName(int id, out string name) {
			return _idToName.TryGetValue(id, out name);
		}

		public bool TryGetId(string name, out int id) {
			return _nameToId.TryGetValue(name, out id);
		}

		public bool ContainsName(string name) {
			return _nameToId.ContainsKey(name);
		}

		public bool ContainsId(int id) {
			return _idToName.ContainsKey(id);
		}

		public string GetName(int id) {
			return _idToName[id];
		}

		public int GetId(string name) {
			return _nameToId[name];
		}

		public void Add(string name, int id) {
			_idToName.Add(id, name);
			_nameToId.Add(name, id);
		}

		public void Remove(string name) {
			_idToName.Remove(_nameToId[name]);
			_nameToId.Remove(name);
		}

		public void Remove(int id) {
			_nameToId.Remove(_idToName[id]);
			_idToName.Remove(id);
		}

		public static IdDictionary Create(Type idClass, Type idType) {
			int count = int.MaxValue;
			FieldInfo fieldInfo = idClass.GetRuntimeFields().FirstOrDefault((FieldInfo field) => field.Name == "Count");
			if (fieldInfo != null) {
				count = Convert.ToInt32(fieldInfo.GetValue(null));
			}
			IdDictionary dictionary = new IdDictionary(count);
			//(from f in idClass.GetRuntimeFields(BindingFlags.Static | BindingFlags.Public)
			(from f in idClass.GetRuntimeFields()
			 where f.FieldType == idType
			 select f).ToList().ForEach(delegate (FieldInfo field) {
				 int num = Convert.ToInt32(field.GetValue(null));
				 if (num < dictionary.Count) {
					 dictionary._nameToId.Add(field.Name, num);
				 }
			 });
			dictionary._idToName = dictionary._nameToId.ToDictionary((KeyValuePair<string, int> kp) => kp.Value, (KeyValuePair<string, int> kp) => kp.Key);
			return dictionary;
		}

		public static IdDictionary Create<IdClass, IdType>() {
			return Create(typeof(IdClass), typeof(IdType));
		}
	}
}
