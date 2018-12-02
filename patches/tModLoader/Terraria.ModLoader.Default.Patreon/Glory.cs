using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class Glory_Head : PatreonItem
	{
		public override string SetName => "Glory";
		public override EquipType ItemEquipType => EquipType.Head;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(30, 32);
		}
	}

	internal class Glory_Body : PatreonItem
	{
		public override string SetName => "Glory";
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(34, 24);
		}
	}

	internal class Glory_Legs : PatreonItem
	{
		public override string SetName => "Glory";
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults() {
			base.SetDefaults();
			item.Size = new Vector2(22, 18);
		}
	}
}