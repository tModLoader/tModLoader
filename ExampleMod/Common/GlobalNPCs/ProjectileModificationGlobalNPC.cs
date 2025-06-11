using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	// This is a class for functionality related to ExampleProjectileModifications.
	public class ProjectileModificationGlobalNPC : GlobalNPC
	{
		public override bool InstancePerEntity => true;
		public int timesHitByModifiedProjectiles;
	}
}
