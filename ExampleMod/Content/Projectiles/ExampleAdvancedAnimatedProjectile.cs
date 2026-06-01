using ExampleMod.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles
{
	// This file shows an animated projectile
	// This file also shows advanced drawing to center the drawn projectile correctly
	public class ExampleAdvancedAnimatedProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			// Total count animation frames
			Main.projFrames[Type] = 4;
		}

		public override void SetDefaults() {
			Projectile.width = 40; // The width of projectile hitbox
			Projectile.height = 40; // The height of projectile hitbox

			Projectile.friendly = true; // Can the projectile deal damage to enemies?
			Projectile.DamageType = DamageClass.Melee; // Is the projectile shoot by a ranged weapon?
			Projectile.ignoreWater = true; // Does the projectile's speed be influenced by water?
			Projectile.tileCollide = false; // Can the projectile collide with tiles?
			Projectile.penetrate = -1; // Look at comments ExamplePiercingProjectile

			Projectile.alpha = 255; // How transparent to draw this projectile. 0 to 255. 255 is completely transparent.
		}

		// Allows you to determine the color and transparency in which a projectile is drawn
		// Return null to use the default color (normally light and buff color)
		// Returns null by default.
		public override Color? GetAlpha(Color lightColor) {
			// return Color.White;
			return new Color(255, 255, 255, 0) * Projectile.Opacity;
		}

		public override void AI() {
			// All projectiles have timers that help to delay certain events
			// Projectile.ai[0], Projectile.ai[1] — timers that are automatically synchronized on the client and server
			// Projectile.localAI[0], Projectile.localAI[0] — only on the client
			// In this example, a timer is used to control the fade in / out and despawn of the projectile
			Projectile.ai[0] += 1f;

			FadeInAndOut();

			// Slow down
			Projectile.velocity *= 0.98f;

			// Loop through the 4 animation frames, spending 5 ticks on each
			// Projectile.frame — index of current frame
			if (++Projectile.frameCounter >= 5) {
				Projectile.frameCounter = 0;
				// Or more compactly Projectile.frame = ++Projectile.frame % Main.projFrames[Type];
				if (++Projectile.frame >= Main.projFrames[Type])
					Projectile.frame = 0;
			}

			// Despawn this projectile after 1 second (60 ticks)
			// You can use Projectile.timeLeft = 60f in SetDefaults() for same goal
			if (Projectile.ai[0] >= 60f)
				Projectile.Kill();

			// Set both direction and spriteDirection to 1 or -1 (right and left respectively)
			// Projectile.direction is automatically set correctly in Projectile.Update, but we need to set it here or the textures will draw incorrectly on the 1st frame.
			Projectile.direction = Projectile.spriteDirection = (Projectile.velocity.X > 0f) ? 1 : -1;

			Projectile.rotation = Projectile.velocity.ToRotation();
			// Since our sprite has an orientation, we need to adjust rotation to compensate for the draw flipping
			if (Projectile.spriteDirection == -1) {
				Projectile.rotation += MathHelper.Pi;
				// For vertical sprites use MathHelper.PiOver2
			}
		}

		// Many projectiles fade in so that when they spawn they don't overlap the gun muzzle they appear from
		public void FadeInAndOut() {
			// If last less than 50 ticks — fade in, than more — fade out
			if (Projectile.ai[0] <= 50f) {
				// Fade in
				Projectile.alpha -= 25;
				// Cap alpha before timer reaches 50 ticks
				if (Projectile.alpha < 100)
					Projectile.alpha = 100;

				return;
			}

			// Fade out
			Projectile.alpha += 25;
			// Cal alpha to the maximum 255(complete transparent)
			if (Projectile.alpha > 255)
				Projectile.alpha = 255;
		}

		// Some advanced drawing because the texture image isn't centered or symmetrical
		// If you don't want to manually draw you can use vanilla projectile rendering offsets
		// Here you can check it https://github.com/tModLoader/tModLoader/wiki/Basic-Projectile#horizontal-sprite-example
		public override bool PreDraw(ref Color lightColor) {
			// SpriteEffects helps to flip texture horizontally and vertically
			SpriteEffects spriteEffects = SpriteEffects.None;
			if (Projectile.spriteDirection == -1)
				spriteEffects = SpriteEffects.FlipHorizontally;

			// Getting texture of projectile
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			// Calculating frameHeight and current Y pos dependence of frame
			// If texture without animation frameHeight is always texture.Height and startY is always 0
			int frameHeight = texture.Height / Main.projFrames[Type];
			int startY = frameHeight * Projectile.frame;

			// Get this frame on texture
			Rectangle sourceRectangle = new Rectangle(0, startY, texture.Width, frameHeight);

			// Alternatively, you can skip defining frameHeight and startY and use this:
			// Rectangle sourceRectangle = texture.Frame(1, Main.projFrames[Type], frameY: Projectile.frame);

			Vector2 origin = sourceRectangle.Size() / 2f;

			// If image isn't centered or symmetrical you can specify origin of the sprite
			// (0,0) for the upper-left corner
			float offsetX = 20f;
			origin.X = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Width - offsetX : offsetX);

			// If sprite is vertical
			// float offsetY = 20f;
			// origin.Y = (float)(Projectile.spriteDirection == 1 ? sourceRectangle.Height - offsetY : offsetY);


			// Applying lighting and draw current frame
			Color drawColor = Projectile.GetAlpha(lightColor);
			Main.EntitySpriteDraw(texture,
				Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY),
				sourceRectangle, drawColor, Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

			// It's important to return false, otherwise we also draw the original texture.
			return false;
		}
	}

	// This is a simple item that is based on the NebulaBlaze and shoots ExampleAdvancedAnimatedProjectile to showcase it.
	internal class ExampleAdvancedAnimatedProjectileItem : ModItem
	{
		public override string Texture => $"Terraria/Images/Item_{ItemID.NebulaBlaze}";

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.NebulaBlaze);
			Item.mana = 3;
			Item.damage = 3;
			Item.shoot = ModContent.ProjectileType<ExampleAdvancedAnimatedProjectile>();
		}
		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<ExampleItem>()
				.AddTile<Tiles.Furniture.ExampleWorkbench>()
				.Register();
		}
	}
}
