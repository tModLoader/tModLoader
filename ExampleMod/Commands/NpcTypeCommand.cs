using Microsoft.Xna.Framework;
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

		public override string Description
		{
			get { return "Find mod npc ids"; }
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			var mod = ModLoader.GetMod(args[0]);
			var type = mod == null ? 0 : mod.NPCType(args[1]);
			caller.Reply(type.ToString(), Color.Yellow);
		}
	}
}
