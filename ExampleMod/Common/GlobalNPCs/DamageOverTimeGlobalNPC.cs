using ExampleMod.Content.Buffs;
using ExampleMod.Content.Projectiles;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	internal class DamageOverTimeGlobalNPC : GlobalNPC
	{
		// Add a buff type here to opt it into the accessory's amplification.
		public static readonly HashSet<int> AmplifiedDebuffs = new();

		public override bool InstancePerEntity => true;
		public bool exampleJavelinDebuff;
		public bool examplePoison;
		public bool exampleFire;
		public bool exampleAmplification;

		public override void SetStaticDefaults() {
			AmplifiedDebuffs.Add(BuffID.Poisoned);
			AmplifiedDebuffs.Add(BuffID.OnFire);
		}

		public override void Unload() {
			AmplifiedDebuffs.Clear();
		}

		public override void ResetEffects(NPC npc) {
			exampleJavelinDebuff = examplePoison = exampleFire = exampleAmplification = false;
		}

		public override void UpdateDOTTally(NPC npc, ref NPC.DOTTally tally) {
			if (exampleJavelinDebuff) {
				int count = 0;
				foreach (var projectile in Main.ActiveProjectiles) {
					if (projectile.type == ModContent.ProjectileType<ExampleJavelinProjectile>() && projectile.ai[0] == 1f && projectile.ai[1] == npc.whoAmI) {
						count++;
					}
				}

				// LifeRegenLoss uses half-life units; the second value controls damage batching.
				tally.Add(count * 6, count * 3, ModContent.BuffType<ExampleJavelinDebuff>());
			}

			if (examplePoison) {
				// AddPoison also participates in Catalyst Band's poison acceleration.
				tally.AddPoison(20, 2, ModContent.BuffType<ExamplePoisonDebuff>());
			}

			if (exampleFire) {
				// AddFlamable applies Slimed and enables the shared Oiled bonus.
				tally.AddFlamable(20, 2, ModContent.BuffType<ExampleFireDebuff>());
			}
		}

		public override void ModifyDOT(NPC npc, int buffType, ref NPC.DOTTally contribution) {
			if (exampleAmplification && AmplifiedDebuffs.Contains(buffType)) {
				// Adjust both loss fields so accelerated damage receives the same multiplier.
				contribution.LifeRegenLoss *= 2;
				contribution.LifeRegenLossThatCanBeBoosted *= 2;
				contribution.ExpectedDamagePerSecond *= 2;
			}
		}
	}
}
