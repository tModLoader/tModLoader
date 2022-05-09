using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace ExampleMod.Content.Pets.MinionBossPet
{
	// You can find a simple pet example in ExampleMod\Content\Pets\ExamplePet
	public class MinionBossPetItem : ModItem
	{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Minion Boss Pet");
			Tooltip.SetDefault("Summons a miniature Minion Boss to follow you");

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			Item.DefaultToVanitypet(ModContent.ProjectileType<MinionBossPetProjectile>(), ModContent.BuffType<MinionBossPetBuff>()); // Vanilla has many useful methods like these, use them! It sets rarity and value aswell, so we have to overwrite those after

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
