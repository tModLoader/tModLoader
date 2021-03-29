using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// This Animated Gun based on Projectile
	public class ExampleAnimatedGunProjectile: ModProjectile
	{
		public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.VortexBeater;
		public override void SetDefaults() {
			Projectile.width = 68; // The width of projectile hitbox
			Projectile.height = 30; // The height of projectile hitbox
			Projectile.aiStyle = 0; // The ai style of the projectile, please reference the source code of Terraria (0 - to manually change behavior )
			Projectile.friendly = true; // Can the projectile deal damage to enemies?
			Projectile.hostile = false;  // Can the projectile deal damage to the player?
			Projectile.tileCollide = false; // Can the projectile collide with tiles?
			Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?


			Projectile.penetrate = -1; //How many monsters the projectile can penetrate. This is a gun, so it cannot penetrate anything. 
			Main.projFrames[Projectile.type] = 7; // How many frames of animation 

			Projectile.hide = true; // This property is required for special rendering of projectiles. Without it, the rendered gun can be a little laggy. 
		}


		public override void AI() {
			// Get current player and mounted point(The point around which the hand rotates)
			Player player = Main.player[Projectile.owner];
			Vector2 mountedCenter = player.RotatedRelativePoint(player.MountedCenter);

			// Looping animation
			if (++Projectile.frameCounter >= 4) {
				Projectile.frameCounter = 0;
				if (++Projectile.frame >= Main.projFrames[Projectile.type])
					Projectile.frame = 0;
			}

			//Using Timers for implementing shoot cooldowns
			bool cooldown = true;
			Projectile.ai[1] += 1f;
			if (Projectile.ai[1] >= Projectile.ai[0]) {
				Projectile.ai[1] = 0;
				cooldown = false;
			}

			//If cooldown is over and we inside current player(for multiplayer)
			if (!cooldown && Main.myPlayer == Projectile.owner) {

				// We can shoot if the button is pressed and there is still ammunition, and the player has items.
				bool canShoot = player.channel && player.HasAmmo(player.inventory[player.selectedItem], canUse: true) && !player.noItems && !player.CCed;

				if (canShoot) {
					//Setting properties for bullets. This can be moved to the fields of the class
					int projToShoot = ProjectileID.Bullet;
					int damage = player.GetWeaponDamage(player.inventory[player.selectedItem]);
					float knockBack = player.GetWeaponKnockback(player.inventory[player.selectedItem], player.inventory[player.selectedItem].knockBack);
					float speed = player.inventory[player.selectedItem].shootSpeed;
					
					//Using ammo
					player.PickAmmo(player.inventory[player.selectedItem], ref projToShoot, ref speed, ref canShoot, ref damage, ref knockBack);
					
					Vector2 rayFromMountedCenterToMouseCoordinates = Main.MouseWorld - mountedCenter;
					//We normalize the ray so it coordinates from 0 to 1
					Vector2 rotatingPoint = Vector2.Normalize(rayFromMountedCenterToMouseCoordinates);
					//Offset the sprite from the ray (it cannot be (0, 0)). If you need to correct the position of the sprite
					rotatingPoint *= new Vector2(x: 20, y: 20);

					//Sync in mulriplayer when velocity is change
					if (rotatingPoint.X != Projectile.velocity.X || rotatingPoint.Y != Projectile.velocity.Y)
						Projectile.netUpdate = true;
					Projectile.velocity = rotatingPoint;

					//Create projectile of bullet
					Projectile.NewProjectile(mountedCenter, rayFromMountedCenterToMouseCoordinates, projToShoot, damage, knockBack, Projectile.owner);
				}
				else {
					Projectile.Kill();
				}
			}
			// Updating projectile parameters
			Projectile.position = player.RotatedRelativePoint(player.MountedCenter, false, false) - Projectile.Size / 2f;
			Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == -1 ? (float)Math.PI : 0f);
			Projectile.spriteDirection = Projectile.direction;
			
			player.ChangeDir(Projectile.direction); // Update player direction
			player.heldProj = Projectile.whoAmI; // The sprite is drawn in hand (If disabled, the sprite will be behind the player)
			Projectile.timeLeft = 2; // As long as the weapon can shoot, we update the life time
			player.SetDummyItemTime(2); // Always setting Item.useTime, Item.useAnimation to 2 frames so the Shoot method will never be called once again
			// It is necessary for the player to understand the position of the item. (for example, how to rotate hand)
			player.itemRotation = MathHelper.WrapAngle((float)Math.Atan2(Projectile.velocity.Y * (float)Projectile.direction, Projectile.velocity.X * (float)Projectile.direction)); 
		}
	}
}
