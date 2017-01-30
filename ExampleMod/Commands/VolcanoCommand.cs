using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
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
			string message = "Did you hear something....A Volcano! Find Cover!";
			Color messageColor = Color.Orange;
			NetMessage.SendData(25, -1, -1, message, 255, messageColor.R, messageColor.G, messageColor.B, 0);
			ExampleWorld exampleWorld = mod.GetModWorld<ExampleWorld>();
			exampleWorld.VolcanoCountdown = ExampleWorld.DefaultVolcanoCountdown;
			exampleWorld.VolcanoCooldown = ExampleWorld.DefaultVolcanoCooldown;
		}
	}
}
