using Microsoft.Xna.Framework;
using Terraria.GameContent.UI;

namespace ExampleMod.Content.Currencies
{
	// An example of a custom currency, similar to the Defenders Medal.
	// Note that the code in ExampleMod.cs is required to register this custom currency. In that code you can see that ExampleItem is assigned as the item used for this currency, there is no "ExampleCustomCurrency" item.
	public class ExampleCustomCurrency : CustomCurrencySingleCoin
	{
		public ExampleCustomCurrency(int coinItemID, long currencyCap, string CurrencyTextKey) : base(coinItemID, currencyCap) {
			this.CurrencyTextKey = CurrencyTextKey;
			CurrencyTextColor = Color.BlueViolet;
		}
	}
}