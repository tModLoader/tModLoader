using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace ExampleMod.Content.EmoteBubbles
{
	// This emote is displayed whenever ExampleZombieThief steals an Example Item
	public class ExampleItemEmote : ModEmoteBubble
	{
		public override void SetStaticDefaults() {
			AddToCategory(EmoteID.Category.Items);
		}
	}
}
