using System;
using System.Collections;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal class ListElement : CollectionElement
{
	private Type listType;

	protected override void PrepareTypes()
	{
		listType = MemberInfo.Type.GetGenericArguments()[0];
		JsonDefaultListValueAttribute = ConfigManager.GetCustomAttributeFromCollectionMemberThenElementType<JsonDefaultListValueAttribute>(MemberInfo.MemberInfo, listType);
	}

	protected override void AddItem()
	{
		((IList)Data).Add(CreateCollectionElementInstance(listType));
	}

	protected override void InitializeCollection()
	{
		Data = Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));
		SetObject(Data);
	}

	protected override void ClearCollection()
	{
		((IList)Data).Clear();
	}

	protected override void SetupList()
	{
		DataList.Clear();

		int top = 0;

		if (Data != null) {
			for (int i = 0; i < ((IList)Data).Count; i++) {
				int index = i;
				var wrapped = UIModConfig.WrapIt(DataList, ref top, MemberInfo, Item, 0, Data, listType, index);

				wrapped.Item2.Left.Pixels += 24;
				wrapped.Item2.Width.Pixels -= 30;

				// Add delete button.
				UIModConfigHoverImage deleteButton = new UIModConfigHoverImage(DeleteTexture, Language.GetTextValue("tModLoader.ModConfigRemove"));
				deleteButton.VAlign = 0.5f;
				deleteButton.OnLeftClick += (a, b) => { ((IList)Data).RemoveAt(index); SetupList(); Interface.modConfig.SetPendingChanges(); };
				wrapped.Item1.Append(deleteButton);
			}
		}
	}
}

internal class NestedUIList : UIList
{
	/*
	public NestedUIList() {
		OverflowHidden = false;
	}
	*/

	public override void ScrollWheel(UIScrollWheelEvent evt)
	{
		if (_scrollbar != null) {
			float oldpos = _scrollbar.ViewPosition;

			_scrollbar.ViewPosition -= evt.ScrollWheelValue;

			if (oldpos == _scrollbar.ViewPosition) {
				base.ScrollWheel(evt);
			}
		}
		else {
			base.ScrollWheel(evt);
		}
	}
}
