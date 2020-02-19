using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI
{
	internal abstract class IDictionaryElementWrapper
	{
		//public UIModConfigDictionaryElementWrapper() { }
		internal virtual object Key => null;
		internal virtual object Value => null;
		//internal UIModConfigDictionaryItem parent;
		//public UIModConfigDictionaryElementWrapper(object key, object value, UIModConfigDictionaryItem parent)
		//{
		//	this.key = key;
		//	this.value = value;
		//	this.parent = parent;
		//}
	}

	internal class DictionaryElementWrapper<K, V> : IDictionaryElementWrapper
	{
		private IDictionary dictionary;
		//private object _key;

		//internal override object Key
		//{
		//	get { return _key; }
		//	set {
		//		dictionary.Remove(key);
		//		_key = value as K;
		//	}
		//}

		private K _key;
		public K key {
			get { return _key; }
			set {
				if (dictionary.Contains(value)) {

				}
				else {
					dictionary.Remove(_key);
					_key = value;
					dictionary.Add(_key, _value);
				}
			}
		}

		private V _value;
		public V value {
			get { return _value; }
			set {
				dictionary[key] = value;
				_value = value;
			}
		}

		//public K key;
		//public V value;
		internal override object Key => key;
		internal override object Value => value;
		//internal UIModConfigDictionaryItem parent;
		public DictionaryElementWrapper(K key, V value, IDictionary dictionary) //, UIModConfigDictionaryItem parent)
		{
			this.dictionary = dictionary;
			this._key = key;
			this._value = value;
			//this.parent = parent;
		}
	}

	//internal class UIModConfigDictionaryElementWrapperWrapper
	//{
	//	public dynamic w;
	//	public UIModConfigDictionaryElementWrapperWrapper(dynamic w)
	//	{
	//		this.w = w;
	//	}
	//}

	internal class DictionaryElement : CollectionElement
	{
		internal Type keyType;
		internal Type valueType;
		internal UIText save;
		public List<IDictionaryElementWrapper> dataWrapperList;

		// These 2 hold the default value of the dictionary value, hence ValueValue
		protected DefaultDictionaryKeyValueAttribute defaultDictionaryKeyValueAttribute;
		protected JsonDefaultDictionaryKeyValueAttribute jsonDefaultDictionaryKeyValueAttribute;

		protected override void PrepareTypes() {
			keyType = memberInfo.Type.GetGenericArguments()[0];
			valueType = memberInfo.Type.GetGenericArguments()[1];
			jsonDefaultListValueAttribute = ConfigManager.GetCustomAttribute<JsonDefaultListValueAttribute>(memberInfo, valueType);
			defaultDictionaryKeyValueAttribute = ConfigManager.GetCustomAttribute<DefaultDictionaryKeyValueAttribute>(memberInfo, null, null);
			jsonDefaultDictionaryKeyValueAttribute = ConfigManager.GetCustomAttribute<JsonDefaultDictionaryKeyValueAttribute>(memberInfo, null, null);
		}

		protected override void AddItem() {
			try {
				object keyValue;
				if (defaultDictionaryKeyValueAttribute != null) {
					keyValue = defaultDictionaryKeyValueAttribute.Value;
				}
				else {
					keyValue = ConfigManager.AlternateCreateInstance(keyType);
					if (!keyType.IsValueType && keyType != typeof(string)) {
						string json = jsonDefaultDictionaryKeyValueAttribute?.json ?? "{}";
						JsonConvert.PopulateObject(json, keyValue, ConfigManager.serializerSettings);
					}
				}
				((IDictionary)data).Add(keyValue, CreateCollectionElementInstance(valueType));
			}
			catch (Exception e) {
				Interface.modConfig.SetMessage("Error: " + e.Message, Color.Red);
			}
		}

		protected override void InitializeCollection() {
			data = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
			SetObject(data);
		}

		protected override void ClearCollection() {
			((IDictionary)data).Clear();
		}

		protected override void SetupList() {
			dataList.Clear();
			int top = 0;
			dataWrapperList = new List<IDictionaryElementWrapper>();

			//var genericListType = typeof(List<>);
			//var specificListType = genericListType.MakeGenericType(typeof(double));
			//var list = Activator.CreateInstance(specificListType);

			//var listType = typeof(List<>);
			//var constructedListType = listType.MakeGenericType(keyType);
			//var keyList = (IList)Activator.CreateInstance(constructedListType);

			//foreach (var item in ((IDictionary)data))
			//{
			//	//var wrapped = UIModConfig.WrapIt(dataList, ref top, memberInfo, item, ref sliderID, keys, keyType, i2);
			//	keyList.Add(item);
			//	//i2++;
			//}
			//((IDictionary)data).

			//string elementTypeName = Console.ReadLine();
			//Type elementType = Type.GetType(elementTypeName);
			//Type[] types = new Type[] { elementType };

			//Type listType = memberInfo.Type.GetGenericArguments()[0];
			//Type listType = typeof(Dictionary<>);
			//Type genericType = listType.MakeGenericType(types);
			//IProxy proxy = (IProxy)Activator.CreateInstance(genericType);

			//Type genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
			Type genericType = typeof(DictionaryElementWrapper<,>).MakeGenericType(keyType, valueType);

			if (data != null) {
				var keys = ((IDictionary)data).Keys;
				var values = ((IDictionary)data).Values;
				var keysEnumerator = keys.GetEnumerator();
				var valuesEnumerator = values.GetEnumerator();
				int i = 0;
				while (keysEnumerator.MoveNext()) {

					valuesEnumerator.MoveNext();
					//var wrapper = new UIModConfigDictionaryElementWrapper<typeof(keysEnumerator.Current), typeof(keysEnumerator.Current)>(keysEnumerator.Current, valuesEnumerator.Current, this);
					//dynamic sampleObject = new ExpandoObject();
					//sampleObject.key = keysEnumerator.Current;
					//sampleObject.value = valuesEnumerator.Current;
					//var wrapperwrapper = new UIModConfigDictionaryElementWrapperWrapper(sampleObject);

					IDictionaryElementWrapper proxy = (IDictionaryElementWrapper)Activator.CreateInstance(genericType,
						new object[] { keysEnumerator.Current, valuesEnumerator.Current, (IDictionary)data });
					dataWrapperList.Add(proxy);
					//var v = new { Key = keysEnumerator.Current, Value = valuesEnumerator.Current };  

					//dataWrapperList.Add(wrapper);
					//}

					//var wrapperwrapper = new UIModConfigDictionaryElementWrapperWrapper(v);

					//	var keys = ((IDictionary)data).Keys.ToList();
					//var values = ((IDictionary)data).Values.ToList();
					//for (int i = 0; i < ((IDictionary)data).Count; i++)
					//{
					//((IDictionary)data).
					//	int index = i;
					//((IDictionary)data).
					//Type tupleType = typeof(Tuple<,>);
					//	var wrapper = new UIModConfigDictionaryElementWrapper(((IDictionary)data)[], , this);
					Type itemType = memberInfo.Type.GetGenericArguments()[0];
					var wrappermemberInfo = ConfigManager.GetFieldsAndProperties(this).ToList()[0];
					int index = i;
					var wrapped = UIModConfig.WrapIt(dataList, ref top, wrappermemberInfo, this, 0, dataWrapperList, genericType, i); // TODO: Sometime key is below value for some reason. IntFloatDictionary.
																														//var wrapped = UIModConfig.WrapIt(dataList, ref top, wrappermemberInfo, wrapperwrapper, ref sliderID);
																														 // save wrap, pre save check?
					wrapped.Item2.Left.Pixels += 24;
					wrapped.Item2.Width.Pixels -= 24;

					// Add delete button.
					UIModConfigHoverImage deleteButton = new UIModConfigHoverImage(deleteTexture, "Remove");
					deleteButton.VAlign = 0.5f;

					// fix delete.
					object o = keysEnumerator.Current;
					deleteButton.OnClick += (a, b) => {
						((IDictionary)data).Remove(o);
						SetupList();
						Interface.modConfig.SetPendingChanges();
					};
					wrapped.Item1.Append(deleteButton);

					i++;
				}
			}
		}
	}
}
