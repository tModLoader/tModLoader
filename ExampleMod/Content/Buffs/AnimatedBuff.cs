using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Buffs
{
	//This file contains two classes, the ModSystem responsible for updating the animation, and the ModBuff itself.

	//If you are not interested in making your buff animated, you don't need this ModSystem class here.
	public class AnimatedBuffSystem : ModSystem
	{
		public const int FrameCount = 4; //Amount of frames we have on our animation spritesheet.
		public const int AnimationSpeed = 60; //In ticks.

		//This is a property, in this case it allows us to restrict the setter to only this class, meaning that "AnimatedBuffSystem.Frame = 5" in another class would not let us compile.
		public static int Frame { get; private set; } = 0;

		private static int FrameCounter { get; set; } = 0;

		public override void PreUpdateEntities() {
			//This hook runs once per game tick, in the same place where other animation updates take place.
			//If we were to put this code in AnimatedBuff.PreDraw, even though vanilla makes sure to only have one buff per player,
			//it's possible that another mod might invoke drawing of buffs, which will then speed up the animation due to code running multiple times per tick.
			if (!Main.dedServ && (Main.hasFocus || Main.netMode != NetmodeID.SinglePlayer)) {
				//The above conditions are the same vanilla uses to update animations.

				FrameCounter++;
				if (FrameCounter > AnimationSpeed) {
					FrameCounter = 0;
					Frame++;
					if (Frame >= FrameCount) {
						Frame = 0;
					}
				}
			}
		}
	}

	//This buff has an extra animation spritesheet, and also showcases PreDraw specifically.
	//(We keep the autoloaded texture as one frame in case other mods need to access the buff sprite directly and aren't aware of it having special draw code).
	public class AnimatedBuff : ModBuff
	{
		private Asset<Texture2D> animatedTexture;

		public override void SetDefaults() {
			DisplayName.SetDefault("Animated Buff");
			Description.SetDefault("Animates.");

			if (Main.netMode != NetmodeID.Server) {
				//Do NOT load textures on the server!
				animatedTexture = Mod.GetTexture("Content/Buffs/AnimatedBuff_Animation");
			}
		}

		public override void Unload() {
			animatedTexture = null;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, int buffIndex, ref BuffDrawParams drawParams) {
			//We draw our special texture here, the animation takes place in AnimatedBuffSystem.

			//Use our animation spritesheet.
			Texture2D ourTexture = animatedTexture.Value;
			//Choose the frame to display.
			//We reference the frame count and frame from the ModSystem here.
			Rectangle ourSourceRectangle = ourTexture.Frame(verticalFrames: AnimatedBuffSystem.FrameCount, frameY: AnimatedBuffSystem.Frame);

			//Be aware of the fact that drawParams.mouseRectangle exists: it defaults to the size of the autoloaded buffs' sprite,
			//it handles mouseovering and clicking on the buff icon. Since our frame in the animation is 32x32 (same as the autoloaded sprite),
			//and we don't change drawParams.position, we don't have to do anything. If you offset the position, or have a non-standard size, change it accordingly.

			//We have two options here:
			//Option 1 is the recommended one, as it requires less code.
			//Option 2 allows you to customize drawing even more, but then you are on your own.

			//For demonstration, both options' codes are written down, but the latter is commented out using /* and */.

			//OPTION 1 - Let the game draw it for us. Therefore we have to assign our variables to drawParams:
			drawParams.texture = ourTexture;
			drawParams.sourceRectangle = ourSourceRectangle;
			//Return true to let the game draw the buff icon.
			return true;

			/*
			//OPTION 2 - Draw our buff manually:
			spriteBatch.Draw(ourTexture, drawParams.position, ourSourceRectangle, drawParams.drawColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

			//Return false to prevent drawing the icon, since we have already drawn it.
			return false;
			*/
		}

		public override void Update(Player player, ref int buffIndex) {
			//Increase all damage by 10%
			player.GetDamage<GenericDamageClass>() += 0.1f;
		}
	}
}