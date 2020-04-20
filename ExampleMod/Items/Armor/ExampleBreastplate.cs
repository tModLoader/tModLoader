using ExampleMod.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items.Armor
{
	/// <summary>
	/// This class serves as an example for a glowmask on an equipable item. Realized via PlayerLayer.
	/// This glowmask is stored as a separate texture (ExampleBreastplate_Body_Glowmask and ExampleBreastplate_FemaleBody_Glowmask), and drawn in ExamplePlayer.
	/// Not including these textures and the code from ExamplePlayer will just leave this class as a basic, functional breastplate example
	/// (You will find the code that handles the drawing in <see cref="ExamplePlayer.ExampleBreastplateGlowmask"/>)
	/// </summary>
	[AutoloadEquip(EquipType.Body)]
	public class ExampleBreastplate : ModItem
	{
		public override void SetStaticDefaults() {
			base.SetStaticDefaults();
			DisplayName.SetDefault("Example Breastplate");
			Tooltip.SetDefault("This is a modded body armor."
				+ "\nImmunity to 'On Fire!'"
				+ "\n+20 max mana and +1 max minions");
		}

		public override void SetDefaults() {
			item.width = 18;
			item.height = 18;
			item.value = 10000;
			item.rare = 2;
			item.defense = 60;
		}

		public override void UpdateEquip(Player player) {
			player.buffImmune[BuffID.OnFire] = true;
			player.statManaMax2 += 20;
			player.maxMinions++;
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<EquipMaterial>(), 60);
			recipe.AddTile(TileType<ExampleWorkbench>());
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}