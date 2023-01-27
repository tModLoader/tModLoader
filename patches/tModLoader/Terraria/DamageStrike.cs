using System;
using Terraria.ModLoader;

namespace Terraria;

public struct DamageStrike
{
	internal BitsByte Flags;

	public DamageClass DamageType;
	public int Damage;
	public bool Crit { readonly get => Flags[0]; set => Flags[0] = value; }
	public int HitDirection { readonly get => Flags[1] ? 1 : -1; set => Flags[1] = value >= 0; }

	public float KnockBack;

	public bool InstantKill { readonly get => Flags[2]; set => Flags[2] = value; }
	public bool HideCombatText { readonly get => Flags[3]; set => Flags[3] = value; }
}
