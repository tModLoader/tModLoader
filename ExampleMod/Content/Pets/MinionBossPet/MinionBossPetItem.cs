using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Pets.MinionBossPet
{
	// You can find a simple pet example in the ExampleMod\Content\Pets\ExamplePet\ folder
	public class MinionBossPetItem : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToVanitypet(ModContent.ProjectileType<MinionBossPetProjectile>(), ModContent.BuffType<MinionBossPetBuff>()); // Vanilla has many useful methods like these, use them! It sets rarity and value as well, so we have to overwrite those after

			Item.width = 28;
			Item.height = 20;
			Item.rare = ItemRarityID.Master;
			Item.master = true; // This makes sure that "Master" displays in the tooltip, as the rarity only changes the item name color
			Item.value = Item.sellPrice(0, 5);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2); // The item applies the buff, the buff spawns the projectile

			return false;
		}
	}
}
