using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class Squid_Head : PatreonItem
	{
		public override string PatreonName => "Squid";
		public override EquipType PatreonEquipType => EquipType.Head;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(26);
		}
	}
	
	internal class Squid_Body : PatreonItem
	{
		public override string PatreonName => "Squid";
		public override EquipType PatreonEquipType => EquipType.Body;
		
		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(34, 26);
		}
	}
	
	internal class Squid_Legs : PatreonItem
	{
		public override string PatreonName => "Squid";
		public override EquipType PatreonEquipType => EquipType.Legs;
		
		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(22, 18);
		}
	}
}