using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	/// <summary>
	/// This item lets you test the existing ItemUseStyleID values for Item.useStyle. Note that the sword texture might not fit each of the useStyle animations.
	/// </summary>
	public class UseStyleShowcase : ModItem
	{
		public override string Texture => "ExampleMod/Content/Items/Weapons/ExampleSword";

		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This item showcases each UseStyle.\n<right> to cycle through UseStyles.");
		}

		public override void SetDefaults() {
			Item.width = 40;
			Item.height = 40;

			// In Visual Studio, you can click on "ItemUseStyleID" and then press F12 to see the list of possible values. You can also type "ItemUseStyleID." to view the list of possible values.
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.autoReuse = true;
			Item.UseSound = SoundID.Item1;
		}

		public override bool AltFunctionUse(Player player) {
			return true;
		}

		public override bool? UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				Item.useStyle++;
				if (Item.useStyle > ItemUseStyleID.RaiseLamp) {
					Item.useStyle = ItemUseStyleID.Swing;
				}
				Main.NewText($"Switching to ItemUseStyleID #{Item.useStyle}");
			}
			else {
				Main.NewText($"This is ItemUseStyleID #{Item.useStyle}");
			}
			return true;
		}
	}
}
