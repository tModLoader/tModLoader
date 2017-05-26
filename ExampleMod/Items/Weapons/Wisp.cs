using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Weapons
{
	public class Wisp : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Chases enemies through walls");
		}

		public override void SetDefaults()
		{
			item.damage = 1;
			item.ranged = true;
			item.width = 14;
			item.height = 14;
			item.maxStack = 999;
			item.consumable = true;
			item.knockBack = 1f;
			item.value = Item.sellPrice(0, 0, 1, 0);
			item.rare = 8;
			item.shoot = mod.ProjectileType("Wisp");
			item.ammo = item.type; // The first item in an ammo class sets the AmmoID to it's type
		}

		public override void AddRecipes()
		{
			WispRecipe recipe = new WispRecipe(mod);
			recipe.AddIngredient(ItemID.Ectoplasm);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this, 50);
			recipe.AddRecipe();
		}
	}

	public class WispRecipe : ModRecipe
	{
		public WispRecipe(Mod mod) : base(mod)
		{
		}

		public override bool RecipeAvailable()
		{
			return Main.LocalPlayer.HasItem(mod.ItemType("SpectreGun"));
		}

		public override int ConsumeItem(int type, int numRequired)
		{
			if (type == ItemID.Ectoplasm && Main.LocalPlayer.adjTile[mod.TileType("ExampleWorkbench")])
			{
				Main.PlaySound(2, -1, -1, mod.GetSoundSlot(SoundType.Item, "Sounds/Item/Wooo"));
				return Main.rand.Next(2) == 0 ? 0 : 1; //You have half chance to not consume your materials
			}
			return base.ConsumeItem(type, numRequired);
		}
	}
}
