using System.Threading;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class LagCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.World;

		public override string Command
			=> "lag";

		public override string Usage
			=> "/lag duration (ms)";

		public override string Description
			=> "Pause the main thread for a period of time";

		public override void Action(CommandCaller caller, string input, string[] args) {
			Thread.Sleep(int.Parse(args[0]));
		}
	}
}