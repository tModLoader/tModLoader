using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.Creative;
using ExampleMod.Content.Projectiles;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleFlail : ModItem
	{
		public override void SetStaticDefaults() 
		{
			DisplayName.SetDefault("Example Flail"); //The name of your flail
			Tooltip.SetDefault("This is a modded flail."); //The description of your flail
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1; //Journey mode, 1 item required to research
        }

		public override void SetDefaults() 
		{
			Item.damage = 30; //The damage of your flail
			Item.DamageType = DamageClass.Melee; //Deals melee damage
			Item.width = 32; //Hitbox width of the item.
			Item.height = 32; //Hitbox height of the item.
			Item.useTime = 45; //The item's use time in ticks (60 ticks == 1 second.)
			Item.useAnimation = 45; //The item's use time in ticks (60 ticks == 1 second.)
			Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
            Item.value = Item.sellPrice(silver: 10); //Sells for 10 silver
            Item.rare = ItemRarityID.Green; //The color of the name of your item
			Item.UseSound = SoundID.Item1; //The sound that this item makes when used
			Item.autoReuse = false; //Thrown flails don't have autoswing
            Item.useTurn = false;
            Item.knockBack = 6.5f; //The knockback of your flail
            Item.shootSpeed = 12f; //The speed of the projectile measured in pixels per frame.
            Item.shoot = ModContent.ProjectileType<ExampleFlailProjectile>(); //the flail projectile
            Item.noMelee = true; //This makes sure the item does not deal damage from the swinging animation
            Item.noUseGraphic = true; //This makes sure the item does not get shown when the player swings his hand
            Item.channel = true;
        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
    }
}