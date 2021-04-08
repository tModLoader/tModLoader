using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Commands
{
	public class ExampleTimeCommand : ModCommand
	{
		// CommandType.World means that command can be used in Chat in SP and MP, but executes on the Server in MP
		public override CommandType Type
			=> CommandType.World;

		// The desired text to trigger this command
		public override string Command
			=> "addTime";

		// A short usage explanation for this command
		public override string Usage
			=> "/addTime numTicks" +
			"\n numTicks — positive or negative number in ticks(1 second = 60 ticks).";

		// A short description of this command
		public override string Description
			=> "Adds numTicks to fast forward or rewind world time";

		public override void Action(CommandCaller caller, string input, string[] args) {
			// Time in ticks for complete full day+night cycle (86600)
			const double cycleLength = Main.dayLength + Main.nightLength;
			// Checking input Arguments
			if (args.Length == 0)
				throw new UsageException("At least one argument was expected.");
			if (!int.TryParse(args[0], out int extraTime))
				throw new UsageException(args[0] + " is not a correct integer value.");


			// Convert current time(0-54000 for day and 0-32400 for night) to cycle time(0-86600)
			double fullTime = Main.time;
			if (!Main.dayTime)
				fullTime += Main.dayLength;


			// Add time from argument
			fullTime += extraTime;
			// Cap the time when the cycle time range is exceeded (fullTime < 0 || fullTime > 86600)
			fullTime %= cycleLength;
			if (fullTime < 0)
				fullTime += cycleLength;


			// If fullTime(0-86600) < dayLength(54000) its a day, otherwise night
			Main.dayTime = fullTime < Main.dayLength;
			// Convert cycle time to default day/night time
			if (!Main.dayTime)
				fullTime -= Main.dayLength;
			Main.time = fullTime;


			// Sync of world data on the server in MP
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendData(MessageID.WorldData);
		}
	}
}