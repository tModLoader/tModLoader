using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class AddTimeCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.World; }
		}

		public override string Command
		{
			get { return "addTime"; }
		}

		public override string Usage
		{
			get { return "/addTime numTicks"; }
		}

		public override string Description 
		{
			get { return "Add or rewind world time"; }
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			double fullTime = Main.time;
			if (!Main.dayTime)
				fullTime += 54000.0;

			fullTime += int.Parse(args[0]);
			fullTime %= 86400.0;
			if (fullTime < 0)
				fullTime += 86400;

			Main.dayTime = fullTime < 54000;
			Main.time = fullTime;
			if (!Main.dayTime)
				Main.time -= 54000;

			if (Main.netMode == 2)
				NetMessage.SendData(MessageID.WorldData);
		}
	}
}