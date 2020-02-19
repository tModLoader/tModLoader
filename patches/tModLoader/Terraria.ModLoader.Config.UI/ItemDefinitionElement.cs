using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI
{
	class ItemDefinitionElement : DefinitionElement<ItemDefinition>
	{
		protected override DefinitionOptionElement<ItemDefinition> CreateDefinitionOptionElement() => new ItemDefinitionOptionElement(Value, 0.5f);

		protected override List<DefinitionOptionElement<ItemDefinition>> CreateDefinitionOptionElementList() {
			var options = new List<DefinitionOptionElement<ItemDefinition>>();
			for (int i = 0; i < ItemLoader.ItemCount; i++) {
				var optionElement = new ItemDefinitionOptionElement(new ItemDefinition(i), optionScale);
				optionElement.OnClick += (a, b) => {
					Value = optionElement.definition;
					updateNeeded = true;
					selectionExpanded = false;
				};
				options.Add(optionElement);
			}
			return options;
		}

		protected override List<DefinitionOptionElement<ItemDefinition>> GetPassedOptionElements() {
			var passed = new List<DefinitionOptionElement<ItemDefinition>>();
			foreach (var option in options) {
				if (ItemID.Sets.Deprecated[option.type])
					continue;
				// Should this be the localized item name?
				if (Lang.GetItemNameValue(option.type).IndexOf(chooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
					continue;
				string modname = "Terraria";
				if (option.type > ItemID.Count) {
					modname = ItemLoader.GetItem(option.type).mod.DisplayName; // or internal name?
				}
				if (modname.IndexOf(chooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
					continue;
				passed.Add(option);
			}
			return passed;
		}
	}

	internal class ItemDefinitionOptionElement : DefinitionOptionElement<ItemDefinition>
	{
		public Item item;

		public ItemDefinitionOptionElement(ItemDefinition definition, float scale = .75f) : base(definition, scale) {
		}

		public override void SetItem(ItemDefinition definition) {
			base.SetItem(definition);
			this.item = new Item();
			this.item.SetDefaults(this.type);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (item != null) {
				CalculatedStyle dimensions = base.GetInnerDimensions();
				spriteBatch.Draw(backgroundTexture, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
				if (!item.IsAir || unloaded) {
					int type = unloaded ? ItemID.Count : this.item.type;
					Texture2D itemTexture = Main.itemTexture[type];
					Rectangle rectangle2;
					if (Main.itemAnimations[type] != null) {
						rectangle2 = Main.itemAnimations[type].GetFrame(itemTexture);
					}
					else {
						rectangle2 = itemTexture.Frame(1, 1, 0, 0);
					}
					Color newColor = Color.White;
					float pulseScale = 1f;
					ItemSlot.GetItemLight(ref newColor, ref pulseScale, item, false);
					int height = rectangle2.Height;
					int width = rectangle2.Width;
					float drawScale = 1f;
					float availableWidth = (float)defaultBackgroundTexture.Width * scale;
					if (width > availableWidth || height > availableWidth) {
						if (width > height) {
							drawScale = availableWidth / width;
						}
						else {
							drawScale = availableWidth / height;
						}
					}
					drawScale *= scale;
					Vector2 vector = backgroundTexture.Size() * scale;
					Vector2 position2 = dimensions.Position() + vector / 2f - rectangle2.Size() * drawScale / 2f;
					Vector2 origin = rectangle2.Size() * (pulseScale / 2f - 0.5f);

					if (ItemLoader.PreDrawInInventory(item, spriteBatch, position2, rectangle2, item.GetAlpha(newColor),
						item.GetColor(Color.White), origin, drawScale * pulseScale)) {
						spriteBatch.Draw(itemTexture, position2, new Rectangle?(rectangle2), item.GetAlpha(newColor), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
						if (item.color != Color.Transparent) {
							spriteBatch.Draw(itemTexture, position2, new Rectangle?(rectangle2), item.GetColor(Color.White), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
						}
					}
					ItemLoader.PostDrawInInventory(item, spriteBatch, position2, rectangle2, item.GetAlpha(newColor),
						item.GetColor(Color.White), origin, drawScale * pulseScale);
					if (ItemID.Sets.TrapSigned[type]) {
						spriteBatch.Draw(Main.wireTexture, dimensions.Position() + new Vector2(40f, 40f) * scale, new Rectangle?(new Rectangle(4, 58, 8, 8)), Color.White, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
					}
				}
			}
			if (IsMouseHovering)
				UIModConfig.tooltip = tooltip;
		}
	}

}
