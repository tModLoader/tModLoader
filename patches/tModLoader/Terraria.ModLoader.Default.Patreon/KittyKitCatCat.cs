namespace Terraria.ModLoader.Default.Patreon
{
	class KittyKitCatCat_Head : PatreonItem
	{
		public override string Texture => "ModLoader/Patreon.KittyKitCatCat_Head";
		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 20;
			item.rare = 9;
			item.vanity = true;
		}
	}

	class KittyKitCatCat_Body : PatreonItem
	{
		public override string Texture => "ModLoader/Patreon.KittyKitCatCat_Body";
		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 20;
			item.rare = 9;
			item.vanity = true;
		}
	}

	class KittyKitCatCat_Legs : PatreonItem
	{
		public override string Texture => "ModLoader/Patreon.KittyKitCatCat_Legs";
		public override void SetDefaults()
		{
			item.width = 18;
			item.height = 14;
			item.rare = 9;
			item.vanity = true;
		}
	}
}
