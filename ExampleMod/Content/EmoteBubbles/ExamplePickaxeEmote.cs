using Terraria.ModLoader;

namespace ExampleMod.Content.EmoteBubbles
{
	// This emote will be randomly displayed when using ExamplePickaxe
	// See Content/Items/Tools/ExamplePickaxe.cs for letting the player use this emote
	public class ExamplePickaxeEmote : ModEmoteBubble
	{
		public override void SetStaticDefaults() => EmoteName.SetDefault("expickaxe");
	}
}
