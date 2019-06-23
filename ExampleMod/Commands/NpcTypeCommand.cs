using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class NpcTypeCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "npcType";

		public override string Usage
			=> "/npcType modName npcName";

		public override string Description
			=> "Find mod npc ids";

		public override void Action(CommandCaller caller, string input, string[] args) {
			var theMod = ModLoader.GetMod(args[0]);
			var type = theMod?.NPCType(args[1]) ?? 0;
			caller.Reply(type.ToString(), Color.Yellow);
		}
	}
}
