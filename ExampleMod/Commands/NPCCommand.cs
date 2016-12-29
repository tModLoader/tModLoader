using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class NpcCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Command
		{
			get { return "npc"; }
		}

		public override string Usage
		{
			get { return "/npc type [x] [y] [number]; x and y may be preceded by ~ to use position relative to player"; }
		}

		public override bool Show
		{
			get { return false; }
		}

		public override bool VerifyArguments(string[] args)
		{
			int type;
			return args.Length == 4 && int.TryParse(args[0], out type);
		}

		public override void Action(string[] args)
		{
			var type = int.Parse(args[0]);
			var player = Main.LocalPlayer;
			int x;
			int y;
			var num = 1;
			if (args.Length > 2)
			{
				var relativeX = false;
				var relativeY = false;
				if (args[1][0] == '~')
				{
					relativeX = true;
					args[1] = args[1].Substring(1);
				}
				if (args[2][0] == '~')
				{
					relativeY = true;
					args[2] = args[2].Substring(1);
				}
				if (!int.TryParse(args[1], out x))
				{
					x = 0;
					relativeX = true;
				}
				if (!int.TryParse(args[2], out y))
				{
					y = 0;
					relativeY = true;
				}
				if (relativeX)
					x += (int) player.Bottom.X;
				if (relativeY)
					y += (int) player.Bottom.Y;
				if (args.Length > 3)
					if (!int.TryParse(args[3], out num))
						num = 1;
			}
			else
			{
				x = (int) player.Bottom.X;
				y = (int) player.Bottom.Y;
			}
			for (var k = 0; k < num; k++)
				if (Main.netMode == 0)
				{
					NPC.NewNPC(x, y, type);
				}
				else if (Main.netMode == 1)
				{
					var netMessage = Mod.GetPacket();
					netMessage.Write((byte) ExampleModMessageType.SpawnNPC);
					netMessage.Write(x);
					netMessage.Write(y);
					netMessage.Write(type);
					netMessage.Send();
				}
		}
	}
}
