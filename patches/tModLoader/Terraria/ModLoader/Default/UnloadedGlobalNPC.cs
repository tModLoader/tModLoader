using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace Terraria.ModLoader.Default
{
	public class UnloadedGlobalNPC : GlobalNPC
	{
		internal IList<TagCompound> data = new List<TagCompound>();

		public override bool InstancePerEntity => true;

		public override bool NeedCustomSaving(NPC npc) => data.Count > 0;
	}
}
