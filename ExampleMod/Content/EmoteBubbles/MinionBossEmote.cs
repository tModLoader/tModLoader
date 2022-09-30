using ExampleMod.Common.Systems;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace ExampleMod.Content.EmoteBubbles
{
	public class MinionBossEmote : ModEmoteBubble
	{
		public override void SetStaticDefaults() {
			// As for emote command name, you will want to lowercase all letters to match the vanilla command.
			// If you don't have this set, it will be the lowercase name of the class.
			// EmoteName automatically assigned from localization files (files in Localization/),
			// but the commented line below is the normal approach.
			// EmoteName.SetDefault("exminionboss");

			// Add the emote to "bosses" category
			AddToCategory(EmoteID.Category.Dangers);
		}

		public override bool IsUnlocked() {
			// This emote only shows when minion boss is downed, just as vanilla do.
			return DownedBossSystem.downedMinionBoss;
		}
	}
}
