using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class NpcTypeCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Command
		{
			get { return "npcType"; }
		}

		public override string Usage
		{
			get { return "/npcType modName npcName"; }
		}

		public override bool Show
		{
			get { return false; }
		}

		public override bool VerifyArguments(string[] args)
		{
			return args.Length == 2;
		}

		public override void Action(string[] args)
		{
			var mod = ModLoader.GetMod(args[0]);
			var type = mod == null ? 0 : mod.NPCType(args[1]);
			Main.NewText(type.ToString(), 255, 255, 0);
		}
	}
}
