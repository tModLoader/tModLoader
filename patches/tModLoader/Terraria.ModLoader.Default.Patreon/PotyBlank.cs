namespace Terraria.ModLoader.Default.Patreon
{
	class PotyBlank_Head : PatreonItem
	{
		public override string Texture => "ModLoader/Patreon.PotyBlank_Head";
		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 20;
			item.rare = 9;
			item.vanity = true;
		}
	}

	class PotyBlank_Body : PatreonItem
	{
		public override string Texture => "ModLoader/Patreon.PotyBlank_Body";
		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 20;
			item.rare = 9;
			item.vanity = true;
		}
	}

	class PotyBlank_Legs : PatreonItem
	{
		public override string Texture => "ModLoader/Patreon.PotyBlank_Legs";
		public override void SetDefaults()
		{
			item.width = 18;
			item.height = 14;
			item.rare = 9;
			item.vanity = true;
		}
	}
}
