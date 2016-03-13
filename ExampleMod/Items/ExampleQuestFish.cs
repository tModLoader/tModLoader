using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class ExampleQuestFish : ModItem
	{
		public override void SetDefaults()
		{
			item.name = "Upside-down Fish";
			item.questItem = true;
			item.maxStack = 1;
			item.width = 26;
			item.height = 26;
			item.uniqueStack = true;
			item.rare = -11;
		}

		public override bool IsQuestFish()
		{
			return true;
		}

		public override bool IsAnglerQuestAvailable()
		{
			return Main.hardMode;
		}

		public override void AnglerQuestChat(ref string description, ref string catchLocation)
		{
			description = "I've heard stories of a fish that swims upside-down. Supposedly you have the stand upside-down yourself to even find one. One of those would go great on my ceiling. Go fetch!";
			catchLocation = "\n(Caught anywhere while standing upside-down.)";
		}
	}
}
