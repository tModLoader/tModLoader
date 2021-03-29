using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	//This example show how to made an Animated Gun(like Vortex Beater)
	//The main idea is to create in this script an animated Projectile that will behave like a Gun(ExampleAnimatedGunProjectile)  
	//Therefore, most of the properties of the weapon are not needed, but we can still use them in the projectile.
	public class ExampleAnimatedGun : ModItem
	{
		public override string Texture => "Terraria/Images/Item_" + ItemID.VortexBeater;

		public override void SetStaticDefaults() {
			Tooltip.SetDefault("This is a modded animated gun.");
		}

		public override void SetDefaults() {
			//Important properties for Animation
			Item.channel = true; //Set wapon to channel mode. When the attack button is held, we create only one projectile(animated gun projectile).
			Item.noUseGraphic = true; //The item's sprite will not be visible while the item is in use

			// This is common properties for Guns like in ExampleGun (https://github.com/tModLoader/tModLoader/wiki/Item-Class-Documentation)
			// Common Properties
			Item.width = 66; // Hitbox width of the item.
			Item.height = 28; // Hitbox height of the item.
			Item.rare = ItemRarityID.Green; // The color that the item's name will be in-game.

			// Use Properties
			Item.useTime = 8; // The item's use time in ticks (60 ticks == 1 second.). Not necessary, but we can use it for inner projectiles.
			Item.useAnimation = 8; // The length of the item's use animation in ticks (60 ticks == 1 second.). Not necessary same as previosly.
			Item.useStyle = ItemUseStyleID.Shoot; // How you use the item (swinging, holding out, etc.)
			Item.autoReuse = true; // Whether or not you can hold click to automatically use it again. Not necessary, but if we want to repeat sound or create multiple projectiles.
			Item.UseSound = SoundID.Item13; // The sound that this item plays when used. 

			// Weapon Properties
			Item.DamageType = DamageClass.Ranged; // Sets the damage type to ranged.
			Item.damage = 50; // Sets the item's damage. Note that projectiles shot by this weapon will use its and the used ammunition's damage added together.
			Item.knockBack = 2f; // Sets the item's knockback. Note that projectiles shot by this weapon will use its and the used ammunition's knockback added together.
			Item.noMelee = true; // So the item's animation doesn't do damage.

			// Gun Properties
			Item.shoot = ModContent.ProjectileType<Projectiles.ExampleAnimatedGunProjectile>(); //It must not be null, but it can be anything. Can be use for creatin multiple projectiles. 
			Item.shootSpeed = 20f; // The speed of the inner projectile (measured in pixels per frame.)
			Item.useAmmo = AmmoID.Bullet; // The "ammo Id" of the ammo item that this weapon uses. Ammo IDs are magic numbers that usually correspond to the item id of one item that most commonly represent the ammo type.
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack) {
			// Create animated gun projectile(ExampleAnimatedGunProjectile). Speed not used because it manualy changed inside projectile script.
			// Item.useTime used as cooldown for shooting bullets inside Projectile script. Timers good choice for that stuff. (https://github.com/tModLoader/tModLoader/wiki/Basic-Projectile) 
			Projectile.NewProjectile(position.X, position.Y, speedX, speedY, ModContent.ProjectileType<Projectiles.ExampleAnimatedGunProjectile>(), damage, knockBack, player.whoAmI, Item.useTime, Item.useTime);
			// We manually create the projectiles, so we don't need the default behavior 
			return false;
		}
	}
}
