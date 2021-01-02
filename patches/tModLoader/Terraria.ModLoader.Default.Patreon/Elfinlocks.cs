using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class Elfinlocks_Head : PatreonItem
	{
		public override string SetName => "Elfinlocks";
		public override string SetSuffix => "'"; 
		public override EquipType ItemEquipType => EquipType.Head;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 34;
			item.height = 22;
		}
	}

	internal class Elfinlocks_Body : PatreonItem
	{
		public override string SetName => "Elfinlocks";
		public override string SetSuffix => "'";
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 42;
			item.height = 24;
		}
	}

	internal class Elfinlocks_Legs : PatreonItem
	{
		public override string SetName => "Elfinlocks";
		public override string SetSuffix => "'";
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}
