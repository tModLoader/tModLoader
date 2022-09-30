﻿using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria.GameContent.UI
{
	public partial class EmoteBubble
	{
		public ModEmoteBubble ModEmoteBubble { get; internal set; }

		/// <summary>
		/// The whoAmI indicator that indicates this <see cref="EmoteBubble"/>, can be used in <see cref="GetExistingEmoteBubble"/>
		/// </summary>
		public int WhoAmI => ID;

		/// <summary>
		/// Whether or not this emote is fully displayed
		/// <br>The first and the last 6 frames are for bubble-popping animation. The emote content is displayed after the animation</br>
		/// </summary>
		public bool IsFullyDisplayed => lifeTime >= 6 && lifeTimeStart - lifeTime >= 6;

		/// <summary>
		/// Gets the emote bubble that exists in the world by <see cref="WhoAmI"/>. Returns null if there is no corresponding emote
		/// </summary>
		/// <param name="whoAmI"></param>
		/// <returns></returns>
		public static EmoteBubble GetExistingEmoteBubble(int whoAmI) => byID.GetValueOrDefault(whoAmI);

		/// <summary>
		/// Try to get <see cref="WhoAmI"/> by a specific emote type
		/// </summary>
		/// <param name="type"></param>
		/// <returns>Returns the <see cref="WhoAmI"/> of the <see cref="EmoteBubble"/>. Returns -1 if there is none</returns>
		public static int TryGetFirst(int type) {
			var emote = byID.FirstOrDefault(i => i.Value.emote == type);
			return emote.Value is not null ? emote.Key : -1;
		}
		
		/// <summary>
		/// Send a emote from the player
		/// </summary>
		/// <param name="player"></param>
		/// <param name="emoteId"></param>
		/// <param name="syncBetweenClients">If true, this emote will be automatically synchronized between clients</param>
		public static void MakePlayerEmote(Player player, int emoteId, bool syncBetweenClients = true) {
			if (Main.netMode is NetmodeID.Server or NetmodeID.SinglePlayer || !syncBetweenClients) {
				NewBubble(emoteId, new WorldUIAnchor(player), 360);
				CheckForNPCsToReactToEmoteBubble(emoteId, player);
			}
			else {
				NetMessage.SendData(MessageID.Emoji, -1, -1, null, player.whoAmI, emoteId);
			}
		}
	}
}
