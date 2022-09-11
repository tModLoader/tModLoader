using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ExampleMod.Content.EmoteBubbles
{
	// This is a showcase of using the same texture for different emotes.
	// Names of these classes are defined using .hjson files in the Localization folder.
	public class ExamplePersonEmote : ModEmoteBubble
	{
		public override string Texture => "ExampleMod/Content/EmoteBubbles/NPCEmotes";

		public override void OnSpawn() {
			// These code make the emote always remains longer than vanilla emotes.
			EmoteBubble.lifeTime *= 2;
			EmoteBubble.lifeTimeStart *= 2;
		}

		// You should decide the frame rectangle yourself by these two methods.
		public override Rectangle? GetFrame() => new Rectangle(EmoteBubble.frame * 34, 0, 34, 28);

		// Do note that you should never use EmoteBubble instance in "Emote Menu Methods".
		// Because in that case the value of EmoteBubble is always null.
		public override Rectangle? GetFrameInEmoteMenu(int frame, int frameCounter) =>
			new Rectangle(frame * 34, 0, 34, 28);
	}

	public class ExampleTravellingMerchantEmote : ModEmoteBubble
	{
		public override string Texture => "ExampleMod/Content/EmoteBubbles/NPCEmotes";

		// Note the differences between y parameters.
		public override Rectangle? GetFrame() => new Rectangle(EmoteBubble.frame * 34, 28, 34, 28);

		public override Rectangle? GetFrameInEmoteMenu(int frame, int frameCounter) =>
			new Rectangle(frame * 34, 28, 34, 28);
	}

	public class ExampleBoneMerchantEmote : ModEmoteBubble
	{
		public override string Texture => "ExampleMod/Content/EmoteBubbles/NPCEmotes";

		public override Rectangle? GetFrame() => new Rectangle(EmoteBubble.frame * 34, 56, 34, 28);

		public override Rectangle? GetFrameInEmoteMenu(int frame, int frameCounter) =>
			new Rectangle(frame * 34, 56, 34, 28);
	}
}
