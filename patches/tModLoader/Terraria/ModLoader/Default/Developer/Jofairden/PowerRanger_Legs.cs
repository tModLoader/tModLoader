using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	[AutoloadEquip(EquipType.Legs)]
	internal class PowerRanger_Legs : AndromedonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();

			item.Size = new Vector2(22, 18);
		}
	}
}
