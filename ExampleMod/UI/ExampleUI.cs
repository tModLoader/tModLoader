using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.UI
{
	// ExampleUIs visibility is toggled by typing "/coin" in chat. (See CoinCommand.cs)
	// ExampleUI is a simple UI example showing how to use UIPanel, UIImageButton, and even a custom UIElement.
	internal class ExampleUI : UIState
	{
		public DragableUIPanel CoinCounterPanel;
		public UIMoneyDisplay MoneyDisplay;
		public UIHoverImageButton ExampleButton;
		public static bool Visible;

		// In OnInitialize, we place various UIElements onto our UIState (this class).
		// UIState classes have width and height equal to the full screen, because of this, usually we first define a UIElement that will act as the container for our UI.
		// We then place various other UIElement onto that container UIElement positioned relative to the container UIElement.
		public override void OnInitialize() {
			// Here we define our container UIElement. In DragableUIPanel.cs, you can see that DragableUIPanel is a UIPanel with a couple added features.
			CoinCounterPanel = new DragableUIPanel();
			CoinCounterPanel.SetPadding(0);
			// We need to place this UIElement in relation to its Parent. Later we will be calling `base.Append(coinCounterPanel);`. 
			// This means that this class, ExampleUI, will be our Parent. Since ExampleUI is a UIState, the Left and Top are relative to the top left of the screen.
			CoinCounterPanel.Left.Set(400f, 0f);
			CoinCounterPanel.Top.Set(100f, 0f);
			CoinCounterPanel.Width.Set(170f, 0f);
			CoinCounterPanel.Height.Set(70f, 0f);
			CoinCounterPanel.BackgroundColor = new Color(73, 94, 171);

			// Next, we create another UIElement that we will place. Since we will be calling `coinCounterPanel.Append(playButton);`, Left and Top are relative to the top left of the coinCounterPanel UIElement. 
			// By properly nesting UIElements, we can position things relatively to each other easily.
			Texture2D buttonPlayTexture = ModContent.GetTexture("Terraria/UI/ButtonPlay");
			UIHoverImageButton playButton = new UIHoverImageButton(buttonPlayTexture, "Reset Coins Per Minute Counter");
			playButton.Left.Set(110, 0f);
			playButton.Top.Set(10, 0f);
			playButton.Width.Set(22, 0f);
			playButton.Height.Set(22, 0f);
			// UIHoverImageButton doesn't do anything when Clicked. Here we assign a method that we'd like to be called when the button is clicked.
			playButton.OnClick += new MouseEvent(PlayButtonClicked);
			CoinCounterPanel.Append(playButton);

			Texture2D buttonDeleteTexture = ModContent.GetTexture("Terraria/UI/ButtonDelete");
			UIHoverImageButton closeButton = new UIHoverImageButton(buttonDeleteTexture, Language.GetTextValue("LegacyInterface.52")); // Localized text for "Close"
			closeButton.Left.Set(140, 0f);
			closeButton.Top.Set(10, 0f);
			closeButton.Width.Set(22, 0f);
			closeButton.Height.Set(22, 0f);
			closeButton.OnClick += new MouseEvent(CloseButtonClicked);
			CoinCounterPanel.Append(closeButton);

			Texture2D buttonFavoriteTexture = ModContent.GetTexture("Terraria/UI/ButtonFavoriteActive");
			ExampleButton = new UIHoverImageButton(buttonFavoriteTexture, "SendClientChanges Example: Non-Stop Party (???)"); // See ExamplePlayer.OnEnterWorld
			ExampleButton.Left.Set(140, 0f);
			ExampleButton.Top.Set(36, 0f);
			ExampleButton.Width.Set(22, 0f);
			ExampleButton.Height.Set(22, 0f);
			ExampleButton.OnClick += new MouseEvent(ExampleButtonClicked);
			CoinCounterPanel.Append(ExampleButton);

			// UIMoneyDisplay is a fairly complicated custom UIElement. UIMoneyDisplay handles drawing some text and coin textures.
			// Organization is key to managing UI design. Making a contained UIElement like UIMoneyDisplay will make many things easier.
			MoneyDisplay = new UIMoneyDisplay();
			MoneyDisplay.Left.Set(15, 0f);
			MoneyDisplay.Top.Set(20, 0f);
			MoneyDisplay.Width.Set(100f, 0f);
			MoneyDisplay.Height.Set(0, 1f);
			CoinCounterPanel.Append(MoneyDisplay);

			Append(CoinCounterPanel);

			// As a recap, ExampleUI is a UIState, meaning it covers the whole screen. We attach coinCounterPanel to ExampleUI some distance from the top left corner.
			// We then place playButton, closeButton, and moneyDiplay onto coinCounterPanel so we can easily place these UIElements relative to coinCounterPanel.
			// Since coinCounterPanel will move, this proper organization will move playButton, closeButton, and moneyDiplay properly when coinCounterPanel moves.
		}

		private void PlayButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuOpen);
			MoneyDisplay.ResetCoins();
		}

		private void CloseButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
			Main.PlaySound(SoundID.MenuClose);
			Visible = false;
		}

		private void ExampleButtonClicked(UIMouseEvent evt, UIElement listeningElement) {
			var examplePlayer = Main.LocalPlayer.GetModPlayer<ExamplePlayer>();
			examplePlayer.nonStopParty = !examplePlayer.nonStopParty;
			ExampleButton.HoverText = "SendClientChanges Example: Non-Stop Party " + (examplePlayer.nonStopParty ? "On" : "Off");
		}

		public void UpdateValue(int pickedUp) {
			MoneyDisplay.Coins += pickedUp;
			MoneyDisplay.AddCoinsPerMinute(pickedUp);
		}
	}

	public class UIMoneyDisplay : UIElement
	{
		public long Coins;

		public UIMoneyDisplay() {
			Width.Set(100, 0f);
			Height.Set(40, 0f);

			for (int i = 0; i < 60; i++) {
				_coinBins[i] = -1;
			}
		}

		//DateTime dpsEnd;
		//DateTime dpsStart;
		//int dpsDamage;
		//public bool dpsStarted;
		//public DateTime dpsLastHit;

		// Array of ints 60 long.
		// "length" = seconds since reset
		// reset on button or 20 seconds of inactivity?
		// pointer to index so on new you can clear previous
		private readonly int[] _coinBins = new int[60];
		private int _coinBinsIndex;

		public void AddCoinsPerMinute(int coins) {
			int second = DateTime.Now.Second;
			if (second != _coinBinsIndex) {
				_coinBinsIndex = second;
				_coinBins[_coinBinsIndex] = 0;
			}
			_coinBins[_coinBinsIndex] += coins;
		}

		public int GetCoinsPerMinute() {
			int second = DateTime.Now.Second;
			if (second != _coinBinsIndex) {
				_coinBinsIndex = second;
				_coinBins[_coinBinsIndex] = 0;
			}

			long sum = _coinBins.Sum(a => a > -1 ? a : 0);
			int count = _coinBins.Count(a => a > -1);
			if (count == 0) {
				return 0;
			}
			return (int)(sum * 60f / count);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch) {
			CalculatedStyle innerDimensions = GetInnerDimensions();	
			//Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);

			float shopx = innerDimensions.X;
			float shopy = innerDimensions.Y;

			int[] coinsArray = Utils.CoinsSplit(Coins);
			for (int j = 0; j < 4; j++) {
				int num = j == 0 && coinsArray[3 - j] > 99 ? -6 : 0;
				spriteBatch.Draw(Main.itemTexture[74 - j], new Vector2(shopx + 11f + (float)(24 * j), shopy /*+ 75f*/), null, Color.White, 0f, Main.itemTexture[74 - j].Size() / 2f, 1f, SpriteEffects.None, 0f);
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontItemStack, coinsArray[3 - j].ToString(), shopx + (float)(24 * j) + (float)num, shopy/* + 75f*/, Color.White, Color.Black, new Vector2(0.3f), 0.75f);
			}

			coinsArray = Utils.CoinsSplit(GetCoinsPerMinute());
			for (int j = 0; j < 4; j++) {
				int num = j == 0 && coinsArray[3 - j] > 99 ? -6 : 0;
				spriteBatch.Draw(Main.itemTexture[74 - j], new Vector2(shopx + 11f + (float)(24 * j), shopy + 25f), null, Color.White, 0f, Main.itemTexture[74 - j].Size() / 2f, 1f, SpriteEffects.None, 0f);
				Utils.DrawBorderStringFourWay(spriteBatch, Main.fontItemStack, coinsArray[3 - j].ToString(), shopx + (float)(24 * j) + (float)num, shopy + 25f, Color.White, Color.Black, new Vector2(0.3f), 0.75f);
			}
			Utils.DrawBorderStringFourWay(spriteBatch, ExampleMod.exampleFont ?? Main.fontItemStack, "CPM", shopx + (float)(24 * 4), shopy + 25f, Color.White, Color.Black, new Vector2(0.3f), 0.75f);
		}

		internal void ResetCoins() {
			Coins = 0;
			for (int i = 0; i < 60; i++) {
				_coinBins[i] = -1;
			}
		}
	}

	public class MoneyCounterGlobalItem : GlobalItem
	{
		public override bool OnPickup(Item item, Player player) {
			if (item.type >= ItemID.CopperCoin && item.type <= ItemID.PlatinumCoin)
				GetInstance<ExampleMod>().ExampleUI.UpdateValue(item.stack * (item.value / 5));

			return base.OnPickup(item, player);
		}
	}
}
