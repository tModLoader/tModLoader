using ExampleMod.Content.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ExampleMod.Content.BossBars
{
	//Shows basic single-entity boss bar code using a custom colored texture
	public class PartyZombieBossBar : ModBossBar
	{
		protected override IEnumerable<int> InitializeValidNPCs() {
			return new List<int> { ModContent.NPCType<PartyZombie>() };
		}

		public override Asset<Texture2D> GetIconTexture(ref Rectangle? iconFrame) {
			return TextureAssets.NpcHead[36]; //Corgi head icon
		}

		public override bool PreDraw(SpriteBatch spriteBatch, NPC npc, ref BossBarDrawParams drawParams) {
			//Make the bar shake the less health the NPC has
			float shakeIntensity = Utils.Clamp(1f - drawParams.lifePercentToShow - 0.2f, 0f, 1f);
			drawParams.barCenter.Y -= 20f;
			drawParams.barCenter += new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f)) * shakeIntensity * 15f;

			drawParams.iconColor = Main.DiscoColor;

			return true;
		}
	}
}
