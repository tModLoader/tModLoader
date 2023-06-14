using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader.Core;

namespace Terraria.ModLoader;

/// <summary>
/// This class serves as a central place from which NPC head slots are stored and NPC head textures are assigned. This can be used to obtain the corresponding slots to head textures.
/// </summary>
public static class NPCHeadLoader
{
	/// <summary>
	/// The number of vanilla town NPC head textures that exist.
	/// </summary>
	public static readonly int VanillaHeadCount = TextureAssets.NpcHead.Length;
	/// <summary>
	/// The number of vanilla boss head textures that exist.
	/// </summary>
	public static readonly int VanillaBossHeadCount = TextureAssets.NpcHeadBoss.Length;

	private static int nextHead = VanillaHeadCount;
	private static int nextBossHead = VanillaBossHeadCount;

	internal static IDictionary<string, int> heads = new Dictionary<string, int>();
	internal static IDictionary<string, int> bossHeads = new Dictionary<string, int>();
	internal static IDictionary<int, int> npcToHead = new Dictionary<int, int>();
	internal static IDictionary<int, int> npcToBossHead = new Dictionary<int, int>();

	internal static int ReserveHeadSlot() => nextHead++;

	public static int NPCHeadCount => nextHead;

	internal static int ReserveBossHeadSlot(string texture)
	{
		if (bossHeads.TryGetValue(texture, out int existing)) {
			return existing;
		}

		return nextBossHead++;
	}

	/// <summary>
	/// Gets the index of the head texture corresponding to the given texture path.
	/// </summary>
	/// <param name="texture">Relative texture path</param>
	/// <returns>The index of the texture in the heads array, -1 if not found.</returns>
	public static int GetHeadSlot(string texture) => heads.TryGetValue(texture, out int slot) ? slot : -1;

	/// <summary>
	/// Gets the index of the boss head texture corresponding to the given texture path.
	/// </summary>
	/// <param name="texture"></param>
	/// <returns></returns>
	public static int GetBossHeadSlot(string texture) => bossHeads.TryGetValue(texture, out int slot) ? slot : -1;

	internal static void ResizeAndFillArrays()
	{
		static void ResetHeadRenderer(ref NPCHeadRenderer renderer, Asset<Texture2D>[] textures)
		{
			Main.ContentThatNeedsRenderTargets.Remove(renderer);
			Main.ContentThatNeedsRenderTargets.Add(renderer = new NPCHeadRenderer(textures));
		}

		//Textures
		Array.Resize(ref TextureAssets.NpcHead, nextHead);
		Array.Resize(ref TextureAssets.NpcHeadBoss, nextBossHead);
		ResetHeadRenderer(ref Main.TownNPCHeadRenderer, TextureAssets.NpcHead);
		ResetHeadRenderer(ref Main.BossNPCHeadRenderer, TextureAssets.NpcHeadBoss);

		foreach (string texture in heads.Keys) {
			TextureAssets.NpcHead[heads[texture]] = ModContent.Request<Texture2D>(texture);
		}

		foreach (string texture in bossHeads.Keys) {
			TextureAssets.NpcHeadBoss[bossHeads[texture]] = ModContent.Request<Texture2D>(texture);
		}

		//Sets. The arrays modified here are resized in NPCLoader.
		LoaderUtils.ResetStaticMembers(typeof(NPCHeadID));

		foreach (int npc in npcToBossHead.Keys) {
			NPCID.Sets.BossHeadTextures[npc] = npcToBossHead[npc];
		}

		//Etc
		Array.Resize(ref Main.instance._npcIndexWhoHoldsHeadIndex, nextHead);
	}

	internal static void Unload()
	{
		nextHead = VanillaHeadCount;
		nextBossHead = VanillaBossHeadCount;
		heads.Clear();
		bossHeads.Clear();
		npcToHead.Clear();
		npcToBossHead.Clear();
	}
	//in Terraria.NPC.TypeToNum replace final return with this
	internal static int GetNPCHeadSlot(int type) => npcToHead.TryGetValue(type, out int slot) ? slot : -1;
}
