using ExampleMod.Common.Systems;
using Terraria.ModLoader;

namespace ExampleMod.Content.EmoteBubbles
{
	public class MinionBossEmote : ModEmoteBubble
	{
		public override bool IsUnlocked() => DownedBossSystem.downedMinionBoss;
	}
}
