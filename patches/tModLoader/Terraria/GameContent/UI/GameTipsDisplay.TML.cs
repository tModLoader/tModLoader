using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Terraria.GameContent.UI;

public partial class GameTipsDisplay
{
	internal List<GameTipData> allTips;

	internal void Initialize()
	{
		allTips = _tipsDefault.Concat(_tipsKeyboard).Concat(_tipsGamepad).Select(localizedText => new GameTipData(localizedText)).ToList();
	}

	internal void Reset()
	{
		ClearTips();
		allTips = allTips.Where(tip => tip.Mod is null).ToList();
		allTips.ForEach(tip => tip.isVisible = true);
		_lastTip = null;
	}
}
