using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	class MusicBox : GlobalItem
	{
		public override void UpdateAccessory(Item item, Player player)
		{
			if (Main.myPlayer == player.whoAmI)
			{
				if (item.type == ItemID.MusicBox && Main.rand.Next(10800) == 0 && Main.curMusic >= Main.maxMusic && Main.curMusic < Main.music.Length)
				{
					if (Main.curMusic == mod.GetSoundSlot(SoundType.Music, "Sounds/Music/DriveMusic"))
					{
						item.SetDefaults(mod.ItemType("ExampleMusicBox"), false);
					}
				}
			}
		}
	}
}
