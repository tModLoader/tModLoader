using Terraria;
using Terraria.ID;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	public class ExampleKeybindPlayer : ModPlayer
	{
		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (ExampleMod.RandomBuffKeybind.JustPressed) {
				int buff = Main.rand.Next(BuffID.Count);
				Player.AddBuff(buff, 600);
				Main.NewText($"ExampleMod's ModKeybind was just pressed. The {Lang.GetBuffName(buff)} buff was given to the player.");
			}
		}
	}
}
