using ExampleMod.Common.Systems;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace ExampleMod.Content.EmoteBubbles
{
	public class MinionBossEmote : ModEmoteBubble
	{
		public override void SetStaticDefaults() {
			// This is the emote command name for this emote.
			// You will want to lowercase all letters to match the vanilla command.
			// If you don't have this set, it will be the lowercase name of the class.
			EmoteName.SetDefault("minionboss");

			// Add the emote to "bosses" category
			AddToCategory(EmoteID.Category.Dangers);
		}

		public override bool IsUnlocked() {
			return DownedBossSystem.downedMinionBoss;
		}
	}
}
