using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.DisplayStyles
{
	// Example of a ModResourcesDisplayStyle utilizing ResourceDrawSettings
	// In this example, we mimic the Fancy-Classic style but swap mana and heart textures with item textures, while also cleaning up the code
	public class ExampleDrawSettingsDisplayStyle : ModResourcesDisplayStyle
	{
		// Change the display name shown in the settings menu
		// This is entirely irrelevant to the internal name of a style, which is "ModName:Name"
		public override string DisplayName => "DrawSettings Example";

		public int playerLifeFruit;
		public int lastHeartIndex;
		public int heartRow1Count;
		public int heartRow2Count;
		public int starColumnCount;
		public int lastStarIndex;
		public float manaPerStar;
		public float lifePerHeart;
		public float playerMana;
		public float playerLife;
		public bool hoveringOverLife;
		public bool hoveringOverMana;
		
		public Asset<Texture2D> heart = TextureAssets.Item[ItemID.ClayBlock];
		public Asset<Texture2D> lifeFruitHeart = TextureAssets.Item[ItemID.Hive];
		public Asset<Texture2D> star = TextureAssets.Item[ItemID.AshBlock];

		public override void Draw() {
			InitializeFields();
			DrawLifeBar();
			DrawManaBar();
		}

		private void DrawLifeBar() {
			Vector2 drawAnchor = new Vector2(Main.screenWidth - 300 + 4, 15f);
			Vector2 drawOffset = new Vector2(15f, 15f);
			Vector2 half = new Vector2(0.5f, 0.5f);
			Vector2 zero = Vector2.Zero;
			Vector2 oneX = Vector2.UnitX;

			// Always reset the hovering bool to false so it stays false if the user is no longer hovering
			hoveringOverLife = false;

			// These two methods draw the front heart textures (separate rows)
			// View the method itself for an explanation of parameters
			DrawResource(heartRow1Count, 0, drawAnchor + drawOffset, HeartFillingDrawer, oneX * 2f, oneX, zero, half, ref hoveringOverLife);
			DrawResource(heartRow2Count, 10, drawAnchor + drawOffset + new Vector2(0f, 28f), HeartFillingDrawer, oneX * 2f, oneX, zero, half, ref hoveringOverLife);

			void HeartFillingDrawer(int elementIndex, int firstElementIndex, int lastElementIndex, out Asset<Texture2D> sprite, out Vector2 offset, out float drawScale, out Rectangle? sourceRect) {
				sourceRect = null;
				offset = Vector2.Zero;
				drawScale = Utils.GetLerpValue(lifePerHeart * elementIndex, lifePerHeart * (elementIndex + 1), playerLife, true);

				// Change the sprite to its life fruit version of the element index is less than the life fruit amount,
				// which means that the heart attempting to be drawn is supposed to be a life fruit
				sprite = elementIndex < playerLifeFruit
					? lifeFruitHeart 
					: heart;
			}
		}

		private void DrawManaBar() {
			Vector2 drawAnchor = new Vector2(Main.screenWidth - 40, 22f);
			Vector2 drawOffset = new Vector2(15f, 16f);
			Vector2 half = new Vector2(0.5f, 0.5f);
			Vector2 zero = Vector2.Zero;
			Vector2 oneY = Vector2.UnitY;

			// Always reset the hovering bool to false so it stays false if the user is no longer hovering
			hoveringOverMana = false;

			// This method draws the star texture
			// View the method itself for an explanation of parameters
			DrawResource(starColumnCount, 0, drawAnchor + drawOffset, StarFillingDrawer, oneY * -2f, oneY, zero, half, ref hoveringOverMana);

			void StarFillingDrawer(int elementIndex, int firstElementIndex, int lastElementIndex, out Asset<Texture2D> sprite, out Vector2 offset, out float drawScale, out Rectangle? sourceRect) {
				sourceRect = null;
				offset = Vector2.Zero;
				sprite = star;
				drawScale = Utils.GetLerpValue(manaPerStar * elementIndex, manaPerStar * (elementIndex + 1), playerMana, true);
			}
		}

		// This is a simple helper method that allows us to call a method with parameters instead of constantly writing out a new ResourceDrawSettings instance
		// It also calls the Draw method which properly draws our resource and also sets the value of our hovering
		// elementCount is the amount to draw
		// elementIndexOffset is the offset for the first and last elements
		// drawAnchor is the top-left anchoring position for drawing ech element
		// drawMethod is the ResourcesDrawSettings.TextureGetter delegate, and initializes some extra fields related to textures in the ResourceDrawSettings instance
		// offsetPerDraw is the amount of offset for each element
		// offsetPerDrawPercentile is a percentage version of offsetPerDraw, both are applied
		// offsetAnchor is the offset for the texture in reference to the provided drawAnchor
		// offsetAnchorPercentile is the percentage version of offsetAnchor, both are applied
		private static void DrawResource(int elementCount, int elementIndexOffset, Vector2 drawAnchor, ResourceDrawSettings.TextureGetter drawMethod, Vector2 offsetPerDraw, Vector2 offsetPerDrawPercentile, Vector2 offsetAnchor, Vector2 offsetAnchorPercentile, ref bool hovering) {
			new ResourceDrawSettings {
				ElementCount = elementCount,
				ElementIndexOffset = elementIndexOffset,
				TopLeftAnchor = drawAnchor,
				GetTextureMethod = drawMethod,
				OffsetPerDraw = offsetPerDraw,
				OffsetPerDrawByTexturePercentile = offsetPerDrawPercentile,
				OffsetSpriteAnchor = offsetAnchor,
				OffsetSpriteAnchorByTexturePercentile = offsetAnchorPercentile,
			}.Draw(Main.spriteBatch, ref hovering);
		}

		private void InitializeFields() {
			// PlayerStatsSnapshot is a small struct containing basic player data
			// We can use Main.LocalPlayer since this is entirely client-side UI drawing
			// and we don't need to worry about MP or a de-sync in player instances
			PlayerStatsSnapshot playerStatsSnapshot = new PlayerStatsSnapshot(Main.LocalPlayer);
			playerLifeFruit = playerStatsSnapshot.LifeFruitCount;
			playerLife = playerStatsSnapshot.Life;
			playerMana = playerStatsSnapshot.Mana;

			manaPerStar = playerStatsSnapshot.ManaPerSegment;
			lifePerHeart = playerStatsSnapshot.LifePerSegment;

			// Simple Utils.Clamp usage to clamp a count between a specific value
			// We clamp both rows between 0 and 10 hearts individually, and calculate their values
			// Row one's value is calculated by dividing a player's maximum life by the life per heart,
			// which, despite being capable of only going above 10, gets clamped at 10
			// Row two's value is calculated by dividing a player's maximum life minus 200 (so it will only go above
			// zero if the player has the first ten hearts in the first row) by the life per heart
			// If the second row overflows above 10, then the extra hearts won't be drawn either
			// Life fruits are manually drawn by getting their count from the snapshot
			heartRow1Count = Utils.Clamp((int)(playerStatsSnapshot.LifeMax / lifePerHeart), 0, 10);
			heartRow2Count = Utils.Clamp((int)((playerStatsSnapshot.LifeMax - 200) / lifePerHeart), 0, 10);

			// Star column count is simple division without a cap
			starColumnCount = (int)(playerStatsSnapshot.ManaMax / manaPerStar);

			// We find the last indexes for the pulsing effect at the end of a row of hearts or column of stars
			lastHeartIndex = heartRow1Count + heartRow2Count - 1;
			lastStarIndex = (int)(playerMana / manaPerStar);
		}

		// A proper example of TryToHover
		// hoveringOverLife and hoveringOverMana are set in the Life and Mana
		// drawing methods, using draw methods with a ref value to change their values
		// CommonResourceBarMethods only contains the two methods shown here, which
		// simply change the displayed mouseText to the player's life out of max life or 
		// mana out of max mana, depending on what we're hovering over.
		public override void TryToHover() {
			if (hoveringOverLife)
				CommonResourceBarMethods.DrawLifeMouseOver();

			if (hoveringOverMana)
				CommonResourceBarMethods.DrawManaMouseOver();
		}
	}
}
