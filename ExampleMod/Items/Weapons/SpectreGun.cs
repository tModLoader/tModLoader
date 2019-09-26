using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items.Weapons
{
	public class SpectreGun : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Uses wisps as ammo");
		}

		public override void SetDefaults() {
			item.damage = 53;
			item.ranged = true;
			item.width = 42;
			item.height = 30;
			item.useTime = 35;
			item.useAnimation = 35;
			item.useStyle = 5;
			item.noMelee = true;
			item.knockBack = 4f;
			item.value = Item.sellPrice(0, 10, 0, 0);
			item.rare = 8;
			item.UseSound = mod.GetLegacySoundSlot(SoundType.Item, "Sounds/Item/Wooo");
			item.autoReuse = true;
			item.shoot = ProjectileType<Projectiles.Wisp>();
			item.shootSpeed = 6f;
			item.useAmmo = ItemType<Wisp>();        //Restrict the type of ammo the weapon can use, so that the weapon cannot use other ammos
		}

		public override void AddRecipes() {
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemType<ExampleItem>(), 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}

		public override void ModifyWeaponDamage(Player player, ref float add, ref float mult, ref float flat) {
			// Here we use the multiplicative damage modifier because Terraria does this approach for Ammo damage bonuses. 
			mult *= player.bulletDamage;
		}

		public override Vector2? HoldoutOffset() {
			return Vector2.Zero;
		}
	}
}
