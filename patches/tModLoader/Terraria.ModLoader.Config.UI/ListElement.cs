using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	internal class ListElement : CollectionElement
	{
		private Type listType;

		protected override void PrepareTypes() {
			listType = memberInfo.Type.GetGenericArguments()[0];
			jsonDefaultListValueAttribute = ConfigManager.GetCustomAttribute<JsonDefaultListValueAttribute>(memberInfo, listType);
		}

		protected override void AddItem() {
			((IList)data).Add(CreateCollectionElementInstance(listType));
		}

		protected override void InitializeCollection() {
			data = Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));
			SetObject(data);
		}

		protected override void ClearCollection() {
			((IList)data).Clear();
		}

		protected override void SetupList() {
			dataList.Clear();
			int top = 0;
			if (data != null) {
				for (int i = 0; i < ((IList)data).Count; i++) {
					int index = i;
					var wrapped = UIModConfig.WrapIt(dataList, ref top, memberInfo, item, 0, data, listType, index);

					wrapped.Item2.Left.Pixels += 24;
					wrapped.Item2.Width.Pixels -= 30;

					// Add delete button.
					UIModConfigHoverImage deleteButton = new UIModConfigHoverImage(deleteTexture, "Remove");
					deleteButton.VAlign = 0.5f;
					deleteButton.OnClick += (a, b) => { ((IList)data).RemoveAt(index); SetupList(); Interface.modConfig.SetPendingChanges(); };
					wrapped.Item1.Append(deleteButton);
				}
			}
		}
	}

	class NestedUIList : UIList
	{
		public NestedUIList() {
			//OverflowHidden = false;
		}

		public override void ScrollWheel(UIScrollWheelEvent evt) {
			if (this._scrollbar != null) {
				float oldpos = this._scrollbar.ViewPosition;
				this._scrollbar.ViewPosition -= (float)evt.ScrollWheelValue;
				if (oldpos == _scrollbar.ViewPosition) {
					base.ScrollWheel(evt);
				}
			}
			else {
				base.ScrollWheel(evt);
			}
		}
	}
}
