using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.Commands
{
	public class Testing : ModCommand
	{
		public static bool GlobalCanShimmer;
		// CommandType.World means that command can be used in Chat in SP and MP, but executes on the Server in MP
		public override CommandType Type
			=> CommandType.Chat;

		// The desired text to trigger this command
		public override string Command
			=> "test";

		// A short usage explanation for this co;mmand
		public override string Usage => "";

		// A short description of this command
		public override string Description => "";

		public override void Action(CommandCaller caller, string input, string[] args) {
			GlobalCanShimmer = !GlobalCanShimmer;
			Main.NewText(nameof(GlobalCanShimmer) + " set: " + GlobalCanShimmer);
		}
	}
}