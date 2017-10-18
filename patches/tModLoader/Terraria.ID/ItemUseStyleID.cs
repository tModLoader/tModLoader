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
		public static int SwingThrow = 1;
		[Description("Two (2)\nEating or using\nUsed for many consumables such as potions or food")]
		public static int EatingUsing = 2;
		[Description("Three (3)\nStabbing\nUsed for shortswords")]
		public static int Stabbing = 3;
		[Description("Four (4)\nHolding up\nUsed for items such as mana/life crystals, life fruit and summoning items")]
		public static int HoldingUp = 4;
		[Description("Five (5)\nHolding out\nUsed for items such as guns, spellbooks, flails and spears")]
		public static int HoldingOut = 5;
	}
}
