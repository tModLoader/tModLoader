using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class Glory_Head : PatreonItem
	{
		public override string SetName => "Glory";
		public override EquipType ItemEquipType => EquipType.Head;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(32, 46);
		}
	}
	
	internal class Glory_Body : PatreonItem
	{
		public override string SetName => "Glory";
		public override EquipType ItemEquipType => EquipType.Body;
		
		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(26, 22);
		}
	}
	
	internal class Glory_Legs : PatreonItem
	{
		public override string SetName => "Glory";
		public override EquipType ItemEquipType => EquipType.Legs;
		
		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(20, 12);
		}
	}
}