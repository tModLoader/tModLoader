using ExampleMod.Common.GlobalItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

//Related to GlobalItem: WeaponWithGrowingDamage
namespace ExampleMod.Common.GlobalProjectiles
{
	public class ProjectileWithGrowingDamage : GlobalProjectile
	{
		private WeaponWithGrowingDamage sourceGlobalItem;

		public override bool InstancePerEntity => true;
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			//Don't try to store the itemSource.Item.  Terraria can re-use an item instance with SetDefaults(),
			//meaning the instance you save could become air or another item.  It is much safer to store the GlobalItem instance.
			if (source is EntitySource_ItemUse itemSource) {
				itemSource.Item.TryGetGlobalItem(out sourceGlobalItem);
			}
		}

		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
			if (sourceGlobalItem == null) {
				return;
			}

			int owner = projectile.owner;
			if (owner < 0 || owner >= Main.player.Length) {
				return;
			}

			Player player = Main.player[owner];
			sourceGlobalItem.OnHitNPCGeneral(player, target, damage, knockback, crit, projectile: projectile);
		}
	}
}
