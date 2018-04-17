namespace Terraria.ModLoader.Default.Patreon
{
	class litcherally_Head : PatreonItem
	{
		public override string Texture => "ModLoader/Patreon.litcherally_Head";
		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 20;
			item.rare = 9;
			item.vanity = true;
		}
	}

	class litcherally_Body : PatreonItem
	{
		public override string Texture => "ModLoader/Patreon.litcherally_Body";
		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 20;
			item.rare = 9;
			item.vanity = true;
		}
	}

	class litcherally_Legs : PatreonItem
	{
		public override string Texture => "ModLoader/Patreon.litcherally_Legs";
		public override void SetDefaults()
		{
			item.width = 18;
			item.height = 14;
			item.rare = 9;
			item.vanity = true;
		}
	}
}
