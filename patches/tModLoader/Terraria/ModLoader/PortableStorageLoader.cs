using System;
using System.Collections.Generic;

namespace Terraria.ModLoader;

public static class PortableStorageLoader
{
	public static int StorageCount => storageDefinitions.Count;

	internal static readonly List<PortableStorage> storageDefinitions = [
		PortableStorage.PiggyBank,
		PortableStorage.Safe,
		PortableStorage.DefendersForge,
		PortableStorage.VoidVault,
	];

	private static readonly int defaultStorageCount = storageDefinitions.Count;

	static PortableStorageLoader()
	{
		RegisterDefaultStorage();
	}

	internal static int Register(PortableStorage storage)
	{
		storageDefinitions.Add(storage);
		return StorageCount - 1;
	}

	internal static void Unload()
	{
		storageDefinitions.RemoveRange(defaultStorageCount, StorageCount - defaultStorageCount);
	}

	private static void RegisterDefaultStorage()
	{
		int i = 0;
		foreach (var storage in storageDefinitions) {
			storage.Type = i++;
			ContentInstance.Register(storage);
			ModTypeLookup<PortableStorage>.Register(storage);
		}
	}

	public static PortableStorage GetPortableStorage(int type)
	{
		if (type < 0) {
			throw new InvalidOperationException("GetPortableStorage does not accept negative numbers, if this is a chest index then use GetPortableStorageFromChest");
		}

		return type < storageDefinitions.Count ? storageDefinitions[type] : null;
	}

	public static PortableStorage GetPortableStorageFromChest(int chestIndex)
	{
		if (chestIndex >= 0) {
			throw new InvalidOperationException("GetPortableStorageFromChests does not accept positive values, if this is a type then use GetPortableStorage");
		}

		return GetPortableStorage(ReverseIds(chestIndex));
	}

	// Swap between positive PortableStorage::Type and negative BankID.
	internal static int ReverseIds(int type) => -(type + 2);
}
