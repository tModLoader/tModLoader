using ExampleMod.NPCs;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Buffs
{
	public class ExampleJavelin : ModBuff
	{
		public override bool Autoload(ref string name, ref string texture) {
			// NPC only buff so we'll just assign it a useless buff icon.
			texture = "ExampleMod/Buffs/BuffTemplate";
			return base.Autoload(ref name, ref texture);
		}

		public override void SetDefaults() {
			DisplayName.SetDefault("Example Javelin");
			Description.SetDefault("Losing life");
		}

		public override void Update(NPC npc, ref int buffIndex) {
			npc.GetGlobalNPC<ExampleGlobalNPC>().exampleJavelin = true;
		}
	}
}
