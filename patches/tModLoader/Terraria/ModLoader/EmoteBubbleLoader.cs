using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI;

namespace Terraria.ModLoader
{
	public static class EmoteBubbleLoader
	{
		public static int EmoteBubbleCount => emoteBubbles.Count + EmoteID.Count;
		internal static readonly List<ModEmoteBubble> emoteBubbles = new List<ModEmoteBubble>();
		internal static readonly List<GlobalEmoteBubble> globalEmoteBubbles = new();

		internal static int Add(ModEmoteBubble emoteBubble) {
			if (ModNet.AllowVanillaClients)
				throw new Exception("Adding emote bubbles breaks vanilla client compatibility");

			emoteBubbles.Add(emoteBubble);
			return EmoteBubbleCount - 1;
		}

		internal static void Unload() {
			emoteBubbles.Clear();
		}

		internal static Dictionary<Mod, List<int>> GetAllUnlockedModEmotes() {
			var result = new Dictionary<Mod, List<int>>();
			foreach (var modEmoteBubble in from modEmote in emoteBubbles where modEmote.IsUnlocked() select modEmote)
			{
				if (!result.TryGetValue(modEmoteBubble.Mod, out var emoteList)) {
					result[modEmoteBubble.Mod] = new List<int> { modEmoteBubble.Type };
					continue;
				}
				emoteList.Add(modEmoteBubble.Type);
			}

			return result;
		}

		/// <summary>
		/// Gets the <see cref="ModEmoteBubble"/> instance corresponding to the specified ID.
		/// </summary>
		/// <param name="type">The ID of the emote bubble</param>
		/// <returns>The <see cref="ModEmoteBubble"/> instance in the emote bubbles array, null if not found.</returns>
		public static ModEmoteBubble GetEmoteBubble(int type) {
			return type >= EmoteID.Count && type < EmoteBubbleCount ? emoteBubbles[type - EmoteID.Count] : null;
		}

		public static void OnSpawn(EmoteBubble emoteBubble) {
			if (emoteBubble.emote >= EmoteID.Count && emoteBubble.emote < EmoteBubbleCount) {
				emoteBubble.ModEmoteBubble = GetEmoteBubble(emoteBubble.emote).NewInstance(emoteBubble);
				emoteBubble.ModEmoteBubble.EmoteBubble = emoteBubble;
			}

			foreach (var globalEmoteBubble in globalEmoteBubbles) {
				globalEmoteBubble.OnSpawn(emoteBubble);
			}
			emoteBubble.ModEmoteBubble?.OnSpawn();
		}

		public static bool UpdateFrame(EmoteBubble emoteBubble) {
			bool result = true;
			foreach (var globalEmoteBubble in globalEmoteBubbles) {
				result &= globalEmoteBubble.UpdateFrame(emoteBubble);
			}
			if (result && emoteBubble.ModEmoteBubble != null) {
				result = emoteBubble.ModEmoteBubble.UpdateFrame();
			}
			return result;
		}

		public static bool PreDraw(EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, SpriteEffects spriteEffects) {
			bool result = true;
			foreach (var globalEmoteBubble in globalEmoteBubbles) {
				result &= globalEmoteBubble.PreDraw(emoteBubble, spriteBatch, texture, position, frame, spriteEffects);
			}
			if (result && emoteBubble.ModEmoteBubble != null) {
				result = emoteBubble.ModEmoteBubble.PreDraw(spriteBatch, texture, position, frame, spriteEffects);
			}
			return result;
		}

		public static void PostDraw(EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, SpriteEffects spriteEffects) {
			foreach (var globalEmoteBubble in globalEmoteBubbles) {
				globalEmoteBubble.PostDraw(emoteBubble, spriteBatch, texture, position, frame, spriteEffects);
			}
			emoteBubble.ModEmoteBubble?.PostDraw(spriteBatch, texture, position, frame, spriteEffects);
		}

		public static Rectangle? GetFrame(EmoteBubble emoteBubble) {
			Rectangle? result = null;
			foreach (var globalEmoteBubble in globalEmoteBubbles) {
				var frame = globalEmoteBubble.GetFrame(emoteBubble);
				if (frame != null)
					result = frame;
			}
			if (emoteBubble.ModEmoteBubble != null) {
				var frame = emoteBubble.ModEmoteBubble.GetFrame();
				if (frame != null)
					result = frame;
			}
			return result;
		}
	}
}
