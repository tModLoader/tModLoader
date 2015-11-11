using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod
{
	public class ExamplePlayer : ModPlayer
	{
		private const int saveVersion = 0;
		public int score = 0;
		public bool elementShield = false;
		private int elementShields = 0;
		public int voidMonolith = 0;

		public override void ResetEffects()
		{
			elementShield = false;
			if (voidMonolith > 0)
			{
				voidMonolith--; //this is a very bad hack until I create ModWorld
			}
		}

		public override void SaveCustomData(BinaryWriter writer)
		{
			writer.Write(saveVersion);
			writer.Write(score);
		}

		public override void LoadCustomData(BinaryReader reader)
		{
			int loadVersion = reader.ReadInt32();
			score = reader.ReadInt32();
		}

		public override void SetupStartInventory(IList<Item> items)
		{
			Item item = new Item();
			item.SetDefaults(mod.ItemType("ExampleItem"));
			item.stack = 5;
			items.Add(item);
		}

		public override void UpdateBiomeVisuals()
		{
			bool usePurity = NPC.AnyNPCs(mod.NPCType("PuritySpirit"));
			player.ManageSpecialBiomeVisuals("ExampleMod:PuritySpirit", usePurity);
			bool useVoidMonolith = voidMonolith > 0 && !usePurity && !NPC.AnyNPCs(NPCID.MoonLordCore);
			player.ManageSpecialBiomeVisuals("ExampleMod:MonolithVoid", useVoidMonolith, player.Center);
		}
	}
}
