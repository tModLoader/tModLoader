using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	public class ExampleGlobalLiquidNPC : GlobalNPC
	{
		//GlobalLiquid doesn't have a field/property or method/hook for the default of vanilla liquid movement speeds
		//So instead, using SetDefaults we set the lavaMovementSpeed to 1f for all NPCs
		public override void SetDefaults(NPC entity) {
			entity.lavaMovementSpeed = 1f;
		}
	}
}
