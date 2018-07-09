using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class Remeus_Head : PatreonItem
	{
		public override string PatreonName => "Remeus";
		public override EquipType PatreonEquipType => EquipType.Head;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(34);
		}
	}
	
	internal class Remeus_Body : PatreonItem
	{
		public override string PatreonName => "Remeus";
		public override EquipType PatreonEquipType => EquipType.Body;
		
		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(30, 18);
		}
	}
	
	internal class Remeus_Legs : PatreonItem
	{
		public override string PatreonName => "Remeus";
		public override EquipType PatreonEquipType => EquipType.Legs;
		
		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(22, 18);
		}
	}
}