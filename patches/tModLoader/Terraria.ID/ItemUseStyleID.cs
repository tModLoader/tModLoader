using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Terraria.ID
{
	public static class ItemUseStyleID
	{
		[Description("One (1)\nSwinging and throwing\nUsed for many weapons, block placement etc.")]
		public const int SwingThrow = 1;
		[Description("Two (2)\nEating or using\nUsed for many consumables such as potions or food")]
		public const int EatingUsing = 2;
		[Description("Three (3)\nStabbing\nUsed for shortswords")]
		public const int Stabbing = 3;
		[Description("Four (4)\nHolding up\nUsed for items such as mana/life crystals, life fruit and summoning items")]
		public const int HoldingUp = 4;
		[Description("Five (5)\nHolding out\nUsed for items such as guns, spellbooks, flails and spears")]
		public const int HoldingOut = 5;
	}
}
