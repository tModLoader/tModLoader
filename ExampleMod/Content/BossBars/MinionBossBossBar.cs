using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.GameContent.UI.BigProgressBar;
using ExampleMod.Content.NPCs.MinionBoss;

namespace ExampleMod.Content.BossBars
{
	//Showcases a non-basic custom boss bar
	public class MinionBossBossBar : ModBossBar
	{
		private int bossHeadIndex = -1;

		protected override IEnumerable<int> InitializeValidNPCs() {
			return new List<int> { ModContent.NPCType<MinionBossBody>() };
			//We don't include the minion here as it's not an essential "segment" or body part of the boss itself (unlike Twins, Moonlord, Golem etc.)
		}

		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			//Display the previously assigned head index
			if (bossHeadIndex != -1) {
				return TextureAssets.NpcHeadBoss[bossHeadIndex];
			}
			return null;
		}

		public override bool? ModifyInfo(ref BigProgressBarInfo info, ref float lifePercent, ref float shieldPercent) {
			//Here the game wants to know if to draw the boss bar or not. Return false whenever the conditions don't apply. 
			//If there is no possibility of returning false (or null) the bar will get drawn at times when it shouldn't

			NPC npc = Main.npc[info.npcIndexToAimAt];
			if (!npc.active)
				return false;

			//We assign bossHeadIndex here because we need to use it in GetIconTexture
			bossHeadIndex = npc.GetBossHeadTextureIndex();

			lifePercent = Utils.Clamp(npc.life / (float)npc.lifeMax, 0f, 1f);

			if (npc.ModNPC is MinionBossBody body) {
				//We did all the calculation work on RemainingShields inside the body NPC already so we just have to fetch the value again
				shieldPercent = Utils.Clamp(body.RemainingShields, 0f, 1f);
			}

			return true;
		}
	}
}
