using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	public class GuideGlobalNPC : GlobalNPC
	{
		public override bool AppliesToEntity(NPC npc, bool lateInstantiation) {
			return npc.type == NPCID.Guide;
		}

		public override void AI(NPC npc) {
			// Make the guide giant and green.
			npc.scale = 1.5f;
			npc.color = Color.ForestGreen;
		}

		public override void EmoteBubblePosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) {
			// Flip and move his emote bubble to the front of him.
			spriteEffects = npc.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
			position.X += npc.width * npc.spriteDirection;
		}

		public override void PartyHatPosition(NPC npc, ref Vector2 position, ref SpriteEffects spriteEffects) {
			// Move where the party hat is on the Guide.
			position.Y -= npc.height / 2f;
			position.X += 4 * npc.spriteDirection;
		}
	}
}
