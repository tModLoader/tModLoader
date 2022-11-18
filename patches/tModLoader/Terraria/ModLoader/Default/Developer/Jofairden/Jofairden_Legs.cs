using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer.Jofairden;

[LegacyName("PowerRanger_Legs")]
[AutoloadEquip(EquipType.Legs)]
internal class Jofairden_Legs : JofairdenArmorItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(22, 18);
	}
}
