using ExampleMod.Content.Rarities;
using ExampleMod.Content.Projectiles.Minions;
using ExampleMod.Content.Buffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleHoveringMinionTotem : ModItem
	{
		public override void SetStaticDefaults() {
			Tooltip.SetDefault("Summons the Example Hoving Minion to fight for you.");

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			Item.width = 40; // The item texture's width
			Item.height = 40; // The item texture's height

			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTime = 20;
			Item.useAnimation = 20;

			Item.DamageType = DamageClass.Summon; // This item is part of the summon class
			Item.mana = 10;
			Item.damage = 100;
			Item.shoot = ModContent.ProjectileType<ExampleHoveringMinion>();
			Item.buffType = ModContent.BuffType<ExampleHoveringMinionBuff>();
			
			Item.value = Item.buyPrice(gold: 1); // The value of the item
			Item.rare = ModContent.RarityType<ExampleModRarity>(); // The item's rarity
			Item.UseSound = SoundID.Item44; // The sound when the weapon is being used
		}

		public override bool Shoot(Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			player.AddBuff(Item.buffType, 2);
			return true;
		}
	}
}
