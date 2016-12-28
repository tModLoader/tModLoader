using System;
using ExampleMod.UI;
using Terraria;
using Terraria.ModLoader;

namespace ExampleMod.Commands
{
	public class CoinCommand : ModCommand
	{
		public override CommandType Type => CommandType.Chat;
		public override string Command => "coin";
		public override string Usage => "/coin";
		public override bool Show => false;
		public override bool VerifyArguments(string[] args) => args.Length == 0;
		public override void Action(string[] args) => ExampleUI.visible = true;
	}
}