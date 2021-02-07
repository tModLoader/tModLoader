using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.UI;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.Shops
{
	// todo: allow custom slot indexes
	public abstract class NPCShop : ModType
	{
		public abstract int NPCType { get; }

		public int Type { get; internal set; }

		public virtual bool EvaluateOnOpen => true;

		protected sealed override void Register() {
			ModTypeLookup<NPCShop>.Register(this);

			NPCShopManager.RegisterShop(this);
		}

		internal readonly Dictionary<string, Tab> tabs = new Dictionary<string, Tab>();
		public Tab DefaultTab { get; private set; }

		public sealed override void SetupContent() {
			DefaultTab = AddTab(null, "Default");
			SetDefaults();
		}

		public virtual void SetDefaults() {
		}

		public Tab AddTab(Mod mod, string key) {
			Tab tab = new Tab
			{
				Name = key,
				Mod = mod
			};
			tabs.Add(key, tab);

			NPCShopManager.entryCache[Type].Add(key, new CacheList());

			return tab;
		}

		public Tab GetTab(string key) => tabs.ContainsKey(key) ? tabs[key] : null;

		public EntryItem CreateEntry(int type) => DefaultTab.AddEntry(type);

		public EntryItem CreateEntry<T>() where T : ModItem => CreateEntry(ModContent.ItemType<T>());

		public int SellItem(Item newItem) {
			int num = ShopSellbackHelper.Remove(newItem);

			if (num >= newItem.stack)
				return 0;

			Item clone = newItem.Clone();
			clone.favorited = false;
			clone.buyOnce = true;

			var cache = NPCShopManager.entryCache[Type][currentTab.Name];
			int index = cache.Add(clone);

			// todo: increase rows when a row is filled (solves not being able to manual sell)
			
			int maxRowIndex = cache.Capacity / 10 - 4;
			npcShopRowIndex = Math.Min(maxRowIndex, index / 10);

			return index;
		}

		public T AddEntry<T>(T entry) where T : Entry {
			DefaultTab.AddEntry(entry);
			return entry;
		}

		public void Evaluate() {
			var cache = NPCShopManager.entryCache[Type];

			foreach (var pair in tabs)
			{
				cache[pair.Key].Clear();

				foreach (Entry entry in pair.Value.Entries)
				{
					cache[pair.Key].AddRange(entry.GetItems());
				}
			}
		}

		// note: different names (because technically the shop is buying the item)
		public virtual bool CanSellItem(Player player, Item item) {
			return true;
		}

		public virtual void PostSellItem(Player player, Item item) {
		}

		public virtual bool CanBuyItem(Player player, Item item) {
			return true;
		}

		public virtual void PostBuyItem(Player player, Item item) {
		}
		
		private static int npcShopRowIndex;
		internal static Tab currentTab;
		private static Dictionary<string, CacheList> temp;

		public void OnClose() {
			NPCShopManager.entryCache[Type] = temp;
		}

		public virtual Rectangle GetDimensions() {
			return new Rectangle(73, Main.instance.invBottom, (int)(560 * 0.755f), (int)(224 * 0.755f));
		}

		public virtual void OnScroll(int delta) {
			var inv = NPCShopManager.entryCache[Type][currentTab.Name];
			int rows = Math.Max(4, inv.Capacity / 10);
			int maxRowIndex = rows - 4;

			npcShopRowIndex = Utils.Clamp(npcShopRowIndex + delta, 0, maxRowIndex);

			if (!PlayerInput.IgnoreMouseInterface)
			{
				Main.LocalPlayer.mouseInterface = true;
			}
		}

		public virtual void OnOpen() {
			npcShopRowIndex = 0;
			currentTab = DefaultTab;

			if (EvaluateOnOpen) Evaluate();

			temp = new Dictionary<string, CacheList>(NPCShopManager.entryCache[Type]);
		}

		public virtual void Draw(SpriteBatch spriteBatch) {
			ref float inventoryScale = ref Main.inventoryScale;
			inventoryScale = 0.755f;

			float mouseX = Main.mouseX;
			float mouseY = Main.mouseY;
			Vector2 slotSize = TextureAssets.InventoryBack.Size() * inventoryScale;
			int invBottom = Main.instance.invBottom;
			var inv = NPCShopManager.entryCache[Type][currentTab.Name];
			int rows = Math.Max(4, inv.Capacity / 10);

			Rectangle shopRect = GetDimensions();

			Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.MouseText.Value, Language.GetText("LegacyInterface.28").Value, 504f, invBottom, Color.White * (Main.mouseTextColor / 255f), Color.Black, Vector2.Zero);
			ItemSlot.DrawSavings(spriteBatch, 504f, invBottom);

			// todo: add tab selection
			if (tabs.Count > 1)
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, FontAssets.MouseText.Value, "Tab: " + currentTab.DisplayName, new Vector2(73f, 426f), Color.White, 0f, Vector2.Zero, Vector2.One);

			// todo: better scrollbar visuals
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(shopRect.Right, invBottom, 4, shopRect.Height), new Color(79, 91, 39));
			spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(shopRect.Right, (int)(invBottom + npcShopRowIndex * (shopRect.Height / (float)rows)), 4, (int)(shopRect.Height * (4f / rows))), new Color(110, 128, 54));

			for (int column = 0; column < 10; column++)
			{
				for (int row = npcShopRowIndex; row < npcShopRowIndex + 4; row++)
				{
					int x = (int)(73f + column * 56 * inventoryScale);
					int y = (int)(invBottom + (row - npcShopRowIndex) * 56 * inventoryScale);
					int slotIndex = column + row * 10;

					Item item = inv[slotIndex];

					if (mouseX >= x && mouseX <= x + slotSize.X && mouseY >= y && mouseY <= y + slotSize.Y && !PlayerInput.IgnoreMouseInterface)
					{
						ItemSlot.OverrideHover(ref item, 15);
						Main.LocalPlayer.mouseInterface = true;
						ItemSlot.LeftClick(ref item, 15);
						ItemSlot.RightClick(ref item, 15);
						ItemSlot.MouseHover(ref item, 15);
					}

					ItemSlot.Draw(spriteBatch, ref item, 15, new Vector2(x, y));

					if (item.IsAir && !inv[slotIndex].IsAir) inv.Remove(slotIndex);
					else inv[slotIndex] = item;
				}
			}

			inv.RemoveEmptyRows();
			int maxRowIndex = inv.Capacity / 10 - 4;
			npcShopRowIndex = Math.Min(npcShopRowIndex, maxRowIndex);
		}
	}
}