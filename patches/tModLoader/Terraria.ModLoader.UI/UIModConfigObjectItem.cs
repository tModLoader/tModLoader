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
using System.Reflection;
using System.Linq;
using Newtonsoft.Json;
using System.Collections;

namespace Terraria.ModLoader.UI
{
	class UIModConfigObjectItem : UIModConfigItem
	{
		private NestedUIList dataList;
		// Label:
		//  Members
		//  Members
		public UIModConfigObjectItem(PropertyFieldWrapper memberInfo, object item, ref int i, IList array = null, int index = -1) : base(memberInfo, item, array)
		{
			drawLabel = false;

			dataList = new NestedUIList();
			dataList.Width.Set(0, 1f);
			dataList.Left.Set(0, 0f);
			dataList.Height.Set(0, 1f);
			dataList.ListPadding = 5f;
			Append(dataList);

			string name = memberInfo.Name;
			if (labelAttribute != null)
			{
				name = labelAttribute.Label;
			}
			if (array == null)
			{
				//var label = new UIText($"{name}:");
				//label.Top.Pixels += 6;
				//label.Left.Pixels += 4;
				//Append(label);

				UISortableElement sortedContainer = new UISortableElement(-1);
				sortedContainer.Width.Set(0f, 1f);
				sortedContainer.Height.Set(30f, 0f);
				sortedContainer.HAlign = 0.5f;
				var text = new UIText($"{name}:");
				text.Top.Pixels += 6;
				text.Left.Pixels += 4;
				sortedContainer.Append(text);
				//Append(sortedContainer);
				dataList.Add(sortedContainer);
			}

			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(item))
			{
				if (variable.isProperty && variable.Name == "Mode")
					continue;
				if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
					continue;

				int top = 0;
				var wrapped = UIModConfig.WrapIt(dataList, ref top, variable, item, ref i);
				if (array != null)
				{
					//wrapped.Item1.Left.Pixels -= 20;
					wrapped.Item1.Width.Pixels += 20;
				}
				else
				{
					wrapped.Item1.Left.Pixels += 20;
					wrapped.Item1.Width.Pixels -= 20;
				}
			}
			Recalculate();
		}

		public override void Recalculate()
		{
			base.Recalculate();
			float h = dataList.GetTotalHeight();
			Height.Set(h, 0f);
			if (Parent != null && Parent is UISortableElement)
			{
				Parent.Height.Set(h, 0f);
			}
		}

		//protected override void DrawSelf(SpriteBatch spriteBatch)
		//{
		//	Rectangle hitbox = GetInnerDimensions().ToRectangle();
		//	Main.spriteBatch.Draw(Main.magicPixel, hitbox, backgroundColor * 0.6f);
		//	base.DrawSelf(spriteBatch);
		//}
	}
}
