using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;

namespace ExampleMod.Content.BigProgressBars
{
	class BlueJellyfishBigProgressBar : IBigProgressBar
	{
		//Works for any NPC this is registered for, draws the jellyfish NPC textures' first frame as the icon

		private float _lifePercentToShow;

		public bool ValidateAndCollectNecessaryInfo(ref BigProgressBarInfo info) {
			if (info.npcIndexToAimAt < 0 || info.npcIndexToAimAt > 200)
				return false;

			NPC npc = Main.npc[info.npcIndexToAimAt];
			if (!npc.active)
				return false;

			_lifePercentToShow = Utils.Clamp(npc.life / (float)npc.lifeMax, 0f, 1f);
			return true;
		}

		public void Draw(ref BigProgressBarInfo info, SpriteBatch spriteBatch) {
			//Grab the blue jellyfish texture
			Texture2D value = TextureAssets.Npc[NPCID.BlueJellyfish].Value;

			//Take its first animation frame and offset the draw a bit (so it aligns with the space for the icon)
			Rectangle barIconFrame = value.Frame(verticalFrames: Main.npcFrameCount[NPCID.BlueJellyfish], frameY: 0, sizeOffsetY: 14);

			BigProgressBarHelper.DrawFancyBar(spriteBatch, _lifePercentToShow, value, barIconFrame);
		}
	}
}
