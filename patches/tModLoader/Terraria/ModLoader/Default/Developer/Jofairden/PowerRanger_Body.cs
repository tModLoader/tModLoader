using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal class PowerRanger_Body : AndromedonItem
	{
		public override EquipType ItemEquipType => EquipType.Body;

		public override void Load() {
			Mod.AddContent<AndromedonBodyGlow>();
			Mod.AddContent<AndromedonBodyShader>();
		}

		public override void SetDefaults() {
			base.SetDefaults();

			item.Size = new Vector2(34, 22);
		}
	}
}
