using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria.GameContent.UI
{
	public partial class EmoteBubble
	{
		public ModEmoteBubble ModEmoteBubble { get; internal set; }

		public int WhoAmI => ID;

		public bool IsDisplayingEmote => lifeTime < 6 || lifeTimeStart - lifeTime < 6;

		/// <summary>
		/// Gets the emote bubble that exists in the world by <see cref="WhoAmI"/>. Returns null if there is no corresponding emote.
		/// </summary>
		/// <param name="whoAmI"></param>
		/// <returns></returns>
		public static EmoteBubble GetExistingEmoteBubble(int whoAmI) =>
			byID.GetValueOrDefault(whoAmI, null);
		
		/// <summary>
		/// Send a emote from the player.
		/// </summary>
		/// <param name="player"></param>
		/// <param name="emoteId"></param>
		public static void MakePlayerEmote(Player player, int emoteId) {
			if (Main.netMode is NetmodeID.Server or NetmodeID.SinglePlayer) {
				NewBubble(emoteId, new WorldUIAnchor(player), 360);
				CheckForNPCsToReactToEmoteBubble(emoteId, player);
			}
			else {
				NetMessage.SendData(MessageID.Emoji, -1, -1, null, player.whoAmI, emoteId);
			}
		}
	}
}
