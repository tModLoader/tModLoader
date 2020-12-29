using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	[AutoloadEquip(EquipType.Body)]
	internal class PowerRanger_Body : AndromedonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();

			item.Size = new Vector2(34, 22);
		}
	}
}
