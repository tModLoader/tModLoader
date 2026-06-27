using ExampleMod.Common.Configs;
using ExampleMod.Common.GlobalItems;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

// Related to GlobalItem: WeaponWithGrowingDamage
namespace ExampleMod.Common.GlobalProjectiles
{
	public class ProjectileWithGrowingDamage : GlobalProjectile
	{
		private WeaponWithGrowingDamage sourceGlobalItem;

		public override bool InstancePerEntity => true;

		public override bool IsLoadingEnabled(Mod mod) {
			// To experiment with this example, you'll need to enable it in the config.
			return ModContent.GetInstance<ExampleModConfig>().WeaponWithGrowingDamageToggle;
		}

		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			// Don't try to store the itemSource.Item. Terraria can re-use an item instance with SetDefaults(),
			// meaning the instance you save could become air or another item. It is much safer to store the GlobalItem instance.
			if (source is IEntitySource_WithStatsFromItem itemSource) {
				itemSource.Item.TryGetGlobalItem(out sourceGlobalItem);
			}
		}

		public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) {
			if (sourceGlobalItem == null) {
				return;
			}

			int owner = projectile.owner;
			if (owner < 0 || owner >= Main.player.Length) {
				return;
			}

			Player player = Main.player[owner];
			sourceGlobalItem.OnHitNPCGeneral(player, target, hit, projectile: projectile);
		}
	}
}
