using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class SoundCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "sound";

		public override string Usage
			=> "/sound type style";

		public override string Description
			=> "Play sounds by id";

		public override void Action(CommandCaller caller, string input, string[] args) {
			if (!int.TryParse(args[0], out int type)) {
				throw new UsageException(args[0] + " is not an integer");
			}

			if (!int.TryParse(args[1], out int style)) {
				throw new UsageException(args[1] + " is not an integer");
			}

			SoundEngine.PlaySound(type, -1, -1, style);
		}
	}
}