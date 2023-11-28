using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.EntitySources
{
	// The following classes showcases pattern matching of IEntitySource instances to make things happen only in specific contexts.
	public sealed class ExampleSourceDependentProjectileTweaks : GlobalProjectile
	{
		// Always override AppliesToEntity when you can!
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) {
			return entity.type is ProjectileID.BulletDeadeye;
		}

		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			// Make bullets shot by tactical skeletons do less damage
			if (source is EntitySource_Parent parent && parent.Entity is NPC npc && npc.type == NPCID.TacticalSkeleton) {
				projectile.damage /= 2;
			}
		}
	}

	public sealed class ExampleSourceDependentItemTweaks : GlobalItem
	{
		public override void OnSpawn(Item item, IEntitySource source) {
			// Accompany all loot from trees with a slime.
			if (source is EntitySource_ShakeTree) {
				NPC.NewNPC(source, (int)item.position.X, (int)item.position.Y, NPCID.BlueSlime);
			}
		}
	}

	public sealed class ExampleSourceDependentItemTweaks2 : GlobalItem
	{
		// Always override AppliesToEntity when you can!
		public override bool AppliesToEntity(Item entity, bool lateInstantiation) {
			return entity.type is ItemID.CopperCoin or ItemID.SilverCoin or ItemID.GoldCoin or ItemID.PlatinumCoin;
		}

		public override void OnSpawn(Item item, IEntitySource source) {
			// make coins spawned from the lucky coin accessory fly into the air
			if (source.Context == "LuckyCoin") {
				item.velocity.Y -= 20;
			}
		}
	}
}
