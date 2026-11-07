using System;
using Terraria;
using Settings = Terraria.GetItemSettings;
using static Terraria.GetItemSettings;

public class GetItemSettingsTest
{
	void Presets() {
		_ = GetItemSettings.InventoryEntityToPlayerInventorySettings;
		_ = GetItemSettings.NPCEntityToPlayerInventorySettings;
		_ = GetItemSettings.LootAllSettings;
		_ = GetItemSettings.LootAllSettingsRegularChest;
		_ = GetItemSettings.GetItemInDropItemCheck;
		_ = GetItemSettings.InventoryUIToInventorySettingsShowAsNew;
		_ = GetItemSettings.InventoryUIToInventorySettings;
		_ = Settings.InventoryEntityToPlayerInventorySettings;
		_ = global::Terraria.GetItemSettings.LootAllSettings;
		_ = InventoryUIToInventorySettingsShowAsNew;
		_ = InventoryUIToInventorySettings;
		_ = Settings.InventoryUIToInventorySettings;
		_ = GetItemSettings.ReturnItemFromSlot;
		_ = default(GetItemSettings);
		_ = nameof(GetItemSettings.InventoryUIToInventorySettingsShowAsNew);
	}

	void Constructors(Action<Item> callback) {
		_ = new GetItemSettings(true, false, true, callback);
		_ = new GetItemSettings(true, false, true, callback, true);
		_ = new Settings(false, false, false, AfterSlot);
		_ = new GetItemSettings(false, false, false, item => item.TurnToAir());
		_ = new GetItemSettings(false, false, false, null);
		_ = new GetItemSettings(false, false, false, default, true);
		GetItemSettings implicitSettings = new(false, false, false, callback, true);
		_ = new GetItemSettings(false, false, false, StepAfterHandlingSlotNormally: callback);
		_ = new GetItemSettings(false, false, false, StepAfterHandlingSlotNormally: callback, true);
		_ = new GetItemSettings(false, false, false, true, callback, true);
		_ = new GetItemSettings(false, false, false, default);
		_ = new GetItemSettings(false, false, false, default, callback);
		_ = new GetItemSettings(LongText: true, NoText: false, CanGoIntoVoidVault: true, callback, NoCoinMerge: true);
	}

	static void AfterSlot(Item item) { }

	void RemovedPreset(GetItemSettings settings) {
		GetItemSettings.InventoryUIToInventorySettings = settings;
		_ = nameof(GetItemSettings.InventoryUIToInventorySettings);
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
