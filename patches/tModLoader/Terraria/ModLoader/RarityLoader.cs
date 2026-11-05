#nullable enable

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria.GameContent.UI;
using Terraria.ID;

namespace Terraria.ModLoader;

// TODO: better name
public struct RarityTextContext
{
	public DynamicSpriteFont Font;
	public string Text;
	public Vector2 Position;
	public Color PrimaryColor;
	public Color OutlineColor;
	public Color? SecondaryOutlineColor;
	public float Rotation;
	public Vector2 Origin;
	public Vector2 Scale;
	public float MaxTextWidth;
	public float TextSpread;
	public Vector2[]? CharOffsets;
	public Color[]? CharColors;
}

public static class RarityLoader
{
	public static int RarityCount => ItemRarityID.Count + rarities.Count;
	internal static readonly List<ModRarity> rarities = new List<ModRarity>();

	internal static int Add(ModRarity rarity)
	{
		rarities.Add(rarity);
		return RarityCount - 1;
	}

	internal static void FinishSetup()
	{
		ItemRarity.Initialize();
	}

	public static ModRarity? GetRarity(int type)
	{
		return type >= ItemRarityID.Count && type < RarityCount ? rarities[type - ItemRarityID.Count] : null;
	}

	public static bool PreDrawText(int type, SpriteBatch sb, ref RarityTextContext textContext)
	{
		if (GetRarity(type) is not { } rarity) {
			return true;
		}

		return rarity.PreDrawText(sb, ref textContext);
	}

	public static void PostDrawText(int type, SpriteBatch sb, in RarityTextContext textContext)
	{
		if (GetRarity(type) is not { } rarity) {
			return;
		}

		rarity.PostDrawText(sb, in textContext);
	}

	internal static void Unload()
	{
		rarities.Clear();
		ItemRarity.Initialize();
	}
}
