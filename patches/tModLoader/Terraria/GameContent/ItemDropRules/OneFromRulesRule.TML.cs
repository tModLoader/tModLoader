using System.Collections.Generic;

namespace Terraria.GameContent.ItemDropRules
{
	partial class OneFromRulesRule
	{
		public int OutOfY => _outOfY;
		public IEnumerable<IItemDropRule> Options => _options;
	}
}
