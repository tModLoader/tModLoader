namespace Terraria.ModLoader.Default.Patreon
{
	class toplayz_Head : PatreonItem
	{
		public override string Texture => "ModLoader/Patreon.toplayz_Head";
		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 20;
			item.rare = 9;
			item.vanity = true;
		}
	}

	class toplayz_Body : PatreonItem
	{
		public override string Texture => "ModLoader/Patreon.toplayz_Body";
		public override void SetDefaults()
		{
			item.width = 28;
			item.height = 20;
			item.rare = 9;
			item.vanity = true;
		}
	}

	class toplayz_Legs : PatreonItem
	{
		public override string Texture => "ModLoader/Patreon.toplayz_Legs";
		public override void SetDefaults()
		{
			item.width = 18;
			item.height = 14;
			item.rare = 9;
			item.vanity = true;
		}
	}
}
