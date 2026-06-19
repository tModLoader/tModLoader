using ExampleMod.Content.Biomes;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs
{
	public class ExampleNPCHappiness : GlobalNPC
	{
		public override void SetStaticDefaults() {
			int examplePersonType = ModContent.NPCType<Content.NPCs.ExamplePerson>(); // Get ExamplePerson's type
			var guideHappiness = NPCHappiness.Get(NPCID.Guide); // Get the key into The Guide's happiness
			var zoologistHappiness = NPCHappiness.Get(NPCID.BestiaryGirl); // Get the key into The Zoologist's happiness

			guideHappiness.SetNPCAffection(examplePersonType, AffectionLevel.Love); // Make the Guide love ExamplePerson!
			zoologistHappiness.SetNPCAffection(examplePersonType, AffectionLevel.Like); // Make the Zoologist like ExamplePerson!

			guideHappiness.SetBiomeAffection<ExampleSurfaceBiome>(AffectionLevel.Love);  // Make the Guide love ExampleSurfaceBiome!
		}
	}
}
