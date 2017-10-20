using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Terraria.ID
{
	public static class ItemHoldStyleID
	{
		[Description("Zero (0)\nDefault\nUsed by any item by default")]
		public const int Default = 0;
		[Description("One (1)\nHolding out\nUsed for items such as torches and glowsticks")]
		public const int HoldingOut = 1;
		[Description("Two (2)\nHolding up \nUsed only by Breathing Reed (ID: 186)")]
		public const int HoldingUp = 2;
		[Description("Three (3)\nHolding out\nUsed only by Magical Harp, a custom style of holding out")]
		public const int HarpHoldingOut = 3;
	}
}
