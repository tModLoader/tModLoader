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
		private Item sourceItem;
		private int sourceItemType;

		public override bool InstancePerEntity => true;
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			if (source is EntitySource_ItemUse itemSource) {
				sourceItem = itemSource.Item;
			}
			else if (source is EntitySource_ItemUse_WithAmmo itemWithUseAmmoSource) {
				sourceItem = itemWithUseAmmoSource.Item;
			}

			if (sourceItem != null)
				sourceItemType = sourceItem.type;
		}

		public override void OnHitNPC(Projectile projectile, NPC target, int damage, float knockback, bool crit) {
			if (sourceItem == null || sourceItem.type != sourceItemType || !sourceItem.TryGetGlobalItem(out WeaponWithGrowingDamage weapon))
				return;

			int owner = projectile.owner;
			if (owner < 0 || owner >= Main.player.Length)
				return;

			Player player = Main.player[owner];
			weapon.OnHitNPCGeneral(sourceItem, player, target, damage, knockback, crit);
		}
	}
}
