using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI;

internal interface IDictionaryElementWrapper
{
	object Key { get; }
	object Value { get; }

	/*
	public UIModConfigDictionaryElementWrapper() { }

	internal UIModConfigDictionaryItem parent;

	public UIModConfigDictionaryElementWrapper(object key, object value, UIModConfigDictionaryItem parent)
	{
		this.key = key;
		this.value = value;
		this.parent = parent;
	}
	*/
}

internal class DictionaryElementWrapper<K, V> : IDictionaryElementWrapper
{
	private readonly IDictionary dictionary;

	private K _key;
	private V _value;

	public K Key {
		get => _key;
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

	public V Value {
		get => _value;
		set {
			dictionary[Key] = value;
			_value = value;
		}
	}

	object IDictionaryElementWrapper.Key => Key;
	object IDictionaryElementWrapper.Value => Value;

	//internal UIModConfigDictionaryItem parent;

	public DictionaryElementWrapper(K key, V value, IDictionary dictionary)
	{ //, UIModConfigDictionaryItem parent)
		this.dictionary = dictionary;
		_key = key;
		_value = value;
		//this.parent = parent;
	}
}

/*
internal class UIModConfigDictionaryElementWrapperWrapper
{
	public dynamic w;
	public UIModConfigDictionaryElementWrapperWrapper(dynamic w) {
		this.w = w;
	}
}
*/

internal class DictionaryElement : CollectionElement
{
	internal Type keyType;
	internal Type valueType;
	internal UIText save;
	public List<IDictionaryElementWrapper> dataWrapperList;

	// These 2 hold the default value of the dictionary value, hence ValueValue
	protected DefaultDictionaryKeyValueAttribute defaultDictionaryKeyValueAttribute;
	protected JsonDefaultDictionaryKeyValueAttribute jsonDefaultDictionaryKeyValueAttribute;

	protected override void PrepareTypes()
	{
		keyType = MemberInfo.Type.GetGenericArguments()[0];
		valueType = MemberInfo.Type.GetGenericArguments()[1];
		JsonDefaultListValueAttribute = ConfigManager.GetCustomAttributeFromCollectionMemberThenElementType<JsonDefaultListValueAttribute>(MemberInfo.MemberInfo, valueType);
		defaultDictionaryKeyValueAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<DefaultDictionaryKeyValueAttribute>(MemberInfo, null, null);
		jsonDefaultDictionaryKeyValueAttribute = ConfigManager.GetCustomAttributeFromMemberThenMemberType<JsonDefaultDictionaryKeyValueAttribute>(MemberInfo, null, null);
	}

	protected override void AddItem()
	{
		try {
			object keyValue;

			if (defaultDictionaryKeyValueAttribute != null) {
				keyValue = defaultDictionaryKeyValueAttribute.Value;
			}
			else {
				keyValue = ConfigManager.AlternateCreateInstance(keyType);

				if (!keyType.IsValueType && keyType != typeof(string)) {
					string json = jsonDefaultDictionaryKeyValueAttribute?.Json ?? "{}";

					JsonConvert.PopulateObject(json, keyValue, ConfigManager.serializerSettings);
				}
			}

			((IDictionary)Data).Add(keyValue, CreateCollectionElementInstance(valueType));
		}
		catch (Exception e) {
			Interface.modConfig.SetMessage("Error: " + e.Message, Color.Red);
		}
	}

	protected override void InitializeCollection()
	{
		Data = Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
		SetObject(Data);
	}

	protected override void ClearCollection()
	{
		((IDictionary)Data).Clear();
	}

	protected override void SetupList()
	{
		DataList.Clear();
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

		if (Data != null) {
			var keys = ((IDictionary)Data).Keys;
			var values = ((IDictionary)Data).Values;
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

				var proxy = (IDictionaryElementWrapper)Activator.CreateInstance(
					genericType,
					new object[] { keysEnumerator.Current, valuesEnumerator.Current, (IDictionary)Data }
				);

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

				Type itemType = MemberInfo.Type.GetGenericArguments()[0];
				var wrappermemberInfo = ConfigManager.GetFieldsAndProperties(this).ToList()[0];
				int index = i;
				//TODO: Sometime key is below value for some reason. IntFloatDictionary.
				var wrapped = UIModConfig.WrapIt(DataList, ref top, wrappermemberInfo, this, 0, dataWrapperList, genericType, i);
				//var wrapped = UIModConfig.WrapIt(dataList, ref top, wrappermemberInfo, wrapperwrapper, ref sliderID);

				// Save wrap, pre save check?
				wrapped.Item2.Left.Pixels += 24;
				wrapped.Item2.Width.Pixels -= 24;

				// Add delete button.
				UIModConfigHoverImage deleteButton = new UIModConfigHoverImage(DeleteTexture, Language.GetTextValue("tModLoader.ModConfigRemove"));
				deleteButton.VAlign = 0.5f;

				// fix delete.
				object o = keysEnumerator.Current;

				deleteButton.OnLeftClick += (a, b) => {
					((IDictionary)Data).Remove(o);
					SetupList();
					Interface.modConfig.SetPendingChanges();
				};

				wrapped.Item1.Append(deleteButton);

				i++;
			}
		}
	}
}
