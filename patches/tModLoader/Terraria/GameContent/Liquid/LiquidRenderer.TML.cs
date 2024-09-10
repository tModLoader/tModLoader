using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Terraria.GameContent.Liquid;
public partial class LiquidRenderer
{
	private const int LiquidShimmerTexture = 14;
	private const int LiquidHoneyTexture = 11;
	private const int LiquidLavaTexture = 1;

	private Texture2D GetLiquidTexture(int type, int waterStyle)
	{
		switch (type) {
			case 0:
				return _liquidTextures[waterStyle].Value;
			case 1:
				return _liquidTextures[LiquidLavaTexture].Value;
			case 2:
				return _liquidTextures[LiquidShimmerTexture].Value;
			default:
				return _liquidTextures[LiquidShimmerTexture + 5 - type].Value;
		}
	}
}
