using ExampleMod.Content.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Weapons
{
	// ExampleHeldProjectileWeapon is a "held projectile" weapon. This means the weapon is actually a projectile. Since it is a projectile, it can have custom animation or run custom logic. Doing custom animation or logic with a regular item is sometimes difficult, which is why more advanced weapons sometimes use held projectiles. Some other "held projectile" items in ExampleMod include ExampleDrill, ExampleLastPrism, and ExampleCustomSwingSword.
	// This example is technically a bow weapon and shoots arrows, but if you are looking for a simple bow example, just use ExampleGun as a guide and change the useAmmo, there is nothing more to making a basic bow weapon.
	// This example demonstrates the weapon animation and fire rate acceleration features of the Laser Machinegun and the occasional secondary projectile feature of Vortex Beater or Phantom Phoenix.
	// This example also teaches manually picking and consuming ammo using Player.PickAmmo.
	public class ExampleHeldProjectileWeapon : ModItem
	{
		public const int HoldoutDistance = 20;

		private static Asset<Texture2D> glowTexture;

		public override void Load() {
			glowTexture = ModContent.Request<Texture2D>(Texture + "_Glow");
		}

		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.useAnimation = 20;
			Item.useTime = 20;
			// By convention, the shootSpeed of a held projectile weapon usually corresponds to how far out the projectile is held. This will unfortunately also affect the velocity of the ammo projectiles this weapon spawns, so we won't be using shootSpeed as the holdout distance in this example.
			Item.shootSpeed = 6f;
			Item.knockBack = 2f;
			Item.width = 56;
			Item.height = 26;
			Item.damage = 60;
			Item.shoot = ModContent.ProjectileType<ExampleHeldProjectileWeaponProjectile>();
			Item.useAmmo = AmmoID.Arrow;
			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(gold: 10);
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.DamageType = DamageClass.Ranged;
			Item.channel = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			// Since this item will attempt to shoot an ammo item, we need to set it back to the actual held projectile here.
			type = ModContent.ProjectileType<ExampleHeldProjectileWeaponProjectile>();

			// The velocity value provided is not correct, so we need to calculate a new velocity since velocity for held projectiles is actually the holdout offset.
			velocity = Vector2.Normalize(velocity) * HoldoutDistance;

			Projectile.NewProjectile(source, position, velocity, type, damage, knockback, Main.myPlayer);
			return false;
		}

		public override bool CanConsumeAmmo(Item ammo, Player player) {
			// This prevents the item from consuming ammo when initially used. The projectile will "spin up" and then it will consume ammo instead.
			if (player.ItemTimeIsZero) {
				return false;
			}
			return true;
		}

		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
			// Draw the glow texture when in the game world.
			Texture2D texture = glowTexture.Value;
			spriteBatch.Draw
			(
				texture,
				new Vector2
				(
					Item.position.X - Main.screenPosition.X + Item.width * 0.5f,
					Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f
				),
				new Rectangle(0, 0, texture.Width, texture.Height),
				Color.White,
				rotation,
				texture.Size() * 0.5f,
				scale,
				SpriteEffects.None,
				0f
			);
		}
	}
}
