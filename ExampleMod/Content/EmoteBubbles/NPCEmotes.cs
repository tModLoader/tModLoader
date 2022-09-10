using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ExampleMod.Content.EmoteBubbles
{
	public class ExamplePersonEmote : ModEmoteBubble
	{
		public override string Texture => "ExampleMod/Content/EmoteBubbles/NPCEmotes";

		public override void OnSpawn() {
			// These code make the emote always remains longer than vanilla emotes.
			EmoteBubble.lifeTime *= 2;
			EmoteBubble.lifeTimeStart *= 2;
		}

		public override Rectangle? GetFrame() => new Rectangle(EmoteBubble.frame * 34, 0, 34, 28);

		public override Rectangle? GetFrameInEmoteMenu(int frame, int frameCounter) =>
			new Rectangle(frame * 34, 0, 34, 28);
	}

	public class ExampleTravellingMerchantEmote : ModEmoteBubble
	{
		public override string Texture => "ExampleMod/Content/EmoteBubbles/NPCEmotes";

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
