using System;
using Terraria.ModLoader;

namespace Terraria;

public partial class NPC
{
	public partial struct DOTTally
	{
		/// <summary>Whether a fire contribution is present. Enables the shared Oiled bonus, including for modded fire.</summary>
		public bool HasFlammableDamage {
			get => _hasFlammableDamage;
			set => _hasFlammableDamage = value;
		}

		/// <summary>
		/// Adds an identified contribution after <see cref="ModNPC.ModifyDOT(int, ref DOTTally)"/> and <see cref="GlobalNPC.ModifyDOT(NPC, int, ref DOTTally)"/>.
		/// The contribution must belong to the same NPC. Its damage values must be nonnegative.
		/// </summary>
		public void AddContribution(int buffType, DOTTally contribution)
		{
			NPCLoader.ModifyDOT(npc, buffType, ref contribution);
			LifeRegenLoss += contribution.LifeRegenLoss;
			LifeRegenLossThatCanBeBoosted += contribution.LifeRegenLossThatCanBeBoosted;
			ExpectedDamagePerSecond = Math.Max(ExpectedDamagePerSecond, contribution.ExpectedDamagePerSecond);
			BlockNaturalRegeneration |= contribution.BlockNaturalRegeneration;
			SuperNaturalLifeRegen += contribution.SuperNaturalLifeRegen;
			HasFlammableDamage |= contribution.HasFlammableDamage;
		}

		/// <summary>Adds damage from the specified buff, allowing per-buff modification. Life regeneration loss is twice the damage per second.</summary>
		public void Add(int regenLoss, int expectedAmountPerSecond, int buffType)
		{
			DOTTally contribution = new() { npc = npc };
			contribution.Add(regenLoss, expectedAmountPerSecond);
			AddContribution(buffType, contribution);
		}

		/// <summary>Adds poison damage from the specified buff, including poison acceleration and per-buff modification.</summary>
		public void AddPoison(int regenLoss, int expectedAmountPerSecond, int buffType)
		{
			DOTTally contribution = new() { npc = npc };
			contribution.AddPoison(regenLoss, expectedAmountPerSecond);
			AddContribution(buffType, contribution);
		}

		/// <summary>Adds bleeding damage from the specified buff, including poison acceleration and per-buff modification.</summary>
		public void AddBloodLoss(int regenLoss, int expectedAmountPerSecond, int buffType)
		{
			DOTTally contribution = new() { npc = npc };
			contribution.AddBloodLoss(regenLoss, expectedAmountPerSecond);
			AddContribution(buffType, contribution);
		}

		/// <summary>Adds fire damage from the specified buff, including vanilla fire interactions and per-buff modification.</summary>
		public void AddFlamable(int regenLoss, int expectedAmountPerSecond, int buffType)
		{
			DOTTally contribution = new() { npc = npc };
			contribution.AddFlamable(regenLoss, expectedAmountPerSecond);
			AddContribution(buffType, contribution);
		}

		/// <summary>Adds damage from attached projectiles for the specified buff, allowing per-buff modification.</summary>
		public void AddStackedProjecitle(short projTypeToLookFor, int damagePerSecond, int expectedDamagePerSecondDivider, int buffType)
		{
			DOTTally contribution = new() { npc = npc };
			contribution.AddStackedProjecitle(projTypeToLookFor, damagePerSecond, expectedDamagePerSecondDivider);
			AddContribution(buffType, contribution);
		}
	}
}
