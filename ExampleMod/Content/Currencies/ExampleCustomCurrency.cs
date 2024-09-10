using Microsoft.Xna.Framework;
using Terraria.GameContent.UI;

namespace ExampleMod.Content.Currencies
{
	public class ExampleCustomCurrency : CustomCurrencySingleCoin
	{
		public ExampleCustomCurrency(int coinItemID, long currencyCap, string CurrencyTextKey) : base(coinItemID, currencyCap) {
			this.CurrencyTextKey = CurrencyTextKey;
			CurrencyTextColor = Color.BlueViolet;
		}
	}
}