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

	private int _armorPenetration;
	public int ArmorPenetration { readonly get => _armorPenetration; set => _armorPenetration = Math.Max(0, value); }

	private int _armorPenetrationPercent;
	public int ArmorPenetrationPercent { readonly get => _armorPenetrationPercent; set => _armorPenetrationPercent = Utils.Clamp(value, 0, 100); }

	public bool InstantKill { readonly get => Flags[2]; set => Flags[2] = value; }
	public bool HideCombatText { readonly get => Flags[3]; set => Flags[3] = value; }

	public readonly int GetRemainingDefense(int baseDefense) => Math.Max(baseDefense * (100 - ArmorPenetrationPercent) / 100 - ArmorPenetration, 0);
}
