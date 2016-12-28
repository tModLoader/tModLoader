using System;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class ScoreCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "score";
		public override string Usage => "/score playerName <get|add|set|reset>";
		public override bool Show => false;

		public override bool VerifyArguments(string[] args) 
			=> args.Length == 2 && (args[1] == "add" || args[1] == "set" || args[1] == "reset" || args[1] == "get");

		public override void Action(string[] args)
		{
			int player;
			for (player = 0; player < 255; player++)
			{
				if (Main.player[player].active && Main.player[player].name == args[0])
				{
					break;
				}
			}
			if (player == 255)
			{
				Main.NewText("Could not find player: " + args[0]);
				return;
			}
			ExamplePlayer modPlayer = Main.player[player].GetModPlayer<ExamplePlayer>(this.Mod);
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
			if (!Int32.TryParse(args[2], out arg))
			{
				Main.NewText(args[2] + " is not an integer");
				return;
			}
			if (args[1] == "add")
			{
				modPlayer.score += arg;
			}
			else
			{
				modPlayer.score = arg;
			}
			Main.NewText(args[0] + "'s score is now " + modPlayer.score);
		}
	}
}