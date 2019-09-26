using ExampleMod.Projectiles.Minions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Items.Weapons
{
	//imported from my tAPI mod because I'm lazy
	public class PurityTotem : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Summons a purity wisp to fight for you.");
		}

		public override void SetDefaults() {
			item.damage = 110;
			item.summon = true;
			item.mana = 10;
			item.width = 26;
			item.height = 28;
			item.useTime = 36;
			item.useAnimation = 36;
			item.useStyle = 1;
			item.noMelee = true;
			item.knockBack = 3;
			item.value = Item.buyPrice(0, 30, 0, 0);
			item.rare = 9;
			item.UseSound = SoundID.Item44;
			item.shoot = ProjectileType<PurityWisp>();
			item.shootSpeed = 10f;
			item.buffType = BuffType<Buffs.PurityWisp>(); //The buff added to player after used the item
			item.buffTime = 3600;               //The duration of the buff, here is 60 seconds
		}

		public override bool AltFunctionUse(Player player) {
			return true;
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			return player.altFunctionUse != 2;
		}

		public override bool UseItem(Player player) {
			if (player.altFunctionUse == 2) {
				player.MinionNPCTargetAim();
			}
			return base.UseItem(player);
		}
	}
}
