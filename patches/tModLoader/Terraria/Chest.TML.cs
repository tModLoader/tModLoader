using System;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria;

public partial class Chest
{
	/// <summary>
	/// Gets the ID identifying this chest's tile type and style, or <see cref="ChestID.None"/> if
	/// this chest is not attached to a valid basic chest tile.
	/// </summary>
	public int type {
		get {
			// World chests keep only tile coordinates, so resolve the tile style when the ID is requested.
			if (x < 0 || y < 0 || x >= Main.maxTilesX || y >= Main.maxTilesY)
				return ChestID.None;

			Tile tile = Main.tile[x, y];
			if (tile == null || !tile.HasTile || tile.TileType >= TileID.Sets.BasicChest.Length || !TileID.Sets.BasicChest[tile.TileType])
				return ChestID.None;

			return ChestID.FromTile(tile.TileType, tile.TileFrameX / 36);
		}
	}

	/// <summary>
	/// Adds the specified item to this chest, stacking with matching items before using empty slots.
	/// The condition is checked first; then the chance is rolled as numerator/denominator.
	/// Returns false if either check fails or there is not enough room for the full stack.
	/// For a live multiplayer world, call this on the server.
	/// </summary>
	/// <param name="itemType">The item type to add.</param>
	/// <param name="stack">The number of items to add.</param>
	/// <param name="condition">An optional condition that must return true before the chance is rolled.</param>
	/// <param name="chanceNumerator">The chance numerator; defaults to a guaranteed result.</param>
	/// <param name="chanceDenominator">The chance denominator; defaults to a guaranteed result.</param>
	/// <returns>Whether the requested stack was added.</returns>
	public bool AddItem(int itemType, int stack = 1, Func<bool> condition = null, int chanceNumerator = 1, int chanceDenominator = 1)
	{
		ValidateItem(itemType, stack);
		if (chanceDenominator <= 0)
			throw new ArgumentOutOfRangeException(nameof(chanceDenominator));
		if (chanceNumerator < 0 || chanceNumerator > chanceDenominator)
			throw new ArgumentOutOfRangeException(nameof(chanceNumerator), "Chance must be between zero and the denominator.");

		if (condition != null && !condition())
			return false;
		// Avoid a random roll for the guaranteed case so it does not consume worldgen RNG state.
		if (chanceNumerator == 0 || (chanceNumerator < chanceDenominator && !WorldGen.genRand.NextBool(chanceNumerator, chanceDenominator)))
			return false;

		return TryAddItem(itemType, stack);
	}

	/// <summary>
	/// Removes up to the requested number of matching items, emptying stacks as they reach zero.
	/// Returns the total number actually removed.
	/// </summary>
	/// <param name="itemType">The item type to remove.</param>
	/// <param name="stack">The maximum number to remove. Defaults to all matching items.</param>
	/// <returns>The number of items removed.</returns>
	public int RemoveItem(int itemType, int stack = int.MaxValue)
	{
		if (itemType <= ItemID.None || itemType >= ItemLoader.ItemCount)
			throw new ArgumentOutOfRangeException(nameof(itemType));
		if (stack <= 0)
			throw new ArgumentOutOfRangeException(nameof(stack));
		if (item == null)
			return 0;

		int removed = 0;
		int itemCount = Math.Min(maxItems, item.Length);
		for (int i = 0; i < itemCount && removed < stack; i++) {
			Item chestItem = item[i];
			if (chestItem == null || chestItem.IsAir || chestItem.type != itemType)
				continue;

			// Remove only as much as is still requested from this stack.
			int amount = Math.Min(chestItem.stack, stack - removed);
			chestItem.stack -= amount;
			removed += amount;
			if (chestItem.stack <= 0)
				chestItem.TurnToAir();
			SyncItem(i);
		}

		return removed;
	}

	/// <summary>
	/// Removes every item from this chest.
	/// </summary>
	/// <returns>The total number of items removed across all stacks.</returns>
	public int ClearItems()
	{
		if (item == null)
			return 0;

		int removed = 0;
		int itemCount = Math.Min(maxItems, item.Length);
		for (int i = 0; i < itemCount; i++) {
			Item chestItem = item[i];
			if (chestItem == null || chestItem.IsAir)
				continue;

			removed += chestItem.stack;
			chestItem.TurnToAir();
			SyncItem(i);
		}

		return removed;
	}

	/// <summary>
	/// Replaces every stack of <paramref name="oldItemType"/> with <paramref name="newItemType"/>.
	/// The old stacks are left untouched if the replacement cannot fit in the chest.
	/// </summary>
	/// <param name="oldItemType">The item type to replace.</param>
	/// <param name="newItemType">The item type to place in its place.</param>
	/// <returns>Whether any matching items were replaced, or true if both item types are identical.</returns>
	public bool ReplaceItem(int oldItemType, int newItemType)
	{
		if (oldItemType <= ItemID.None || oldItemType >= ItemLoader.ItemCount)
			throw new ArgumentOutOfRangeException(nameof(oldItemType));
		if (newItemType <= ItemID.None || newItemType >= ItemLoader.ItemCount)
			throw new ArgumentOutOfRangeException(nameof(newItemType));
		if (oldItemType == newItemType)
			return true;
		if (item == null)
			return false;

		long stack = 0;
		int itemCount = Math.Min(maxItems, item.Length);
		for (int i = 0; i < itemCount; i++) {
			if (item[i] != null && item[i].type == oldItemType)
				stack += item[i].stack;
		}

		// Count and validate capacity before removing anything, making a failed replacement non-destructive.
		if (stack == 0 || !CanFitItem(newItemType, stack, oldItemType))
			return false;

		RemoveItem(oldItemType);
		return TryAddItem(newItemType, (int)stack);
	}

	private bool TryAddItem(int itemType, int stack)
	{
		if (item == null || !CanFitItem(itemType, stack))
			return false;

		int itemCount = Math.Min(maxItems, item.Length);
		// Fill compatible partial stacks first, matching normal inventory behavior.
		for (int i = 0; i < itemCount && stack > 0; i++) {
			Item chestItem = item[i];
			if (chestItem == null || chestItem.type != itemType || chestItem.stack >= chestItem.maxStack)
				continue;

			int amount = Math.Min(stack, chestItem.maxStack - chestItem.stack);
			chestItem.stack += amount;
			stack -= amount;
			SyncItem(i);
		}

		// Put any remaining amount into empty slots; capacity was checked up front.
		for (int i = 0; i < itemCount && stack > 0; i++) {
			if (item[i] != null && !item[i].IsAir)
				continue;

			Item chestItem = item[i] ??= new Item();
			chestItem.SetDefaults(itemType);
			chestItem.stack = Math.Min(stack, chestItem.maxStack);
			stack -= chestItem.stack;
			SyncItem(i);
		}

		return stack == 0;
	}

	private bool CanFitItem(int itemType, long stack, int slotsFreedByItemType = ItemID.None)
	{
		if (item == null)
			return false;

		Item defaults = new();
		defaults.SetDefaults(itemType);
		if (defaults.maxStack <= 0)
			return false;

		long capacity = 0;
		int itemCount = Math.Min(maxItems, item.Length);
		for (int i = 0; i < itemCount; i++) {
			Item chestItem = item[i];
			// During replacement, slots occupied by the old type will be cleared and can be reused.
			if (chestItem == null || chestItem.IsAir || (slotsFreedByItemType != ItemID.None && chestItem.type == slotsFreedByItemType))
				capacity += defaults.maxStack;
			else if (chestItem.type == itemType)
				capacity += Math.Max(0, chestItem.maxStack - chestItem.stack);
		}

		return capacity >= stack;
	}

	private static void ValidateItem(int itemType, int stack)
	{
		// Reject invalid IDs and non-positive counts consistently for item insertion.
		if (itemType <= ItemID.None || itemType >= ItemLoader.ItemCount)
			throw new ArgumentOutOfRangeException(nameof(itemType));
		if (stack <= 0)
			throw new ArgumentOutOfRangeException(nameof(stack));
	}

	private void SyncItem(int slot)
	{
		// Worldgen is not networked; only synchronize mutations to a live server-side world chest.
		if (Main.netMode == NetmodeID.Server && !WorldGen.generatingWorld &&
			index >= 0 && index < Main.maxChests && ReferenceEquals(Main.chest[index], this))
			NetMessage.SendData(MessageID.SyncChestItem, -1, -1, null, index, slot);
	}
}
