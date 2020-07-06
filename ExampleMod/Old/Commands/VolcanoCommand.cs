using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ExampleMod.Commands
{
	internal class VolcanoCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Console;

		public override string Command
			=> "volcano";

		public override string Description
			=> "Trigger a volcano";

		public override void Action(CommandCaller caller, string input, string[] args) {
			const string key = "Mods.ExampleMod.VolcanoWarning";
			Color messageColor = Color.Orange;
			NetMessage.BroadcastChatMessage(NetworkText.FromKey(key), messageColor);
			ExampleWorld exampleWorld = GetInstance<ExampleWorld>();
			exampleWorld.VolcanoCountdown = ExampleWorld.DefaultVolcanoCountdown;
			exampleWorld.VolcanoCooldown = ExampleWorld.DefaultVolcanoCooldown;
		}
	}
}
