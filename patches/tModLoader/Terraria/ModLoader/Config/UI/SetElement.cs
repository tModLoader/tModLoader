using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI;

internal interface ISetElementWrapper
{
	object Value { get; }
}

// TODO: Somehow prevent this class from displaying?
internal class SetElementWrapper<V> : ISetElementWrapper
{
	private readonly object set;

	private V _value;

	public V Value {
		get => _value;
		set {
			var removeMethod = set.GetType().GetMethods().FirstOrDefault(m => m.Name == "Remove");
			var addMethod = set.GetType().GetMethods().FirstOrDefault(m => m.Name == "Add");
			var containsMethod = set.GetType().GetMethods().FirstOrDefault(m => m.Name == "Contains");

			if ((bool)containsMethod.Invoke(set, new object[] { value })) {
			}
			else {
				removeMethod.Invoke(set, new object[] { _value });
				_value = value;
				addMethod.Invoke(set, new object[] { _value });
			}

			/*
			if (set.Contains(value)) {

			}
			else {
				set.Remove(_value);
				_value = value;
				set.Add(_value, _value);
			}
			*/
		}
	}

	object ISetElementWrapper.Value => Value;

	public SetElementWrapper(V value, object set)
	{
		this.set = set;
		_value = value;
	}
}

internal class SetElement : CollectionElement
{
	private Type setType;

	public List<ISetElementWrapper> DataWrapperList { get; set; }

	protected override void PrepareTypes()
	{
		setType = MemberInfo.Type.GetGenericArguments()[0];
		JsonDefaultListValueAttribute = ConfigManager.GetCustomAttributeFromCollectionMemberThenElementType<JsonDefaultListValueAttribute>(MemberInfo.MemberInfo, setType);
	}

	protected override void AddItem()
	{
		var addMethod = Data.GetType().GetMethods().FirstOrDefault(m => m.Name == "Add");
		addMethod.Invoke(Data, new object[] { CreateCollectionElementInstance(setType) });
	}

	protected override void InitializeCollection()
	{
		Data = Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(setType));
		SetObject(Data);
	}

	protected override void ClearCollection()
	{
		var clearMethod = Data.GetType().GetMethods().FirstOrDefault(m => m.Name == "Clear");
		clearMethod.Invoke(Data, new object[] { });
	}

	protected override void SetupList()
	{
		DataList.Clear();

		int top = 0;
		var genericType = typeof(SetElementWrapper<>).MakeGenericType(setType);

		DataWrapperList = new List<ISetElementWrapper>();

		if (Data != null) {
			var valuesEnumerator = ((IEnumerable)Data).GetEnumerator();
			int i = 0;

			while (valuesEnumerator.MoveNext()) {
				ISetElementWrapper proxy = (ISetElementWrapper)Activator.CreateInstance(genericType,
					new object[] { valuesEnumerator.Current, (object)Data });
				DataWrapperList.Add(proxy);

				var wrappermemberInfo = ConfigManager.GetFieldsAndProperties(this).ToList().First(x => x.Name == "DataWrapperList");
				int index = i;
				var wrapped = UIModConfig.WrapIt(DataList, ref top, wrappermemberInfo, this, 0, DataWrapperList, genericType, i);
				wrapped.Item2.Left.Pixels += 24;
				wrapped.Item2.Width.Pixels -= 24;

				// Add delete button.
				UIModConfigHoverImage deleteButton = new UIModConfigHoverImage(DeleteTexture, Language.GetTextValue("tModLoader.ModConfigRemove"));
				deleteButton.VAlign = 0.5f;

				// fix delete.
				object o = valuesEnumerator.Current; // needed for closure?
				deleteButton.OnLeftClick += (a, b) => {
					var removeMethod = Data.GetType().GetMethods().FirstOrDefault(m => m.Name == "Remove");
					removeMethod.Invoke(Data, new object[] { o });
					SetupList();
					Interface.modConfig.SetPendingChanges();
				};
				wrapped.Item1.Append(deleteButton);

				i++;
			}
		}
	}
}
