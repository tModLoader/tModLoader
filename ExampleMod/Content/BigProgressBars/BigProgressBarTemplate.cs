using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;

namespace ExampleMod.Content.BigProgressBars
{
	class BigProgressBarTemplate : IBigProgressBar
	{
		//This class contains the same code vanilla CommonBossBigProgressBar uses. You are free to modify it as you see fit for your needs
		//See PartyZombieBigProgressBar.cs + NPCs/PartyZombie.cs for usage example

		private float _lifePercentToShow;
		private int _headIndex;

		public bool ValidateAndCollectNecessaryInfo(ref BigProgressBarInfo info) {
			if (info.npcIndexToAimAt < 0 || info.npcIndexToAimAt > Main.maxNPCs)
				return false;

			NPC npc = Main.npc[info.npcIndexToAimAt];
			if (!npc.active)
				return false;
			//These checks above are required to make sure that the game tracks a valid NPC

			int bossHeadTextureIndex = npc.GetBossHeadTextureIndex();
			if (bossHeadTextureIndex == -1)
				return false;
			//Only draws if this npc has a boss head texture

			_lifePercentToShow = Utils.Clamp(npc.life / (float)npc.lifeMax, 0f, 1f);
			_headIndex = bossHeadTextureIndex;
			return true;
		}

		public void Draw(ref BigProgressBarInfo info, SpriteBatch spriteBatch) {
			//Here you can do anything you want to draw your bar onto the screen. This example draws the boss head icon assigned previously
			//You can also make use of the existing methods available: DrawBareBonesBar and DrawFancyBar. They are not very customizable, and if you need to, 
			//it would be easier to find the code in the source (read the adaption guide on the tml wiki)
			//and adapt it to your needs instead of making it from scratch

			Texture2D value = TextureAssets.NpcHeadBoss[_headIndex].Value;
			Rectangle barIconFrame = value.Frame();
			BigProgressBarHelper.DrawFancyBar(spriteBatch, _lifePercentToShow, value, barIconFrame);
		}
	}
}
