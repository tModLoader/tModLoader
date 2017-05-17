using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	class VolcanoCommand : ModCommand
	{
		public override CommandType Type
		{
			get { return CommandType.Console; }
		}

		public override string Command
		{
			get { return "volcano"; }
		}

		public override string Description
		{
			get { return "Trigger a volcano"; }
		}

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			string key = "Mods.ExampleMod.VolcanoWarning";
			Color messageColor = Color.Orange;
			NetMessage.BroadcastChatMessage(NetworkText.FromKey(key), messageColor);
			ExampleWorld exampleWorld = mod.GetModWorld<ExampleWorld>();
			exampleWorld.VolcanoCountdown = ExampleWorld.DefaultVolcanoCountdown;
			exampleWorld.VolcanoCooldown = ExampleWorld.DefaultVolcanoCooldown;
		}
	}
}
