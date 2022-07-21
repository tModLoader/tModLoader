﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.ModLoader.IO;
using System.IO;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace ExampleMod.Content.Items.Consumables
{
	// This showcases how the CanStack hook can be used in conjunction with custom data
	// Custom data is also shown in ExampleDataItem, but here we need to use more hooks

	// This item, when crafted, stores the players name, and only lets other players open it. Bags with the same stored name aren't stackable
	public class ExampleCanStackItem : ModItem
	{
		// We set this when the item is crafted. In other contexts, this will be the empty string ""
		public string craftedPlayerName = string.Empty;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example CanStack Item: Gift Bag");
			Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}"); // References a language key that says "Right Click To Open" in the language of the game

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 3;
		}

		public override void SetDefaults() {
			Item.maxStack = 99; // This item is stackable, otherwise the example wouldn't work
			Item.consumable = true;
			Item.width = 22;
			Item.height = 26;
			Item.rare = ItemRarityID.Blue;
		}

		public override bool CanRightClick() {
			// The bag can't be opened if it wasn't crafted
			if (craftedPlayerName == string.Empty) {
				return false;
			}

			// The bag can't be opened by the player who crafted it
			return Main.LocalPlayer.name != craftedPlayerName;
		}

		public override bool CanStack(Item item2) {
			// The bag can only be stacked with other bags if the names match

			// We have to cast the second item to the class (This is safe to do as the hook is only called on items of the same type)
			var otherItem = item2.ModItem as ExampleCanStackItem;

			return craftedPlayerName == otherItem.craftedPlayerName;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			LeadingConditionRule hardmodeCondition = new(new Conditions.IsHardmode());
			hardmodeCondition.OnSuccess(ItemDropRule.Common(ItemID.ChocolateChipCookie));
			hardmodeCondition.OnFailedConditions(ItemDropRule.Common(ItemID.Coconut));
			itemLoot.Add(hardmodeCondition);
		}

		// The following 4 hooks are needed if your item data should be persistent between saves, and work in multiplayer
		public override void SaveData(TagCompound tag) {
			tag.Add("craftedPlayerName", craftedPlayerName);
		}

		public override void LoadData(TagCompound tag) {
			craftedPlayerName = tag.GetString("craftedPlayerName");
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write(craftedPlayerName);
		}

		public override void NetReceive(BinaryReader reader) {
			craftedPlayerName = reader.ReadString();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (craftedPlayerName != string.Empty) {
				// Here we make a distinction to disclose that the bag can't be opened by the player who crafted it
				if (Main.LocalPlayer.name == craftedPlayerName) {
					tooltips.Add(new TooltipLine(Mod, "CraftedPlayerNameCannotOpen", $"You crafted this bag and cannot open it!"));
				}
				else {
					tooltips.Add(new TooltipLine(Mod, "CraftedPlayerNameOther", $"This is a bag from {craftedPlayerName}, open it to receive a gift!"));
				}
			}
			else {
				tooltips.Add(new TooltipLine(Mod, "CraftedPlayerNameEmpty", $"This bag was not crafted, it will do nothing"));
			}
		}

		public override void OnCreate(ItemCreationContext context) {
			if (context is RecipeCreationContext) {
				// If the item was crafted, store the crafting players name
				craftedPlayerName = Main.LocalPlayer.name;
			}
		}

		public override void AddRecipes() {
			CreateRecipe().
				AddIngredient<ExampleItem>(20).
				AddTile(TileID.WorkBenches).
				Register();
		}
	}
}
