using System;

namespace Terraria.ModLoader.Config.UI
{
	internal class ArrayElement : CollectionElement
	{
		Type itemType;

		protected override bool CanAdd => false;

		protected override void AddItem() {
			throw new NotImplementedException();
		}

		protected override void ClearCollection() {
			throw new NotImplementedException();
		}

		protected override void InitializeCollection() {
			throw new NotImplementedException();
		}

		protected override void NullCollection() {
			throw new NotImplementedException();
		}

		protected override void PrepareTypes() {
			itemType = memberInfo.Type.GetElementType();
		}

		protected override void SetupList() {
			dataList.Clear();
			Array array = memberInfo.GetValue(item) as Array;
			int count = array.Length;
			int top = 0;
			for (int i = 0; i < count; i++) {
				int index = i;
				UIModConfig.WrapIt(dataList, ref top, memberInfo, item, 0, data, itemType, index);
			}
		}
	}
}
