using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ExampleMod.Items
{
	public class ExampleInstancedGlobalItem : GlobalItem
	{
		public static int itemsCrafted;

		public string originalOwner;

		public override bool InstancePerEntity
		{
			get
			{
				return true;
			}
		}

		public override GlobalItem NewInstance(Item item)
		{
			return base.NewInstance(item);
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (originalOwner != null)
			{
				TooltipLine line = new TooltipLine(mod, "ExampleTooltip", "Crafted by: " + originalOwner);
				line.overrideColor = Color.LimeGreen;
				tooltips.Add(line);
			}
		}

		public override void Load(Item item, TagCompound tag)
		{
			originalOwner = tag.GetString("originalOwner");
		}

		public override bool NeedsSaving(Item item)
		{
			return true;
		}

		public override TagCompound Save(Item item)
		{
			return new TagCompound {
				{"originalOwner", originalOwner}
			};
		}

		public override void OnCraft(Item item, Recipe recipe)
		{
			originalOwner = Main.LocalPlayer.name;
		}

		public override void NetSend(Item item, BinaryWriter writer)
		{
			writer.Write(originalOwner);
		}

		public override void NetReceive(Item item, BinaryReader reader)
		{
			originalOwner = reader.ReadString();
		}
	}
}
