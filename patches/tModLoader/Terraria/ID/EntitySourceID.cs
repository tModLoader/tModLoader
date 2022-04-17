using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terraria.ID
{
	// Added by TML.
	// !!! Won't be needed in Terraria 1.4.4 !!!
	public static class EntitySourceID
	{
		// Common
		public const int None = 0;
		// From ProjectileSourceID
		public const int SetBonus_SolarExplosion_WhenTakingDamage = 1;
		public const int SetBonus_SolarExplosion_WhenDashing = 2;
		public const int SetBonus_ForbiddenStorm = 3;
		public const int SetBonus_Titanium = 4;
		public const int SetBonus_Orichalcum = 5;
		public const int SetBonus_Chlorophyte = 6;
		public const int SetBonus_Stardust = 7;
		public const int WeaponEnchantment_Confetti = 8;
		public const int Death_TombStone = 9; // PlayerDeath_TombStone
		public const int TorchGod = 10;
		public const int FallingStar = 11;
		public const int PlayerHurt_DropFootball = 12;
		public const int StormTigerTierSwap = 13;
		public const int AbigailTierSwap = 14;
		public const int SetBonus_GhostHeal = 15;
		public const int SetBonus_GhostHurt = 16;
		public const int VampireKnives = 18;
		// From ItemSourceID
		public const int SetBonus_Nebula = 19;
		public const int LuckyCoin = 20;
		//public const int PlayerDeath = 21; // EntitySource_Death now exists.
		public const int PlayerDropItemCheck = 21;
		public const int GrandDesignOrMultiColorWrench = 22;
		// Common
		public const int Count = 23;
	}
}
