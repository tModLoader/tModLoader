using System;

namespace Terraria.ModLoader.Config.UI;

internal class ArrayElement : CollectionElement
{
	private Type itemType;

	protected override bool CanAdd => false;

	protected override void AddItem()
	{
		throw new NotImplementedException();
	}

	protected override void ClearCollection()
	{
		throw new NotImplementedException();
	}

	protected override void InitializeCollection()
	{
		throw new NotImplementedException();
	}

	protected override void NullCollection()
	{
		throw new NotImplementedException();
	}

	protected override void PrepareTypes()
	{
		itemType = MemberInfo.Type.GetElementType();
	}

	protected override void SetupList()
	{
		DataList.Clear();

		Array array = MemberInfo.GetValue(Item) as Array;
		int count = array.Length;
		int top = 0;

		for (int i = 0; i < count; i++) {
			int index = i;
			UIModConfig.WrapIt(DataList, ref top, MemberInfo, Item, 0, Data, itemType, index);
		}
	}
}
