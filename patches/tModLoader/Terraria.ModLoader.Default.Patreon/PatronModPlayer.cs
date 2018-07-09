using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class PatronModPlayer : ModPlayer
	{
		public override bool CloneNewInstances => true;

		public bool OrianSet;

		public static PatronModPlayer Player(Player player) => player.GetModPlayer<PatronModPlayer>();

		public override void ResetEffects()
		{
			OrianSet = false;
		}

		public override void PostUpdate()
		{
			if (OrianSet)
			{
				var close = Main.npc
					.Where(x => x.active && !x.friendly && !NPCID.Sets.TownCritter[x.type] && x.type != NPCID.TargetDummy)
					.FirstOrDefault(x => x.Distance(player.position) <= Main.screenWidth / 4f);
				
				if (close != null) Lighting.AddLight(player.Center, Color.DeepSkyBlue.ToVector3() * 1.5f);
			}
		}
	}
}