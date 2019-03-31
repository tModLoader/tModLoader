using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class NpcCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.World;

		public override string Command
			=> "npc";

		public override string Usage
			=> "/npc type [x] [y] [number]\nx and y may be preceded by ~ to use position relative to player";

		public override string Description
			=> "Spawn an npc";

		public override void Action(CommandCaller caller, string input, string[] args) {
			if (!int.TryParse(args[0], out int type)) {
				throw new UsageException(args[0] + " is not an integer");
			}

			int x;
			int y;
			var num = 1;
			if (args.Length > 2) {
				var relativeX = false;
				var relativeY = false;
				if (args[1][0] == '~') {
					relativeX = true;
					args[1] = args[1].Substring(1);
				}
				if (args[2][0] == '~') {
					relativeY = true;
					args[2] = args[2].Substring(1);
				}
				if (!int.TryParse(args[1], out x)) {
					x = 0;
					relativeX = true;
				}
				if (!int.TryParse(args[2], out y)) {
					y = 0;
					relativeY = true;
				}
				if (relativeX) {
					x += (int)caller.Player.Bottom.X;
				}

				if (relativeY) {
					y += (int)caller.Player.Bottom.Y;
				}

				if (args.Length > 3) {
					if (!int.TryParse(args[3], out num)) {
						num = 1;
					}
				}
			}
			else {
				x = (int)caller.Player.Bottom.X;
				y = (int)caller.Player.Bottom.Y;
			}
			for (var k = 0; k < num; k++) {
				int slot = NPC.NewNPC(x, y, type);
				//if (Main.netMode == 2 && slot < 200)
				//	NetMessage.SendData(MessageID.SyncNPC, -1, -1, "", slot);
			}
		}
	}
}
