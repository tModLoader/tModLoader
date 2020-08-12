using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class PowerRanger_Legs : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void Load() {
			Mod.AddContent<AndromedonLegsGlow>();
			Mod.AddContent<AndromedonLegsShader>();
		}

		public override void SetDefaults() {
			base.SetDefaults();

			item.Size = new Vector2(22, 18);
		}
	}
}
