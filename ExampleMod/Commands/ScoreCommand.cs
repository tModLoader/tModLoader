using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class ScoreCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.Chat; }
		}

		public override string Command
		{
			get { return "score"; }
		}

		public override string Usage
		{
			get { return "/score playerName <get|add|set|reset>"; }
		}

		public override bool Show
		{
			get { return false; }
		}

		public override bool VerifyArguments(string[] args)
		{
			return args.Length == 2 && (args[1] == "add" || args[1] == "set" || args[1] == "reset" || args[1] == "get");
		}

		public override void Action(string[] args)
		{
			int player;
			for (player = 0; player < 255; player++)
				if (Main.player[player].active && Main.player[player].name == args[0])
					break;
			if (player == 255)
			{
				Main.NewText("Could not find player: " + args[0]);
				return;
			}
			var modPlayer = Main.player[player].GetModPlayer<ExamplePlayer>(Mod);
			if (args[1] == "get")
			{
				Main.NewText(args[0] + "'s score is " + modPlayer.score);
				return;
			}
			if (args[1] == "reset")
			{
				modPlayer.score = 0;
				Main.NewText(args[0] + "'s score is now " + modPlayer.score);
				return;
			}
			if (args.Length < 3)
			{
				Main.NewText("Usage: /score playerName <add|set> amount");
				return;
			}
			int arg;
			if (!int.TryParse(args[2], out arg))
			{
				Main.NewText(args[2] + " is not an integer");
				return;
			}
			if (args[1] == "add")
				modPlayer.score += arg;
			else
				modPlayer.score = arg;
			Main.NewText(args[0] + "'s score is now " + modPlayer.score);
		}
	}
}