using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace ExampleMod.Content.EmoteBubbles
{
	// This emote is displayed whenever ExampleZombieThief steals an Example Item
	public class ExampleItemEmote : ModEmoteBubble
	{
		public override void SetStaticDefaults() {
			EmoteName.SetDefault("exitem");
			AddToCategory(EmoteID.Category.Items);
		}
	}
}
