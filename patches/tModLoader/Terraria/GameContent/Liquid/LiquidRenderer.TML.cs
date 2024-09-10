using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria.GameContent.Liquid;
public partial class LiquidRenderer
{
	private const int LiquidShimmerTexture = 14;
	private const int LiquidHoneyTexture = 11;
	private const int LiquidLavaTexture = 1;

	internal Asset<Texture2D>[] _moddedLiquidTextures = new Asset<Texture2D>[0];
	// internal Asset<Texture2D>[][] _moddedLiquidTextures = new Asset<Texture2D>[0][]; // this is for liquid style, if we decide to support it in the future
	private Texture2D GetLiquidTexture(int type, int waterStyle)
	{
		switch (type) {
			case 0:
				return _liquidTextures[waterStyle].Value;
			case 1:
				return _liquidTextures[LiquidLavaTexture].Value;
			case 2:
				return _liquidTextures[LiquidHoneyTexture].Value;
			case 3:
				return _liquidTextures[LiquidShimmerTexture].Value;
			default:
				return _moddedLiquidTextures[type - LiquidID.Count].Value;
		}
	}
}
