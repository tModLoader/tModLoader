using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	// This buff has an extra animation spritesheet, and also showcases PreDraw specifically.
	// (We keep the autoloaded texture as one frame in case other mods need to access the buff sprite directly and aren't aware of it having special draw code).
	public class AnimatedBuff : ModBuff
	{
		// Some constants we define to make our life easier.
		public static readonly int FrameCount = 4; // Amount of frames we have on our animation spritesheet.
		public static readonly int AnimationSpeed = 60; // In ticks.
		public static readonly string AnimationSheetPath = "ExampleMod/Content/Buffs/AnimatedBuff_Animation";

		public static readonly int DamageBonus = 10;

		private Asset<Texture2D> animatedTexture;

		public override LocalizedText Description => base.Description.WithFormatArgs(DamageBonus);

		public override void SetStaticDefaults() {
			animatedTexture = ModContent.Request<Texture2D>(AnimationSheetPath);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams) {
			// You can use this hook to make something special happen when the buff icon is drawn (such as reposition it, pick a different texture, etc.).

			// We draw our special texture here with a specific animation.

			// Use our animation spritesheet.
			Texture2D ourTexture = animatedTexture.Value;
			// Choose the frame to display, here based on constants and the game's tick count.
			Rectangle ourSourceRectangle = ourTexture.Frame(verticalFrames: FrameCount, frameY: (int)Main.GameUpdateCount / AnimationSpeed % FrameCount);

			// Other stuff you can do in this hook
			/*
			// Here we make the icon have a lime green tint.
			drawParams.drawColor = Color.LimeGreen * Main.buffAlpha[buffIndex];
			*/

			// Be aware of the fact that drawParams.mouseRectangle exists: it defaults to the size of the autoloaded buffs' sprite,
			// it handles mouseovering and clicking on the buff icon. Since our frame in the animation is 32x32 (same as the autoloaded sprite),
			// and we don't change drawParams.position, we don't have to do anything. If you offset the position, or have a non-standard size, change it accordingly.

			// We have two options here:
			// Option 1 is the recommended one, as it requires less code.
			// Option 2 allows you to customize drawing even more, but then you are on your own.

			// For demonstration, both options' codes are written down, but the latter is commented out using /* and */.

			// OPTION 1 - Let the game draw it for us. Therefore we have to assign our variables to drawParams:
			drawParams.Texture = ourTexture;
			drawParams.SourceRectangle = ourSourceRectangle;
			// Return true to let the game draw the buff icon.
			return true;

			/*
			// OPTION 2 - Draw our buff manually:
			spriteBatch.Draw(ourTexture, drawParams.position, ourSourceRectangle, drawParams.drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			// Return false to prevent drawing the icon, since we have already drawn it.
			return false;
			*/
		}

		public override void Update(Player player, ref int buffIndex) {
			// Increase all damage by 10%
			player.GetDamage<GenericDamageClass>() += DamageBonus / 100f;
		}
	}
}
