using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalItems
{
	// This file shows you how to use AddToShotProjectile in a globalitem context. 
	// AddToShotProjectile is a method that gets called once, only when Shoot returns true.
	// It's applications include: Adding data to your item when it does something and you cannot directly modify a projectile,
	// doing deterministic or clientsided things during an event (Events don't get synced!)
	// and doing things when a projectile hits a tile in places that don't have it, such as here.
	public class ExampleGlobalEvents : GlobalItem
	{
		public override void AddToShotProjectile(Projectile projectile, Item item) {
			if (item.type == ItemID.Flamethrower) {
				// Here we add a method to be called when the projectile's Kill method is called. It says hi!
				projectile.OnProjectileKill += SayHi_OnProjectileKill;
			}

			// I want to set an NPC on fire always, but I don't want to pull over code to my GlobalProjectile, as the projectile this fires is vanilla.
			// What do I do? 
			if (item.type == ModContent.ItemType<Content.Items.Weapons.ExampleMagicWeapon>()) {
				projectile.OnProjectileHitNPC += AddADebuff_OnProjectileHitNPC;
			}
		}

		private void SayHi_OnProjectileKill(Projectile projectile) => Main.NewText("Hi!", Main.DiscoColor);
		
		private void AddADebuff_OnProjectileHitNPC(Projectile projectile, NPC npc) => npc.AddBuff(BuffID.OnFire, 50);
	}
}
