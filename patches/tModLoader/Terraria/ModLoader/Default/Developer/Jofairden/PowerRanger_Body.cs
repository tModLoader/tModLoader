using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	[AutoloadEquip(EquipType.BodyLegacy)]
	internal class PowerRanger_Body : AndromedonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();

			Item.Size = new Vector2(34, 22);
		}
	}
}
