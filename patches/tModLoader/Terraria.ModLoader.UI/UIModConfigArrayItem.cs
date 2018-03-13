using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.GameInput;
using Terraria.Graphics;
using Terraria.UI;
using Terraria.UI.Chat;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Terraria.ModLoader.UI
{
	internal class UIModConfigArrayItem : UIModConfigItem
	{
		private object data;
		private NestedUIList dataList;

		// does not apply?
		//public override int NumberTicks => 0;
		//public override float TickIncrement => 0;

		public UIModConfigArrayItem(PropertyFieldWrapper memberInfo, object item, ref int sliderIDInPage) : base(memberInfo, item, null)
		{
			drawLabel = false;
			string name = memberInfo.Name;
			LabelAttribute att = (LabelAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(LabelAttribute));
			if (att != null)
			{
				name = att.Label;
			}

			UISortableElement sortedContainer = new UISortableElement(-1);
			sortedContainer.Width.Set(0f, 1f);
			sortedContainer.Height.Set(30f, 0f);
			sortedContainer.HAlign = 0.5f;
			var text = new UIText(name);
			text.Top.Pixels += 6;
			text.Left.Pixels += 4;
			sortedContainer.Append(text);
			Append(sortedContainer);

			UIPanel panel = new UIPanel();
			panel.Width.Set(-20f, 1f);
			panel.Left.Set(20f, 0f);
			panel.Top.Set(30f, 0f);
			panel.Height.Set(-30, 1f);
			Append(panel);

			dataList = new NestedUIList();
			dataList.Width.Set(-20, 1f);
			dataList.Left.Set(0, 0f);
			dataList.Height.Set(0, 1f);
			dataList.ListPadding = 5f;
			panel.Append(dataList);

			UIScrollbar scrollbar = new UIScrollbar();
			scrollbar.SetView(100f, 1000f);
			scrollbar.Height.Set(0f, 1f);
			scrollbar.Top.Set(0f, 0f);
			scrollbar.Left.Pixels += 8;
			scrollbar.HAlign = 1f;
			dataList.SetScrollbar(scrollbar);
			panel.Append(scrollbar);

			data = memberInfo.GetValue(item);

			SetupList(ref sliderIDInPage);
		}

		private void SetupList(ref int sliderIDInPage)
		{
			Type itemType = memberInfo.Type.GetElementType();
			dataList.Clear();
			Array array = memberInfo.GetValue(item) as Array;
			int count = array.Length;
			int top = 0;
			for (int i = 0; i < count; i++)
			{
				int index = i;
				UIModConfig.WrapIt(dataList, ref top, memberInfo, item, ref sliderIDInPage, data, itemType, index);
			}
		}

		//protected override void DrawSelf(SpriteBatch spriteBatch)
		//{
		////	Rectangle hitbox = GetInnerDimensions().ToRectangle();
		////	Main.spriteBatch.Draw(Main.magicPixel, hitbox, Color.Purple * 0.6f);
		//	base.DrawSelf(spriteBatch);
		//}
	}
}
