using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	[AutoloadEquip(EquipType.Legs)]
	internal class PowerRanger_Legs : AndromedonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();

			Item.Size = new Vector2(22, 18);
		}
	}
}
