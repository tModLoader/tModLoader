using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.Default;
using Terraria.UI;

namespace Terraria.ModLoader.Config.UI;

internal class ItemDefinitionElement : DefinitionElement<ItemDefinition>
{
	protected override DefinitionOptionElement<ItemDefinition> CreateDefinitionOptionElement() => new ItemDefinitionOptionElement(Value, 0.5f);

	protected override List<DefinitionOptionElement<ItemDefinition>> CreateDefinitionOptionElementList()
	{
		var options = new List<DefinitionOptionElement<ItemDefinition>>();

		for (int i = 0; i < ItemLoader.ItemCount; i++) {
			var optionElement = new ItemDefinitionOptionElement(new ItemDefinition(i), OptionScale);
			optionElement.OnLeftClick += (a, b) => {
				Value = optionElement.Definition;
				UpdateNeeded = true;
				SelectionExpanded = false;
			};
			options.Add(optionElement);
		}

		return options;
	}

	protected override List<DefinitionOptionElement<ItemDefinition>> GetPassedOptionElements()
	{
		var passed = new List<DefinitionOptionElement<ItemDefinition>>();

		foreach (var option in Options) {
			if (ItemID.Sets.Deprecated[option.Type])
				continue;

			// Should this be the localized item name?
			if (!Lang.GetItemNameValue(option.Type).Contains(ChooserFilter.CurrentString, StringComparison.OrdinalIgnoreCase))
				continue;

			string modname = "Terraria";

			if (option.Type >= ItemID.Count) {
				modname = ItemLoader.GetItem(option.Type).Mod.DisplayNameClean; // or internal name?
			}

			if (modname.IndexOf(ChooserFilterMod.CurrentString, StringComparison.OrdinalIgnoreCase) == -1)
				continue;

			passed.Add(option);
		}
		return passed;
	}
}

internal class ItemDefinitionOptionElement : DefinitionOptionElement<ItemDefinition>
{
	public Item Item { get; set; }

	public ItemDefinitionOptionElement(ItemDefinition definition, float scale = .75f) : base(definition, scale)
	{
	}

	public override void SetItem(ItemDefinition definition)
	{
		base.SetItem(definition);

		Item = new Item();
		Item.SetDefaults(Type);
	}

	protected override void DrawSelf(SpriteBatch spriteBatch)
	{
		if (Item != null) {
			CalculatedStyle dimensions = base.GetInnerDimensions();
			spriteBatch.Draw(BackgroundTexture.Value, dimensions.Position(), null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);

			if (!Item.IsAir || Unloaded) {
				int type = Unloaded ? ModContent.ItemType<UnloadedItem>() : Item.type;
				if (TextureAssets.Item[type].State == AssetState.NotLoaded)
					Main.Assets.Request<Texture2D>(TextureAssets.Item[type].Name, AssetRequestMode.AsyncLoad);
				Texture2D itemTexture = TextureAssets.Item[type].Value;
				Rectangle rectangle2;

				if (Main.itemAnimations[type] != null) {
					rectangle2 = Main.itemAnimations[type].GetFrame(itemTexture);
				}
				else {
					rectangle2 = itemTexture.Frame(1, 1, 0, 0);
				}

				Color newColor = Color.White;
				float pulseScale = 1f;
				ItemSlot.GetItemLight(ref newColor, ref pulseScale, Item, false);
				int height = rectangle2.Height;
				int width = rectangle2.Width;
				float drawScale = 1f;
				float availableWidth = (float)DefaultBackgroundTexture.Width() * Scale;

				if (width > availableWidth || height > availableWidth) {
					if (width > height) {
						drawScale = availableWidth / width;
					}
					else {
						drawScale = availableWidth / height;
					}
				}

				drawScale *= Scale;
				Vector2 vector = BackgroundTexture.Size() * Scale;
				Vector2 position2 = dimensions.Position() + vector / 2f;
				Vector2 origin = rectangle2.Size() / 2;

				if (ItemLoader.PreDrawInInventory(Item, spriteBatch, position2, rectangle2, Item.GetAlpha(newColor),
					Item.GetColor(Color.White), origin, drawScale * pulseScale)) {
					spriteBatch.Draw(itemTexture, position2, new Rectangle?(rectangle2), Item.GetAlpha(newColor), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);

					if (Item.color != Color.Transparent) {
						spriteBatch.Draw(itemTexture, position2, new Rectangle?(rectangle2), Item.GetColor(Color.White), 0f, origin, drawScale * pulseScale, SpriteEffects.None, 0f);
					}
				}

				ItemLoader.PostDrawInInventory(Item, spriteBatch, position2, rectangle2, Item.GetAlpha(newColor), Item.GetColor(Color.White), origin, drawScale * pulseScale);

				if (ItemID.Sets.TrapSigned[type]) {
					spriteBatch.Draw(TextureAssets.Wire.Value, dimensions.Position() + new Vector2(40f, 40f) * Scale, new Rectangle?(new Rectangle(4, 58, 8, 8)), Color.White, 0f, new Vector2(4f), 1f, SpriteEffects.None, 0f);
				}
			}
		}

		if (IsMouseHovering)
			UIModConfig.Tooltip = Tooltip;
	}
}

