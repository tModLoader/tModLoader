using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.UI;

namespace Terraria.ModLoader.Config.UI
{
	internal abstract class ISetElementWrapper
	{
		internal virtual object Value => null;
	}

	// TODO: Somehow prevent this class from displaying?
	internal class SetElementWrapper<V> : ISetElementWrapper
	{
		private object set;

		private V _value;
		public V value {
			get { return _value; }
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

				//if (set.Contains(value))
				//{

				//}
				//else
				//{
				//	set.Remove(_value);
				//	_value = value;
				//	set.Add(_value, _value);
				//}
			}
		}
		internal override object Value => value;

		public SetElementWrapper(V value, object set) {
			this.set = set;
			this._value = value;
		}
	}

	internal class SetElement : CollectionElement
	{
		private Type setType;
		public List<ISetElementWrapper> dataWrapperList;

		protected override void PrepareTypes() {
			setType = memberInfo.Type.GetGenericArguments()[0];
			jsonDefaultListValueAttribute = ConfigManager.GetCustomAttribute<JsonDefaultListValueAttribute>(memberInfo, setType);
		}

		protected override void AddItem() {
			var addMethod = data.GetType().GetMethods().FirstOrDefault(m => m.Name == "Add");
			addMethod.Invoke(data, new object[] { CreateCollectionElementInstance(setType) });
		}

		protected override void InitializeCollection() {
			data = Activator.CreateInstance(typeof(HashSet<>).MakeGenericType(setType));
			SetObject(data);
		}

		protected override void ClearCollection() {
			var clearMethod = data.GetType().GetMethods().FirstOrDefault(m => m.Name == "Clear");
			clearMethod.Invoke(data, new object[] { });
		}

		protected override void SetupList() {
			dataList.Clear();
			int top = 0;
			dataWrapperList = new List<ISetElementWrapper>();

			Type genericType = typeof(SetElementWrapper<>).MakeGenericType(setType);

			if (data != null) {
				var valuesEnumerator = ((IEnumerable)data).GetEnumerator();

				int i = 0;
				while (valuesEnumerator.MoveNext()) {
					ISetElementWrapper proxy = (ISetElementWrapper)Activator.CreateInstance(genericType,
						new object[] { valuesEnumerator.Current, (object)data });
					dataWrapperList.Add(proxy);

					var wrappermemberInfo = ConfigManager.GetFieldsAndProperties(this).ToList()[0];
					int index = i;
					var wrapped = UIModConfig.WrapIt(dataList, ref top, wrappermemberInfo, this, 0, dataWrapperList, genericType, i);
					wrapped.Item2.Left.Pixels += 24;
					wrapped.Item2.Width.Pixels -= 24;

					// Add delete button.
					UIModConfigHoverImage deleteButton = new UIModConfigHoverImage(deleteTexture, "Remove");
					deleteButton.VAlign = 0.5f;

					// fix delete.
					object o = valuesEnumerator.Current; // needed for closure?
					deleteButton.OnClick += (a, b) => {
						var removeMethod = data.GetType().GetMethods().FirstOrDefault(m => m.Name == "Remove");
						removeMethod.Invoke(data, new object[] { o });
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
