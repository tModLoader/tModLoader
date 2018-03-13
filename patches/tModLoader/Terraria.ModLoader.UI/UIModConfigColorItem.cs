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
	class UIModConfigColorItem : UIModConfigItem
	{
		class ColorObject
		{
			internal Color current;
			PropertyFieldWrapper memberInfo;
			object item;
			IList<Color> array;
			int index;

			[Label("Red")]
			public byte R
			{
				get { return current.R; }
				set
				{
					current.R = value;
					Update();
				}
			}
			[Label("Green")]
			public byte G
			{
				get { return current.G; }
				set
				{
					current.G = value;
					Update();
				}
			}
			[Label("Blue")]
			public byte B
			{
				get { return current.B; }
				set
				{
					current.B = value;
					Update();
				}
			}
			[Label("Alpha")]
			public byte A
			{
				get { return current.A; }
				set
				{
					current.A = value;
					Update();
				}
			}
			private void Update()
			{
				if (array == null)
					memberInfo.SetValue(item, current);
				else
					array[index] = current;
			}
			public ColorObject(PropertyFieldWrapper memberInfo, object item)
			{
				this.item = item;
				this.memberInfo = memberInfo;
				current = (Color)memberInfo.GetValue(item);
			}
			public ColorObject(IList<Color> array, int index)
			{
				current = array[index];
				this.array = array;
				this.index = index;
			}
		}

		int height;
		ColorObject c;
		public UIModConfigColorItem(PropertyFieldWrapper memberInfo, object item, ref int i, IList<Color> array = null, int index = -1) : base(memberInfo, item, (IList)array)
		{
			if (array != null)
			{
				drawLabel = false;
				height = 30;
				c = new ColorObject(array, index);
			}
			else
			{
				height = 30;
				c = new ColorObject(memberInfo, item);
			}

			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(c))
			{
				var wrapped = UIModConfig.WrapIt(this, ref height, variable, c, ref i);

				if (array != null)
				{
					wrapped.Item1.Left.Pixels -= 20;
					wrapped.Item1.Width.Pixels += 20;
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
			Rectangle hitbox = GetInnerDimensions().ToRectangle();
			hitbox = new Rectangle(hitbox.X + hitbox.Width / 2, hitbox.Y, hitbox.Width / 2, 30);
			Main.spriteBatch.Draw(Main.magicPixel, hitbox, c.current);
		}

		internal float GetHeight()
		{
			return height;
		}
	}
}