using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	public class ExampleNPCHappiness : GlobalNPC 
	{
		public override void SetStaticDefaults() {
			int examplePersonType = ModContent.NPCType<Content.NPCs.ExamplePerson>(); // Get ExamplePerson's type
			var guideNpc = ContentSamples.NpcsByNetId[NPCID.Guide]; // Get The Guide's content sample

			guideNpc.Happiness.LikeNPC(examplePersonType); // Make the Guide like ExamplePerson!
		}
	}
}
