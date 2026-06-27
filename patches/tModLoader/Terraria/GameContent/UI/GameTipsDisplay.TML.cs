using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Terraria.GameContent.UI;

public partial class GameTipsDisplay
{
	internal void Reset()
	{
		ClearTips();
		if (_tipProvider is GameTipsProvider p)
			p.Reset();
	}
}
