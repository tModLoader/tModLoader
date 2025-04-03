using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.ResourceSets;
using Terraria.ModLoader;

namespace ExampleMod.Common.UI.ExampleDisplaySets
{
	// This file showcases an example resource display set
	// In this example, it draws a clone of the Bars display set, but with the mana and stars "swapped" to the other location
	public class ExampleReversedBarsDisplay : ModResourceDisplaySet
	{
		// Variable names were copied from HorizontalBarsPlayerResourcesDisplaySet
		private int _maxSegmentCount;
		private int _hpSegmentsCount;
		private int _mpSegmentsCount;
		private int _hpFruitCount;
		private float _hpPercent;
		private float _mpPercent;
		private bool _hpHovered;
		private bool _mpHovered;
		private Asset<Texture2D> _hpFill;
		private Asset<Texture2D> _hpFillHoney;
		private Asset<Texture2D> _mpFill;
		private Asset<Texture2D> _panelLeft;
		private Asset<Texture2D> _panelMiddleHP;
		private Asset<Texture2D> _panelRightHP;
		private Asset<Texture2D> _panelMiddleMP;
		private Asset<Texture2D> _panelRightMP;

		private PlayerStatsSnapshot preparedSnapshot;

		public override void Load() {
			if (Main.dedServ)
				return;

			string vanillaFolder = "Images/UI/PlayerResourceSets/HorizontalBars/";
			string modFolder = "ExampleMod/Common/UI/ExampleDisplaySets/";
			_hpFill = Main.Assets.Request<Texture2D>(vanillaFolder + "HP_Fill");
			_hpFillHoney = Main.Assets.Request<Texture2D>(vanillaFolder + "HP_Fill_Honey");
			_mpFill = Main.Assets.Request<Texture2D>(vanillaFolder + "MP_Fill");

			_panelLeft = Main.Assets.Request<Texture2D>(vanillaFolder + "Panel_Left");
			_panelMiddleHP = Main.Assets.Request<Texture2D>(vanillaFolder + "HP_Panel_Middle");
			_panelRightHP = ModContent.Request<Texture2D>(modFolder + "HP_Panel_Right");
			_panelMiddleMP = Main.Assets.Request<Texture2D>(vanillaFolder + "MP_Panel_Middle");
			_panelRightMP = ModContent.Request<Texture2D>(modFolder + "MP_Panel_Right");
		}

		// DrawLife runs before DrawMana, so keep that in mind
		public override void DrawLife(SpriteBatch spriteBatch) {
			int num = 16;
			int num2 = 18;
			int num3 = Main.screenWidth - 300 - 22 + num;

			Vector2 vector = new Vector2(num3 - 10, num2 + 24);
			vector.X += (_maxSegmentCount - _hpSegmentsCount) * _panelMiddleHP.Width();

			bool isHovered = false;

			// Draw the life panels
			ResourceDrawSettings resourceDrawSettings = default;
			resourceDrawSettings.ElementCount = _hpSegmentsCount + 2;
			resourceDrawSettings.ElementIndexOffset = 0;
			resourceDrawSettings.TopLeftAnchor = vector;
			resourceDrawSettings.GetTextureMethod = LifePanelDrawer;
			resourceDrawSettings.OffsetPerDraw = Vector2.Zero;
			resourceDrawSettings.OffsetPerDrawByTexturePercentile = Vector2.UnitX;
			resourceDrawSettings.OffsetSpriteAnchor = Vector2.Zero;
			resourceDrawSettings.OffsetSpriteAnchorByTexturePercentile = Vector2.Zero;
			resourceDrawSettings.StatsSnapshot = preparedSnapshot;
			resourceDrawSettings.DisplaySet = this;
			resourceDrawSettings.ResourceIndexOffset = -1;  // Make the range [-1, 20] instead of [0, 21]
			resourceDrawSettings.Draw(spriteBatch, ref isHovered);

			// Draw the life bar filling
			resourceDrawSettings = default;
			resourceDrawSettings.ElementCount = _hpSegmentsCount;
			resourceDrawSettings.ElementIndexOffset = 0;
			resourceDrawSettings.TopLeftAnchor = vector + new Vector2(6f, 6f);
			resourceDrawSettings.GetTextureMethod = LifeFillingDrawer;
			resourceDrawSettings.OffsetPerDraw = new Vector2(_hpFill.Width(), 0f);
			resourceDrawSettings.OffsetPerDrawByTexturePercentile = Vector2.Zero;
			resourceDrawSettings.OffsetSpriteAnchor = Vector2.Zero;
			resourceDrawSettings.OffsetSpriteAnchorByTexturePercentile = Vector2.Zero;
			resourceDrawSettings.StatsSnapshot = preparedSnapshot;
			resourceDrawSettings.DisplaySet = this;
			resourceDrawSettings.Draw(spriteBatch, ref isHovered);

			_hpHovered = isHovered;
		}

		public override void DrawMana(SpriteBatch spriteBatch) {
			int num = 16;
			int num2 = 18;
			int num3 = Main.screenWidth - 300 - 22 + num;

			Vector2 vector2 = new Vector2(num3, num2);
			vector2.X += (_maxSegmentCount - _mpSegmentsCount) * _panelMiddleMP.Width();

			bool isHovered = false;

			// Draw the mana panels
			ResourceDrawSettings resourceDrawSettings = default;
			resourceDrawSettings.ElementCount = _mpSegmentsCount + 2;
			resourceDrawSettings.ElementIndexOffset = 0;
			resourceDrawSettings.TopLeftAnchor = vector2;
			resourceDrawSettings.GetTextureMethod = ManaPanelDrawer;
			resourceDrawSettings.OffsetPerDraw = Vector2.Zero;
			resourceDrawSettings.OffsetPerDrawByTexturePercentile = Vector2.UnitX;
			resourceDrawSettings.OffsetSpriteAnchor = Vector2.Zero;
			resourceDrawSettings.OffsetSpriteAnchorByTexturePercentile = Vector2.Zero;
			resourceDrawSettings.StatsSnapshot = preparedSnapshot;
			resourceDrawSettings.DisplaySet = this;
			resourceDrawSettings.ResourceIndexOffset = -1;  // Make the range [-1, 20] instead of [0, 21]
			resourceDrawSettings.Draw(spriteBatch, ref isHovered);

			// Draw the mana bar filling
			resourceDrawSettings = default;
			resourceDrawSettings.ElementCount = _mpSegmentsCount;
			resourceDrawSettings.ElementIndexOffset = 0;
			resourceDrawSettings.TopLeftAnchor = vector2 + new Vector2(6f, 6f);
			resourceDrawSettings.GetTextureMethod = ManaFillingDrawer;
			resourceDrawSettings.OffsetPerDraw = new Vector2(_mpFill.Width(), 0f);
			resourceDrawSettings.OffsetPerDrawByTexturePercentile = Vector2.Zero;
			resourceDrawSettings.OffsetSpriteAnchor = Vector2.Zero;
			resourceDrawSettings.OffsetSpriteAnchorByTexturePercentile = Vector2.Zero;
			resourceDrawSettings.StatsSnapshot = preparedSnapshot;
			resourceDrawSettings.DisplaySet = this;
			resourceDrawSettings.Draw(spriteBatch, ref isHovered);

			_mpHovered = isHovered;
		}

		public override bool PreHover(out bool hoveringLife) {
			hoveringLife = _hpHovered;
			return _hpHovered || _mpHovered;
		}

		public override void PreDrawResources(PlayerStatsSnapshot snapshot) {
			_hpSegmentsCount = snapshot.AmountOfLifeHearts;
			_mpSegmentsCount = snapshot.AmountOfManaStars;
			_maxSegmentCount = 20;
			_hpFruitCount = snapshot.LifeFruitCount;
			_hpPercent = (float)snapshot.Life / snapshot.LifeMax;
			_mpPercent = (float)snapshot.Mana / snapshot.ManaMax;
			preparedSnapshot = snapshot;
		}

		// The methods below were copied from HorizontalBarsPlayerResourcesDisplaySet and modified to account for the changed right panels
		private void LifePanelDrawer(int elementIndex, int firstElementIndex, int lastElementIndex, out Asset<Texture2D> sprite, out Vector2 offset, out float drawScale, out Rectangle? sourceRect) {
			sourceRect = null;
			offset = Vector2.Zero;
			sprite = _panelLeft;
			drawScale = 1f;
			if (elementIndex == lastElementIndex) {
				sprite = _panelRightHP;
				// Offset swapped with offset from ManaPanelDrawer
				//offset = new Vector2(-16f, -10f);
				offset = new Vector2(-16f, -6f);
			}
			else if (elementIndex != firstElementIndex) {
				sprite = _panelMiddleHP;
				// Make the panels draw from right to left
				int opposite = lastElementIndex - (elementIndex - firstElementIndex);
				int drawIndexOffset = opposite - elementIndex;
				offset.X = drawIndexOffset * _panelMiddleHP.Width();
			}
		}

		private void ManaPanelDrawer(int elementIndex, int firstElementIndex, int lastElementIndex, out Asset<Texture2D> sprite, out Vector2 offset, out float drawScale, out Rectangle? sourceRect) {
			sourceRect = null;
			offset = Vector2.Zero;
			sprite = _panelLeft;
			drawScale = 1f;
			if (elementIndex == lastElementIndex) {
				sprite = _panelRightMP;
				// Offset swapped with offset from LifePanelDrawer
				//offset = new Vector2(-16f, -6f);
				offset = new Vector2(-16f, -10f);
			}
			else if (elementIndex != firstElementIndex) {
				sprite = _panelMiddleMP;
				// Make the panels draw from right to left
				int opposite = lastElementIndex - (elementIndex - firstElementIndex);
				int drawIndexOffset = opposite - elementIndex;
				offset.X = drawIndexOffset * _panelMiddleMP.Width();
			}
		}

		private void LifeFillingDrawer(int elementIndex, int firstElementIndex, int lastElementIndex, out Asset<Texture2D> sprite, out Vector2 offset, out float drawScale, out Rectangle? sourceRect) {
			sprite = _hpFill;
			if (elementIndex < _hpFruitCount)
				sprite = _hpFillHoney;

			HorizontalBarsPlayerResourcesDisplaySet.FillBarByValues(elementIndex, sprite, _hpSegmentsCount, _hpPercent, out offset, out drawScale, out sourceRect);
			// Make the bar fillings draw from right to left
			int opposite = lastElementIndex - (elementIndex - firstElementIndex);
			int drawIndexOffset = opposite - elementIndex;
			offset.X += drawIndexOffset * sprite.Width();
		}

		private void ManaFillingDrawer(int elementIndex, int firstElementIndex, int lastElementIndex, out Asset<Texture2D> sprite, out Vector2 offset, out float drawScale, out Rectangle? sourceRect) {
			sprite = _mpFill;
			HorizontalBarsPlayerResourcesDisplaySet.FillBarByValues(elementIndex, sprite, _mpSegmentsCount, _mpPercent, out offset, out drawScale, out sourceRect);
			// Make the bar fillings draw from right to left
			int opposite = lastElementIndex - (elementIndex - firstElementIndex);
			int drawIndexOffset = opposite - elementIndex;
			offset.X += drawIndexOffset * sprite.Width();
		}
	}
}
