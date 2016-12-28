using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class NpcTypeCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "npcType";
		public override string Usage => "/npcType modName npcName";
		public override bool Show => false;
		public override bool VerifyArguments(string[] args) => args.Length == 2;

		public override void Action(string[] args)
		{
			Mod mod = ModLoader.GetMod(args[0]);
			int type = mod == null ? 0 : mod.NPCType(args[1]);
			Main.NewText(type.ToString(), 255, 255, 0);
		}
	}
}
