using Terraria.ModLoader;

namespace ExampleMod.Common.Players
{
	class ExamplePlayer : ModPlayer
	{
		public bool ShowMinionCount;

		public override void ResetEffects() {
			ShowMinionCount = false;
		}

		public override void UpdateEquips() {
			if (Player.accThirdEye)
				ShowMinionCount = true;
		}
	}
}
