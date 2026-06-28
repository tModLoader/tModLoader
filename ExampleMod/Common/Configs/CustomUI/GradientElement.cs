using ExampleMod.Common.Configs.CustomDataTypes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;

// ATTENTION: Below this point is custom config UI element.
// Be aware that mods using custom config elements will break with the next few tModLoader updates until their design is finalized.
// You will need to be very active in updating your mod if you use these as they can break in any update.

// This file defines a custom ConfigElement based on Gradient data type
// with custom drawing implemented that can be used in ModConfig classes.
namespace ExampleMod.Common.Configs.CustomUI
{
	// This custom config UI element uses vanilla config elements paired with custom drawing.
	class GradientElement : ConfigElement
	{
		private List<Tuple<UIElement, UIElement>> wrappedElements = [];

		private object GetSubItem() {
			object subItem = MemberInfo.GetValue(Item);

			if (subItem == null) {
				subItem = Activator.CreateInstance(MemberInfo.Type);
				JsonConvert.PopulateObject("{}", subItem, ConfigManager.serializerSettings);
				MemberInfo.SetValue(Item, subItem);
			}

			return subItem;
		}

		public override void OnBind() {
			base.OnBind();

			var subItem = GetSubItem();

			// Item is the owner object instance, MemberInfo is the info about this field in Item

			int height = 30;
			int order = 0;

			foreach (PropertyFieldWrapper variable in ConfigManager.GetFieldsAndProperties(subItem)) {
				var wrapped = ConfigManager.WrapIt(this, ref height, variable, subItem, order++);
				wrappedElements.Add(wrapped);

				if (List != null) {
					wrapped.Item1.Left.Pixels -= 20;
					wrapped.Item1.Width.Pixels += 20;
				}
			}
		}

		// When the config is modified, we need to update the references to the objects in case they were modified
		// RefreshUI() is called when the config might need refreshing
		// For more info, read the documentation for RefreshUI()
		public override void RefreshUI() {
			foreach (var wrappedElement in wrappedElements) {
				if (wrappedElement.Item2 is not ConfigElement configElement)
					return;

				configElement.UpdateObject(GetSubItem(), null, -1);
				configElement.RefreshUI();
			}
		}

		public override void Draw(SpriteBatch spriteBatch) {
			base.Draw(spriteBatch);
			var hitbox = GetInnerDimensions().ToRectangle();
			if (MemberInfo.GetValue(Item) is Gradient g) {
				int left = (hitbox.Left + hitbox.Right) / 2;
				int right = hitbox.Right;
				int steps = right - left;
				for (int i = 0; i < steps; i += 1) {
					float percent = (float)i / steps;
					spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(left + i, hitbox.Y, 1, 30), Color.Lerp(g.start, g.end, percent));
				}

				//Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(hitbox.X + hitbox.Width / 2, hitbox.Y, hitbox.Width / 4, 30), g.start);
				//Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(hitbox.X + 3 * hitbox.Width / 4, hitbox.Y, hitbox.Width / 4, 30), g.end);
			}
		}
	}
}
