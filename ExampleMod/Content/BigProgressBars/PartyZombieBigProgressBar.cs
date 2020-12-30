using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;

namespace ExampleMod.Content.BigProgressBars
{
	class PartyZombieBigProgressBar : IBigProgressBar
	{
		//Works for any NPC this is registered for, draws the Confetti item texture as the icon

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
			Texture2D value = TextureAssets.Item[ItemID.Confetti].Value;
			Rectangle barIconFrame = value.Frame();
			BigProgressBarHelper.DrawFancyBar(spriteBatch, _lifePercentToShow, value, barIconFrame);
		}
	}
}
