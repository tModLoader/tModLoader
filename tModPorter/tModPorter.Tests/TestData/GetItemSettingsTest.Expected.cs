using System;
using Terraria;
using Settings = Terraria.GetItemSettings;
using static Terraria.GetItemSettings;

public class GetItemSettingsTest
{
	void Presets() {
		_ = GetItemSettings.QuickTransferFromSlot;
		_ = GetItemSettings.GiftRecieved;
		_ = GetItemSettings.LootAllFromBank;
		_ = GetItemSettings.LootAllFromChest;
		_ = GetItemSettings.ReturnItemFromSlot;
		_ = GetItemSettings.ReturnItemShowAsNew;
		_ = default(GetItemSettings);
		_ = Settings.QuickTransferFromSlot;
		_ = global::Terraria.GetItemSettings.LootAllFromBank;
		_ = ReturnItemShowAsNew;
		_ = default(GetItemSettings);
		_ = default(GetItemSettings);
		_ = GetItemSettings.ReturnItemFromSlot;
		_ = default(GetItemSettings);
		_ = nameof(GetItemSettings.ReturnItemShowAsNew);
	}

	void Constructors(Action<Item> callback) {
		_ = new GetItemSettings(true, false, true, StepAfterHandlingSlotNormally: callback);
		_ = new GetItemSettings(true, false, true, StepAfterHandlingSlotNormally: callback, NoCoinMerge: true);
		_ = new Settings(false, false, false, StepAfterHandlingSlotNormally: AfterSlot);
		_ = new GetItemSettings(false, false, false, StepAfterHandlingSlotNormally: item => item.TurnToAir());
		_ = new GetItemSettings(false, false, false, StepAfterHandlingSlotNormally: null);
		_ = new GetItemSettings(false, false, false, StepAfterHandlingSlotNormally: default, NoCoinMerge: true);
		GetItemSettings implicitSettings = new(false, false, false, StepAfterHandlingSlotNormally: callback, NoCoinMerge: true);
		_ = new GetItemSettings(false, false, false, StepAfterHandlingSlotNormally: callback);
		_ = new GetItemSettings(false, false, false, StepAfterHandlingSlotNormally: callback, NoCoinMerge: true);
		_ = new GetItemSettings(false, false, false, true, callback, true);
		_ = new GetItemSettings(false, false, false, default);
		_ = new GetItemSettings(false, false, false, default, callback);
		_ = new GetItemSettings(LongText: true, NoText: false, CanGoIntoVoidVault: true, StepAfterHandlingSlotNormally: callback, NoCoinMerge: true);
	}

	static void AfterSlot(Item item) { }

	void RemovedPreset(GetItemSettings settings) {
#if COMPILE_ERROR
		GetItemSettings.InventoryUIToInventorySettings/* tModPorter Note: Removed. Use default(GetItemSettings) for the preset value. */ = settings;
		_ = nameof(GetItemSettings.InventoryUIToInventorySettings/* tModPorter Note: Removed. Use default(GetItemSettings) for the preset value. */);
#endif
	}

	void Unrelated() {
		_ = OtherSettings.InventoryUIToInventorySettings;
		_ = new OtherSettings(false, false, false, AfterSlot);
		int InventoryUIToInventorySettings = 1;
		_ = InventoryUIToInventorySettings;
	}

	class OtherSettings
	{
		public static int InventoryUIToInventorySettings;
		public OtherSettings(bool a, bool b, bool c, Action<Item> callback) { }
	}
}
