using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ExampleMod.Common.UI.ExampleCoinsUI
{
	// ExampleUIs visibility is toggled by typing "/coins" in chat (See CoinCommand.cs)
	// ExampleCoinsUI is a simple UI example showing how to use UIPanel, UIImageButton, and even a custom UIElement
	// For more info about UI you can check https://github.com/tModLoader/tModLoader/wiki/Basic-UI-Element and https://github.com/tModLoader/tModLoader/wiki/Advanced-guide-to-custom-UI
	internal class ExampleCoinsUIState : UIState
	{
		public ExampleDraggableUIPanel CoinCounterPanel;
		public UIMoneyDisplay MoneyDisplay;

		// In OnInitialize, we place various UIElements onto our UIState (this class).
		// UIState classes have width and height equal to the full screen, because of this, usually we first define a UIElement that will act as the container for our UI.
		// We then place various other UIElement onto that container UIElement positioned relative to the container UIElement.
		public override void OnInitialize() {
			// Here we define our container UIElement. In DraggableUIPanel.cs, you can see that DraggableUIPanel is a UIPanel with a couple added features.
			CoinCounterPanel = new ExampleDraggableUIPanel();
			CoinCounterPanel.SetPadding(0);
			// We need to place this UIElement in relation to its Parent. Later we will be calling `base.Append(coinCounterPanel);`.
			// This means that this class, ExampleCoinsUI, will be our Parent. Since ExampleCoinsUI is a UIState, the Left and Top are relative to the top left of the screen.
			// SetRectangle method help us to set the position and size of UIElement
			SetRectangle(CoinCounterPanel, left: 400f, top: 100f, width: 170f, height: 70f);
			CoinCounterPanel.BackgroundColor = new Color(73, 94, 171);

			// Next, we create another UIElement that we will place. Since we will be calling `coinCounterPanel.Append(playButton);`, Left and Top are relative to the top left of the coinCounterPanel UIElement.
			// By properly nesting UIElements, we can position things relatively to each other easily.
			Asset<Texture2D> buttonPlayTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonPlay");
			ExampleUIHoverImageButton playButton = new ExampleUIHoverImageButton(buttonPlayTexture, Language.GetTextValue("Mods.ExampleMod.UI.ExampleCoinsUIState.Reset"));
			SetRectangle(playButton, left: 110f, top: 10f, width: 22f, height: 22f);
			// UIHoverImageButton doesn't do anything when Clicked. Here we assign a method that we'd like to be called when the button is clicked.
			playButton.OnLeftClick += new MouseEvent(PlayButtonClicked);
			CoinCounterPanel.Append(playButton);

			Asset<Texture2D> buttonDeleteTexture = ModContent.Request<Texture2D>("Terraria/Images/UI/ButtonDelete");
			ExampleUIHoverImageButton closeButton = new ExampleUIHoverImageButton(buttonDeleteTexture, Language.GetTextValue("LegacyInterface.52")); // Localized text for "Close"
			SetRectangle(closeButton, left: 140f, top: 10f, width: 22f, height: 22f);
			closeButton.OnLeftClick += new MouseEvent(CloseButtonClicked);
			CoinCounterPanel.Append(closeButton);

			// UIMoneyDisplay is a fairly complicated custom UIElement. UIMoneyDisplay handles drawing some text and coin textures.
			// Organization is key to managing UI design. Making a contained UIElement like UIMoneyDisplay will make many things easier.
			MoneyDisplay = new UIMoneyDisplay();
			SetRectangle(MoneyDisplay, 15f, 20f, 100f, 40f);
			CoinCounterPanel.Append(MoneyDisplay);

			Append(CoinCounterPanel);
			// As a recap, ExampleCoinsUI is a UIState, meaning it covers the whole screen. We attach CoinCounterPanel to ExampleCoinsUI some distance from the top left corner.
			// We then place playButton, closeButton, and MoneyDisplay onto CoinCounterPanel so we can easily place these UIElements relative to CoinCounterPanel.
			// Since CoinCounterPanel will move, this proper organization will move playButton, closeButton, and MoneyDisplay properly when CoinCounterPanel moves.
		}

		private void SetRectangle(UIElement uiElement, float left, float top, float width, float height) {
			uiElement.Left.Set(left, 0f);
			uiElement.Top.Set(top, 0f);
			uiElement.Width.Set(width, 0f);
			uiElement.Height.Set(height, 0f);
		}

		private void PlayButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuOpen);
			MoneyDisplay.ResetCoins();
		}

		private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
			SoundEngine.PlaySound(SoundID.MenuClose);
			ModContent.GetInstance<ExampleCoinsUISystem>().HideMyUI();
		}

		public void UpdateValue(int pickedUp) {
			MoneyDisplay.AddCoinsPerMinute(pickedUp);
		}
	}

	public class UIMoneyDisplay : UIElement
	{
		// How many coins have been collected in copper
		public long collectedCoins;
		// Time from start(or reset) to calculate how many coins collected per minute
		private DateTime? startTime;
		// Saving coin textures to an array to make them easier to access
		private readonly Texture2D[] coinsTextures = new Texture2D[4];

		public UIMoneyDisplay() {
			startTime = null;

			for (int j = 0; j < 4; j++) {
				// Textures may not be loaded without it
				Main.instance.LoadItem(74 - j);
				coinsTextures[j] = TextureAssets.Item[74 - j].Value;
			}

			// This allows clicks to "pass-through" this element to the parent element and not be consumed by this element. This allows ExampleDraggableUIPanel to be dragged even when the user is clicking on the UIMoneyDisplay.
			IgnoresMouseInteraction = true;
		}
		public void AddCoinsPerMinute(int coins) {
			collectedCoins += coins;

			// We begin to remember the time only after at least one coin has been collected
			if (startTime == null)
				startTime = DateTime.Now;
		}

		public int GetCoinsPerMinute() {
			if (collectedCoins == 0)
				return 0;

			// If the time has passed less than minutes, the current number of coins will be displayed
			return (int)(collectedCoins / Math.Max(1, (DateTime.Now - startTime.Value).TotalMinutes));
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			CalculatedStyle innerDimensions = GetInnerDimensions();
			// Getting top left position of this UIElement
			float shopx = innerDimensions.X;
			float shopy = innerDimensions.Y;

			// Drawing first line of coins (current collected coins)
			// CoinsSplit converts the number of copper coins into an array of all types of coins
			DrawCoins(spriteBatch, shopx, shopy, Utils.CoinsSplit(collectedCoins));

			// Drawing second line of coins (coins per minute) and text "CPM"
			DrawCoins(spriteBatch, shopx, shopy, Utils.CoinsSplit(GetCoinsPerMinute()), 0, 25);
			Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, "CPM", shopx + (float)(24 * 4), shopy + 25f, Color.White, Color.Black, new Vector2(0.3f), 0.75f);
		}

		private void DrawCoins(SpriteBatch spriteBatch, float shopx, float shopy, int[] coinsArray, int xOffset = 0, int yOffset = 0) {
			for (int j = 0; j < 4; j++) {
				spriteBatch.Draw(coinsTextures[j], new Vector2(shopx + 11f + 24 * j + xOffset, shopy + yOffset), null, Color.White, 0f, coinsTextures[j].Size() / 2f, 1f, SpriteEffects.None, 0f);
				Utils.DrawBorderStringFourWay(spriteBatch, FontAssets.ItemStack.Value, coinsArray[3 - j].ToString(), shopx + 24 * j + xOffset, shopy + yOffset, Color.White, Color.Black, new Vector2(0.3f), 0.75f);
			}
		}

		public void ResetCoins() {
			collectedCoins = 0;
			startTime = DateTime.Now;
		}
	}

	public class MoneyCounterGlobalItem : GlobalItem
	{
		public override bool AppliesToEntity(Item item, bool lateInstantiation) {
			return item.type >= ItemID.CopperCoin && item.type <= ItemID.PlatinumCoin;
		}

		public override bool OnPickup(Item item, Player player) {
			// If we have picked up coins of any type, then we will update the values in exampleCoinsUI
			ModContent.GetInstance<ExampleCoinsUISystem>().exampleCoinsUI.UpdateValue(item.stack * (item.value / 5));
			return base.OnPickup(item, player);
		}
	}
}
