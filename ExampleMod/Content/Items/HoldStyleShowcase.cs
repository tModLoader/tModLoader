using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	/// <summary>
	/// This item lets you test the existing ItemHoldStyleID values for Item.holdStyle. Note that the sword texture might not fit each of the holdStyle animations.
	/// </summary>
	public class HoldStyleShowcase : ModItem
	{
		public override string Texture => "ExampleMod/Content/Items/Weapons/ExampleSword";

		public override void SetStaticDefaults() {
			// Tooltip.SetDefault("This item showcases each HoldStyle.\n<right> to cycle through HoldStyles.");
		}

		public override void SetDefaults() {
			Item.width = 40;
			Item.height = 40;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item1;

			// In Visual Studio, you can click on "ItemHoldStyleID" and then press F12 to see the list of possible values. You can also type "ItemHoldStyleID." to view the list of possible values.
			Item.holdStyle = ItemHoldStyleID.None;
		}

		public override bool AltFunctionUse(Player player) {
			return true;
		}

		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				Item.holdStyle++;
				if (Item.holdStyle > ItemHoldStyleID.HoldRadio) {
					Item.holdStyle = ItemHoldStyleID.None;
				}
				Main.NewText($"Switching to ItemHoldStyleID #{Item.holdStyle}");
			}
			else {
				Main.NewText($"This is ItemHoldStyleID #{Item.holdStyle}");
			}
			return true;
		}
	}
}
