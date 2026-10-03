using System;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace Terraria.ModLoader;

/// <summary>
/// The drop rules for a single pot break. Each rule runs independently, just like item loot.
/// This list is rebuilt for each pot, so changes do not require unloading cleanup.
/// </summary>
public sealed class PotLoot : ILoot
{
	private readonly List<IItemDropRule> rules = new();

	/// <summary>The tile type of the pot.</summary>
	public int TileType { get; }

	/// <summary>The pot style calculated from its vertical frame (frameY / 36).</summary>
	public int Style { get; }

	/// <summary>The leftmost tile coordinate of the pot.</summary>
	public int X { get; }

	/// <summary>The topmost tile coordinate of the pot.</summary>
	public int Y { get; }

	/// <summary>The closest player, not necessarily the player who broke the pot.</summary>
	public Player Player { get; }

	internal PotLoot(int tileType, int style, int x, int y, Player player)
	{
		TileType = tileType;
		Style = style;
		X = x;
		Y = y;
		Player = player;
	}

	public List<IItemDropRule> Get(bool includeGlobalDrops = true) => new(rules);

	public IItemDropRule Add(IItemDropRule entry)
	{
		rules.Add(entry);
		return entry;
	}

	public IItemDropRule Remove(IItemDropRule entry)
	{
		rules.Remove(entry);
		return entry;
	}

	public void RemoveWhere(Predicate<IItemDropRule> predicate, bool includeGlobalDrops = true) => rules.RemoveAll(predicate);

	/// <summary>Removes all additional rules currently registered for this pot.</summary>
	public void Clear() => rules.Clear();
}
