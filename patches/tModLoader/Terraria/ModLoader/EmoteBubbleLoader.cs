using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	public static class EmoteBubbleLoader
	{
		public static int EmoteBubbleCount => emoteBubbles.Count + EmoteID.Count;
		internal static readonly List<ModEmoteBubble> emoteBubbles = new();
		internal static readonly List<GlobalEmoteBubble> globalEmoteBubbles = new();
		internal static readonly Dictionary<int, List<ModEmoteBubble>> categoryEmoteLookup = new();

		internal static int Add(ModEmoteBubble emoteBubble) {
			if (ModNet.AllowVanillaClients)
				throw new Exception("Adding emote bubbles breaks vanilla client compatibility");

			emoteBubbles.Add(emoteBubble);
			return EmoteBubbleCount - 1;
		}

		internal static void Unload() {
			emoteBubbles.Clear();
			globalEmoteBubbles.Clear();
			categoryEmoteLookup.Clear();
		}
		
		internal static void ResizeArrays() {
			Array.Resize(ref Lang._emojiNameCache, EmoteBubbleCount);

			for (int k = EmoteID.Count; k < EmoteBubbleCount; k++) {
				Lang._emojiNameCache[k] = LocalizedText.Empty;
			}
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

		internal static List<int> AddEmotesToCategory(this List<int> emotesList, int categoryId) {
			if (categoryEmoteLookup.TryGetValue(categoryId, out var modEmotes)) {
				emotesList.AddRange(from e in modEmotes where e.IsUnlocked() select e.Type);
			}

			return emotesList;
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

		public static bool UpdateFrameInEmoteMenu(int emoteType, ref int frameCounter) {
			bool result = true;
			foreach (var globalEmoteBubble in globalEmoteBubbles) {
				result &= globalEmoteBubble.UpdateFrameInEmoteMenu(emoteType, ref frameCounter);
			}
			if (result && GetEmoteBubble(emoteType)?.UpdateFrameInEmoteMenu(ref frameCounter) is false) {
				result = false;
			}
			return result;
		}

		public static bool PreDraw(EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) {
			bool result = true;
			foreach (var globalEmoteBubble in globalEmoteBubbles) {
				result &= globalEmoteBubble.PreDraw(emoteBubble, spriteBatch, texture, position, frame, origin, spriteEffects);
			}
			if (result && emoteBubble.ModEmoteBubble != null) {
				result = emoteBubble.ModEmoteBubble.PreDraw(spriteBatch, texture, position, frame, origin, spriteEffects);
			}
			return result;
		}

		public static void PostDraw(EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects) {
			foreach (var globalEmoteBubble in globalEmoteBubbles) {
				globalEmoteBubble.PostDraw(emoteBubble, spriteBatch, texture, position, frame, origin, spriteEffects);
			}
			emoteBubble.ModEmoteBubble?.PostDraw(spriteBatch, texture, position, frame, origin, spriteEffects);
		}
		
		public static bool PreDrawInEmoteMenu(int emoteType, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin) {
			bool result = true;
			foreach (var globalEmoteBubble in globalEmoteBubbles) {
				result &= globalEmoteBubble.PreDrawInEmoteMenu(emoteType, spriteBatch, uiEmoteButton, position, frame, origin);
			}
			if (result && GetEmoteBubble(emoteType)?.PreDrawInEmoteMenu(spriteBatch, uiEmoteButton, position, frame, origin) is false) {
				result = false;
			}
			return result;
		}

		public static void PostDrawInEmoteMenu(int emoteType, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin) {
			foreach (var globalEmoteBubble in globalEmoteBubbles) {
				globalEmoteBubble.PostDrawInEmoteMenu(emoteType, spriteBatch, uiEmoteButton, position, frame, origin);
			}
			GetEmoteBubble(emoteType)?.PostDrawInEmoteMenu(spriteBatch, uiEmoteButton, position, frame, origin);
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
		
		public static Rectangle? GetFrameInEmoteMenu(int emoteType, int frame, int frameCounter) {
			Rectangle? result = null;
			foreach (var globalEmoteBubble in globalEmoteBubbles) {
				var frameRect = globalEmoteBubble.GetFrameInEmoteMenu(emoteType, frame, frameCounter);
				if (frameRect != null)
					result = frameRect;
			}
			if (emoteType >= EmoteID.Count) {
				var frameRect = GetEmoteBubble(emoteType)?.GetFrameInEmoteMenu(frame, frameCounter);
				if (frameRect != null)
					result = frameRect;
			}
			return result;
		}
	}
}
