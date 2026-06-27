using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	// Showcases various debuff immunity changes. Both individual debuff immunity changes and inherited immunity changes are shown.

	// For mod compatibility, it is important to put code adjusting debuffs in the correct methods:
	// Adjust BuffID.Sets.GrantImmunityWith and NPCID.Sets.SpecificDebuffImmunity in ModNPC/GlobalNPC.SetStaticDefaults.
	// It is acceptable to adjust SpecificDebuffImmunity in PostSetupContent if there is a need to change the immunities of content from other mods and GrantImmunityWith is insufficient.
	// Only adjust NPC.buffImmune[] directly in ModNPC/GlobalNPC.SetDefaults for complex or world state specific logic.
	// Adjusting NPC.buffImmune[] during AI or SetDefaults methods is possible, but be aware that debuff inheritance will not apply to manual changes.
	// For example: While boss is spinning, set NPC.buffImmune[OnFire] = true.
	// Using the NPC.BecomeImmuneTo and NPC.ClearImmuneToBuffs methods instead of adjusting NPC.buffImmune[] directly will allow debuff inheritance. MinionBossBody showcases this approach.
	public class BuffImmuneGlobalNPC : GlobalNPC
	{
		public override void SetStaticDefaults() {
			// DONT apply to NPCID.Sets.SpecificDebuffImmunity, that's not the point.

			// This example does 2 things.
			// Firstly, it links together existing debuffs with modded debuffs. These links ensure that any NPC immune to an existing debuff will automatically be immune to the modded debuff. See ExampleJavelinDebuff.cs for another examples of this concept.
			// Secondly, it changes some existing NPC immunities.

			// This code causes any NPC immune to Ichor to automatically be immune to BetsysCurse.
			// SetDefenseDebuffStaticDefaults is called in ExampleDefenseDebuff to do the same for that buff.
			// Originally Ghost, Desert Spirit, and Ancient Vision are immune to both BetsysCurse and Ichor,
			// while Ichor Sticker and Paladin are immune to just Ichor. These will change when this mod is active.
			SetDefenseDebuffStaticDefaults(BuffID.BetsysCurse);

			// It is vital that modders use GrantImmunityWith to "inherit" immunities rather than iterate over NPCID.Sets.SpecificDebuffImmunity. GrantImmunityWith helps preserve the intentions of all the mods loaded. Failure to use GrantImmunityWith will result in NPC with illogical immunities resulting from mod code manually changing immunities one after the other.
			// NPCID.Sets.SpecificDebuffImmunity should only be modified to indicate explicit immunities, not inherited immunities.

			// NPC Buff Immunity Changes:

			// IchorSticker is already immune to Ichor, but not the others. It will automatically become immune to BetsysCurse and ExampleDefenseDebuff due to GrantImmunityWith.

			// DesertGhoulCrimson inflicts Ichor, so this mod changes it to be immune to Ichor, which in turn will cause it to be immune to BetsysCurse and ExampleDefenseDebuff as well.
			NPCID.Sets.SpecificDebuffImmunity[NPCID.DesertGhoulCrimson][BuffID.Ichor] = true;

			// Paladin is specifically immune to Ichor, but setting it to false here will make it not immune. This is the hypothetical design decision of this mod. Any other mod properly using GrantImmunityWith to inherit the Ichor buff immunity for their own Ichor variant won't cause Paladin to be immune to that buff, as intended.
			NPCID.Sets.SpecificDebuffImmunity[NPCID.Paladin][BuffID.Ichor] = false;
		}

		public static void SetDefenseDebuffStaticDefaults(int buffType) {
			// This helper method causes any NPC immune to Ichor to automatically be immune to the provided buff.
			BuffID.Sets.GrantImmunityWith[buffType].Add(BuffID.Ichor);
		}

		public override void SetDefaults(NPC entity) {
			// Any final custom global buff immunity logic can go in GlobalNPC.SetDefaults, if necessary. (This runs after ModNPC.SetDefaults)
			// Try to stick with the provided functionality for maximum compatibility instead of doing things manually.
			// For example, the following buff inheritance will apply if the NPC is immune to ANY of the specified buffs:
			// BuffID.Sets.GrantImmunityWith[ModContent.BuffType<PoisonFire>()].AddRange([BuffID.OnFire, BuffID.Poisoned]);
			// If the intention is only be immune to PoisonFire if immune to both OnFire AND Poisoned, that effect could be done here.
		}
	}
}
