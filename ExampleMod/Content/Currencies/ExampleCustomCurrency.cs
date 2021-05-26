using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.UI;
using Terraria.Localization;

namespace ExampleMod
{
	public class ExampleCustomCurrency : CustomCurrencySingleCoin
	{
		public static readonly Color ExampleCustomCurrencyTextColor = Color.BlueViolet;

		public ExampleCustomCurrency(int coinItemID, long currencyCap) : base(coinItemID, currencyCap)
		{
		}

		public override void GetPriceText(string[] lines, ref int currentLine, int price)
		{
			Color color = ExampleCustomCurrencyTextColor * (Main.mouseTextColor / 255f); // The text color is blue-violet, mentioned with the variable above.
			lines[currentLine++] = $"[c/{color.Hex3()}:{Language.GetTextValue("LegacyTooltip.50")} {price} faces]";
			// Note that color.Hex3() changes a color like {R: 255, G: 0, B: 0} to FF0000.
		}
	}
}
