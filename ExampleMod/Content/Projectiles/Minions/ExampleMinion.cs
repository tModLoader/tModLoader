using Terraria.ModLoader;

namespace ExampleMod.Content.Projectiles.Minions
{
	public abstract class ExampleMinion : ModProjectile
	{
		public override void AI() {
			// The minion's AI is determined by the contents of these methods
			CheckActive();
			Behavior();
		}
		
		// These methods are left abstract so a subclass can define them		

		public abstract void CheckActive();

		public abstract void Behavior();
	}
}