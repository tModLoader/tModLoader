using Microsoft.Xna.Framework;
using Terraria.GameContent.UI;
using Terraria.ModLoader;

namespace ExampleMod.Content.EmoteBubbles
{
	// This abstract class is used for town NPC emotes quick setup.
	public abstract class ModTownEmote : ModEmoteBubble
	{
		// Redirecting texture path.
		public override string Texture => "ExampleMod/Content/EmoteBubbles/NPCEmotes";

		/// <summary>
		/// Which row of the sprite sheet is this NPC emote in?
		/// </summary>
		public virtual int Row => 0;

		public override void SetStaticDefaults() {
			// Add NPC emotes to "Town" category.
			AddToCategory(EmoteID.Category.Town);
		}

		// You should decide the frame rectangle yourself by these two methods.
		public override Rectangle? GetFrame() {
			return new Rectangle(EmoteBubble.frame * 34, 28 * Row, 34, 28);
		}

		// Do note that you should never use EmoteBubble instance in "Emote Menu Methods".
		// Because in that case the value of EmoteBubble is always null.
		public override Rectangle? GetFrameInEmoteMenu(int frame, int frameCounter) {
			return new Rectangle(frame * 34, 28 * Row, 34, 28);
		}
	}

	// This is a showcase of using the same texture for different emotes.
	// Names of these classes are defined using .hjson files in the Localization folder.
	public class ExamplePersonEmote : ModTownEmote
	{
		public override void OnSpawn() {
			// These code make the emote always remains longer than vanilla emotes.
			EmoteBubble.lifeTime *= 2;
			EmoteBubble.lifeTimeStart *= 2;
		}

		public override int Row => 0;
	}

	public class ExampleTravellingMerchantEmote : ModTownEmote
	{
		public override int Row => 1;
	}

	public class ExampleBoneMerchantEmote : ModTownEmote
	{
		public override int Row => 2;
	}
}
