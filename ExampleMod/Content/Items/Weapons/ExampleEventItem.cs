using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ExampleMod.Content.Items.Weapons
{
	public class ExampleEventItem : ModItem
	{
		private int timesHit;
		public override string Texture => "Examplemod/Content/Items/Weapons/ExampleShootingSword";
		public override void SetDefaults() {
			item.useTime = 20;
			item.useAnimation = 20;
			item.useStyle = ItemUseStyleID.Shoot;
			item.autoReuse = true;
			item.damage = 20;
			item.DamageType = DamageClass.Ranged;
			item.width = 32;
			item.height = 32;
			item.shoot = 10;
			// This Ammo is nonspecific. I may not be able to control this, which is why I should use the events.
			item.useAmmo = AmmoID.Bullet;
		}
		// Here I add something to every shot it fires. Notice that I'm not overriding shoot.
		// The Shoot method returns 'True' by default. The method below will only be called if it returns true.
		public override void AddToShotProjectile(Projectile projectile) {
			projectile.OnProjectileHitNPC += Projectile_OnProjectileHitNPC;
		}
		// Here I launch the NPC up, give it the Cursed inferno debuff, and size it up by 1.2f when it is hit by this.
		private void Projectile_OnProjectileHitNPC(Projectile projectile, NPC npc) {
			DisplayName.SetDefault($"I have been used {timesHit} times!");
			npc.velocity.Y -= 5;
			npc.AddBuff(BuffID.CursedInferno, 60);
			npc.scale *= 1.2f;
		}
	}
}
