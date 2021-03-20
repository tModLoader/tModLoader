using System.Collections.Generic;
using System.Linq;
using Terraria.UI;
using Terraria.GameContent.Bestiary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace Terraria.ModLoader.UI
{
	class UIBestiaryBar : UIElement
	{
		private class BestiaryBarItem
		{
			internal readonly string Tooltop;
			internal readonly int EntryCount;
			internal readonly int CompletedCount;
			internal readonly Color DrawColor;

			public BestiaryBarItem(string tooltop, int entryCount, int completedCount, Color drawColor) {
				Tooltop = tooltop;
				EntryCount = entryCount;
				CompletedCount = completedCount;
				DrawColor = drawColor;
			}
		}

		private BestiaryDatabase _db;
		private List<BestiaryBarItem> _bestiaryBarItems;

		public UIBestiaryBar(BestiaryDatabase db) {
			_db = db;
			_bestiaryBarItems = new List<BestiaryBarItem>();

			RecalculateBars();
		}

		private readonly Color[] _colors = {
			new Color(232, 76, 61),//red
			new Color(155, 88, 181),//purple
			new Color(27, 188, 155),//aqua
			new Color(243, 156, 17),//orange
			new Color(45, 204, 112),//green
			new Color(241, 196, 15),//yellow
		};

		public void RecalculateBars() {
			if (ModLoader.Mods.Length == 0) //Ensures that the mods have been loaded prior to accessing the db mod fields
				return;
			_bestiaryBarItems.Clear();

			List<BestiaryEntry> items = _db.GetBestiaryEntriesByMod(null);
			int collected = items.Count(oe => oe.UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0);
			_bestiaryBarItems.Add(new BestiaryBarItem($"Terraria: {(float)collected / items.Count * 100f:N2}% Collected", items.Count, collected, _colors[0]));
			for (int i = 1; i < ModLoader.Mods.Length; i++) {
				items = _db.GetBestiaryEntriesByMod(ModLoader.Mods[i]);
				collected = items.Count(oe => oe.UIInfoProvider.GetEntryUICollectionInfo().UnlockState > BestiaryEntryUnlockState.NotKnownAtAll_0);
				_bestiaryBarItems.Add(new BestiaryBarItem($"{ModLoader.Mods[i].DisplayName}: {(float)collected / items.Count * 100f:N2}% Collected", items.Count, collected, _colors[i++ % _colors.Length]));
			}
		}

		protected override void DrawSelf(SpriteBatch sb) {
			int xOffset = 0;
			var rectangle = GetDimensions().ToRectangle();

			bool drawHover = false;
			BestiaryBarItem hoverData = null;

			for (int i = 0; i < _bestiaryBarItems.Count; i++) {
				var barData = _bestiaryBarItems[i];

				int offset = (int)(rectangle.Width * (barData.EntryCount / (float)_db.Entries.Count));
				if (i == _bestiaryBarItems.Count - 1) {
					offset = rectangle.Width - xOffset;
				}
				int width = (int)(offset * (barData.CompletedCount / (float)barData.EntryCount));

				var drawArea = new Rectangle(rectangle.X + xOffset, rectangle.Y, width, rectangle.Height);
				var outlineArea = new Rectangle(rectangle.X + xOffset, rectangle.Y, offset, rectangle.Height);
				xOffset += offset;
				sb.Draw(TextureAssets.MagicPixel.Value, outlineArea, barData.DrawColor * 0.3f);
				sb.Draw(TextureAssets.MagicPixel.Value, drawArea, barData.DrawColor);

				if(!drawHover && outlineArea.Contains(new Point(Main.mouseX, Main.mouseY))) {
					drawHover = true;
					hoverData = barData;
				}
			}

			if(drawHover && hoverData != null) {
				Main.instance.MouseText(hoverData.Tooltop, 0, 0);
			}
		}
	}
}
