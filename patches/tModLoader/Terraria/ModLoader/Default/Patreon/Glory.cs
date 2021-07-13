using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Patreon
{
	[AutoloadEquip(EquipType.Head)]
	internal class Glory_Head : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			Item.Size = new Vector2(30, 32);
		}
	}

	[AutoloadEquip(EquipType.BodyLegacy)]
	internal class Glory_Body : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			Item.Size = new Vector2(34, 24);
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	internal class Glory_Legs : PatreonItem
	{
		public override void SetDefaults() {
			base.SetDefaults();
			Item.Size = new Vector2(22, 18);
		}
	}
}