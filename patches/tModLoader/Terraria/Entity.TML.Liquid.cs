using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;

namespace Terraria;
partial class Entity
{
	/// <summary>
	/// This field is for detecting if an entity is "Wet", it is meant to simplify the current check which store the different wet check in separate field
	/// 0 = wet/Water
	/// 1 = lavaWet
	/// 2 = honeyWet
	/// 3 = shimmerWet
	/// </summary>
	protected bool[] liquidWet = new bool[LiquidLoader.LiquidCount];

	public bool waterWet => wet && liquidWet.Count(i => true) == 1;

	public bool IsInLiquid<T>() where T : ModLiquid => IsInLiquid(ModContent.LiquidType<T>());

	public bool IsInLiquid(int type) => liquidWet[type];
}
