using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	//note this command is effectively broken in multiplayer due to the lack of netcode around ExamplePlayer.score
	public class ScoreCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "score";

		public override string Usage
			=> "/score playerName <get|reset>" +
			"\n/score playerName <add|set> amount";

		public override string Description
			=> "Manipulate ExamplePlayer.score";

		public override void Action(CommandCaller caller, string input, string[] args) {
			int player;
			for (player = 0; player < 255; player++) {
				if (Main.player[player].active && Main.player[player].name == args[0]) {
					break;
				}
			}

			if (player == 255) {
				throw new UsageException("Could not find player: " + args[0]);
			}
			var modPlayer = Main.player[player].GetModPlayer<ExamplePlayer>();
			if (args[1] == "get") {
				caller.Reply(args[0] + "'s score is " + modPlayer.score);
				return;
			}
			if (args[1] == "reset") {
				modPlayer.score = 0;
				caller.Reply(args[0] + "'s score is now " + modPlayer.score);
				return;
			}
			if (args.Length < 3) {
				throw new UsageException("Usage: /score playerName <add|set> amount");
			}

			if (!int.TryParse(args[2], out int arg)) {
				throw new UsageException(args[2] + " is not an integer");
			}
			if (args[1] == "add") {
				modPlayer.score += arg;
			}
			else {
				modPlayer.score = arg;
			}

			Main.NewText(args[0] + "'s score is now " + modPlayer.score);
		}
	}
}