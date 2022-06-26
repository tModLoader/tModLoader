using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;

namespace Terraria.GameContent.UI
{
	public partial class GameTipsDisplay
	{
		internal List<GameTipData> vanillaTips {
			get;
			private set;
		}
		internal List<GameTipData> allTips;

		internal void Initialize() {
			vanillaTips = _tipsDefault.Concat(_tipsKeyboard).Concat(_tipsGamepad).Select(localizedText => new GameTipData(localizedText)).ToList();
			allTips = vanillaTips.Select(tip => tip.Clone()).ToList();
		}

		internal void Reset() {
			ClearTips();
			allTips = vanillaTips.Select(tip => tip.Clone()).ToList();
			_lastTip = null;
		}
	}
}
