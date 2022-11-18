using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Developer.Jofairden;

[LegacyName("PowerRanger_Body")]
[AutoloadEquip(EquipType.Body)]
internal class Jofairden_Body : JofairdenArmorItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(34, 22);
	}
}
