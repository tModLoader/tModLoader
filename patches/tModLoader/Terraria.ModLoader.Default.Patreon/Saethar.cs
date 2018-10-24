using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class Saethar_Head : PatreonItem
	{
		public override string PatreonName => "Saethar";
		public override EquipType PatreonEquipType => EquipType.Head;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(34);
		}
	}
	
	internal class Saethar_Body : PatreonItem
	{
		public override string PatreonName => "Saethar";
		public override EquipType PatreonEquipType => EquipType.Body;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(30, 18);
		}
	}
	
	internal class Saethar_Legs : PatreonItem
	{
		public override string PatreonName => "Saethar";
		public override EquipType PatreonEquipType => EquipType.Legs;
		
		public override void SetDefaults()
		{
			base.SetDefaults();
			item.Size = new Vector2(22, 18);
		}
	}
	
	internal class Saethar_Wings : PatreonItem
	{
		public override string PatreonName => "Saethar";
		public override EquipType PatreonEquipType => EquipType.Wings;
		
		public override void SetDefaults()
		{
			base.SetDefaults();
			item.vanity = false;
			item.width = 24;
			item.height = 8;
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.wingTimeMax = 150;
		}
		
	}
}