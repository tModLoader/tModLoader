using ExampleMod.Common.Systems;
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
		}

		public override bool IsUnlocked() => DownedBossSystem.downedMinionBoss;
	}
}
