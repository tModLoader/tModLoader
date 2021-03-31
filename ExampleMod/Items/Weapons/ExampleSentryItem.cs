using ExampleMod.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items.Weapons
{
	public class ExampleSentryItem : ModItem
	{
		public override void SetStaticDefaults()
		{
			Tooltip.SetDefault("Summons an example sentry to fight for you");
			ItemID.Sets.GamepadWholeScreenUseRange[item.type] = true;
			ItemID.Sets.LockOnIgnoresCollision[item.type] = true;
		}

		public override void SetDefaults()
		{
			item.damage = 50;
			item.sentry = true;
			item.mana = 10; //How much mana this weapon takes to use.
			item.width = 26; //Item width hitbox.
			item.height = 28; //Item height hitbox.
			item.useTime = 36;
			item.useAnimation = 36;
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.noMelee = true; //Restricts this weapon dealing melee damage.
			item.knockBack = 3;
			item.value = Item.buyPrice(0, 30, 0, 0); //How much this item is sold for.
			item.rare = ItemRarityID.Cyan;
			item.UseSound = SoundID.Item83;
			item.shoot = ModContent.ProjectileType<ExampleSentry>();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
			if (player.whoAmI == Main.myPlayer) {
				position = Main.MouseWorld;
			}
			return true;
		}
	}
}