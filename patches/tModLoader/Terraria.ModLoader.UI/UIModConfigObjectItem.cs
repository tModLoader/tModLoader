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
		int height;
		// Label:
		//  Members
		//  Members
		public UIModConfigObjectItem(PropertyFieldWrapper memberInfo, object item, ref int i, IList array = null, int index = -1) : base(memberInfo, item)
		{
			drawLabel = false;
			//// Class
			//BackgroundColorAttribute bca = (BackgroundColorAttribute)Attribute.GetCustomAttribute(variableInfo.Type, typeof(BackgroundColorAttribute), true);
			//if (bca != null)
			//{
			//	backgroundColor = bca.color;
			//}
			//if (array != null)
			//{
			//	bca = (BackgroundColorAttribute)Attribute.GetCustomAttribute(item.GetType(), typeof(BackgroundColorAttribute), true);
			//	if (bca != null)
			//	{
			//		backgroundColor = bca.color;
			//	}
			//}
			//// Member
			//bca = (BackgroundColorAttribute)Attribute.GetCustomAttribute(variableInfo.MemberInfo, typeof(BackgroundColorAttribute));
			//if (bca != null)
			//{
			//	backgroundColor = bca.color;
			//}

			string name = memberInfo.Name;
			LabelAttribute att = (LabelAttribute)Attribute.GetCustomAttribute(memberInfo.MemberInfo, typeof(LabelAttribute));
			if (att != null)
			{
				name = att.Label;
			}
			if (array == null)
			{
				var label = new UIText($"{name}:");
				label.Top.Pixels += 6;
				label.Left.Pixels += 4;
				Append(label);
				height = 30;
			}


			PropertyInfo[] properties = item.GetType().GetProperties(
			//	BindingFlags.DeclaredOnly |
				BindingFlags.Public |
				BindingFlags.Instance);

			FieldInfo[] fields = item.GetType().GetFields(
			//	BindingFlags.DeclaredOnly |
				BindingFlags.Public |
				BindingFlags.Instance);

			var fieldsAndProperties = fields.Select(x => new PropertyFieldWrapper(x)).Concat(properties.Select(x => new PropertyFieldWrapper(x)));

			foreach (PropertyFieldWrapper variable in fieldsAndProperties)
			{
				if (variable.isProperty && variable.Name == "Mode")
					continue;
				if (Attribute.IsDefined(variable.MemberInfo, typeof(JsonIgnoreAttribute)))
					continue;

				var wrapped = UIModConfig.WrapIt(this, ref height, variable, item, ref i);

				if (array != null)
				{
					wrapped.Item1.Left.Pixels -= 20;
					wrapped.Item1.Width.Pixels += 20;
				}
			}
		}

		//protected override void DrawSelf(SpriteBatch spriteBatch)
		//{
		//	Rectangle hitbox = GetInnerDimensions().ToRectangle();
		//	Main.spriteBatch.Draw(Main.magicPixel, hitbox, backgroundColor * 0.6f);
		//	base.DrawSelf(spriteBatch);
		//}

		internal float GetHeight()
		{
			return height;
		}
	}
}
