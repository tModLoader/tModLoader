using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Chat.Commands;
using Terraria.GameContent.UI;
using Terraria.GameContent.UI.Elements;
using Terraria.Initializers;
using Terraria.Localization;

namespace Terraria.ModLoader;

public static class EmoteBubbleLoader
{
	public static int EmoteBubbleCount => emoteBubbles.Count + EmoteID.Count;
	internal static readonly List<ModEmoteBubble> emoteBubbles = new();
	internal static readonly List<GlobalEmoteBubble> globalEmoteBubbles = new();
	internal static readonly Dictionary<int, List<ModEmoteBubble>> categoryEmoteLookup = new();

	internal static int Add(ModEmoteBubble emoteBubble)
	{
		emoteBubbles.Add(emoteBubble);
		return EmoteBubbleCount - 1;
	}

	internal static void Unload()
	{
		emoteBubbles.Clear();
		globalEmoteBubbles.Clear();
		categoryEmoteLookup.Clear();
	}

	internal static void ResizeArrays()
	{
		Array.Resize(ref Lang._emojiNameCache, EmoteBubbleCount);

		for (int k = EmoteID.Count; k < EmoteBubbleCount; k++) {
			Lang._emojiNameCache[k] = LocalizedText.Empty;
		}
	}

	internal static void FinishSetup()
	{
		foreach (ModEmoteBubble emoteBubble in emoteBubbles) {
			Lang._emojiNameCache[emoteBubble.Type] = emoteBubble.Command;

			if (emoteBubble.Command != LocalizedText.Empty) // TODO: does this work?
				EmojiCommand._byName[emoteBubble.Command] = emoteBubble.Type;
		}
	}

	internal static Dictionary<Mod, List<int>> GetAllUnlockedModEmotes()
	{
		var result = new Dictionary<Mod, List<int>>();
		foreach (var modEmote in emoteBubbles.Where(e => e.IsUnlocked())) {
			if (!result.TryGetValue(modEmote.Mod, out var emoteList))
				result[modEmote.Mod] = emoteList = new();
			emoteList.Add(modEmote.Type);
		}

		return result;
	}

	// Uses extension method so that patches are minimal.
	internal static List<int> AddEmotesToCategory(this List<int> emotesList, int categoryId)
	{
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
	public static ModEmoteBubble GetEmoteBubble(int type)
	{
		return type >= EmoteID.Count && type < EmoteBubbleCount ? emoteBubbles[type - EmoteID.Count] : null;
	}

	public static void OnSpawn(EmoteBubble emoteBubble)
	{
		if (emoteBubble.emote >= EmoteID.Count && emoteBubble.emote < EmoteBubbleCount) {
			emoteBubble.ModEmoteBubble = GetEmoteBubble(emoteBubble.emote).NewInstance(emoteBubble);
		}

		foreach (var globalEmoteBubble in globalEmoteBubbles) {
			globalEmoteBubble.OnSpawn(emoteBubble);
		}
		emoteBubble.ModEmoteBubble?.OnSpawn();
	}

	public static bool UpdateFrame(EmoteBubble emoteBubble)
	{
		bool result = true;
		foreach (var globalEmoteBubble in globalEmoteBubbles) {
			result &= globalEmoteBubble.UpdateFrame(emoteBubble);
		}
		if (!result)
			return false;

		return emoteBubble.ModEmoteBubble?.UpdateFrame() ?? true;
	}

	public static bool UpdateFrameInEmoteMenu(int emoteType, ref int frameCounter)
	{
		bool result = true;
		foreach (var globalEmoteBubble in globalEmoteBubbles) {
			result &= globalEmoteBubble.UpdateFrameInEmoteMenu(emoteType, ref frameCounter);
		}
		if (!result)
			return false;

		return GetEmoteBubble(emoteType)?.UpdateFrameInEmoteMenu(ref frameCounter) ?? true;
	}

	public static bool PreDraw(EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects)
	{
		bool result = true;
		foreach (var globalEmoteBubble in globalEmoteBubbles) {
			result &= globalEmoteBubble.PreDraw(emoteBubble, spriteBatch, texture, position, frame, origin, spriteEffects);
		}
		if (!result)
			return false;

		return emoteBubble.ModEmoteBubble?.PreDraw(spriteBatch, texture, position, frame, origin, spriteEffects) ?? true;
	}

	public static void PostDraw(EmoteBubble emoteBubble, SpriteBatch spriteBatch, Texture2D texture, Vector2 position, Rectangle frame, Vector2 origin, SpriteEffects spriteEffects)
	{
		foreach (var globalEmoteBubble in globalEmoteBubbles) {
			globalEmoteBubble.PostDraw(emoteBubble, spriteBatch, texture, position, frame, origin, spriteEffects);
		}
		emoteBubble.ModEmoteBubble?.PostDraw(spriteBatch, texture, position, frame, origin, spriteEffects);
	}

	public static bool PreDrawInEmoteMenu(int emoteType, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin)
	{
		bool result = true;
		foreach (var globalEmoteBubble in globalEmoteBubbles) {
			result &= globalEmoteBubble.PreDrawInEmoteMenu(emoteType, spriteBatch, uiEmoteButton, position, frame, origin);
		}
		if (!result)
			return false;

		return GetEmoteBubble(emoteType)?.PreDrawInEmoteMenu(spriteBatch, uiEmoteButton, position, frame, origin) ?? true;
	}

	public static void PostDrawInEmoteMenu(int emoteType, SpriteBatch spriteBatch, EmoteButton uiEmoteButton, Vector2 position, Rectangle frame, Vector2 origin)
	{
		foreach (var globalEmoteBubble in globalEmoteBubbles) {
			globalEmoteBubble.PostDrawInEmoteMenu(emoteType, spriteBatch, uiEmoteButton, position, frame, origin);
		}
		GetEmoteBubble(emoteType)?.PostDrawInEmoteMenu(spriteBatch, uiEmoteButton, position, frame, origin);
	}


	public static Rectangle? GetFrame(EmoteBubble emoteBubble)
	{
		if (emoteBubble.ModEmoteBubble != null) {
			return emoteBubble.ModEmoteBubble.GetFrame();
		}
		Rectangle? result = null;
		foreach (var globalEmoteBubble in globalEmoteBubbles) {
			var frameRect = globalEmoteBubble.GetFrame(emoteBubble);
			if (frameRect != null)
				result = frameRect;
		}
		return result;
	}

	public static Rectangle? GetFrameInEmoteMenu(int emoteType, int frame, int frameCounter)
	{
		if (emoteType >= EmoteID.Count) {
			return GetEmoteBubble(emoteType)?.GetFrameInEmoteMenu(frame, frameCounter);
		}
		Rectangle? result = null;
		foreach (var globalEmoteBubble in globalEmoteBubbles) {
			var frameRect = globalEmoteBubble.GetFrameInEmoteMenu(emoteType, frame, frameCounter);
			if (frameRect != null)
				result = frameRect;
		}
		return result;
	}
}

