using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class AlejandroAkbal_Head : PatreonItem
	{
		public override string SetName => "AlejandroAkbal";
		public override EquipType ItemEquipType => EquipType.Head;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 34;
			item.height = 22;
		}
	}

	// female sprite was pretty much the same and made it difficult to support, so ommitted. 
	internal class AlejandroAkbal_Body : PatreonItem
	{
		public override string SetName => "AlejandroAkbal";
		public override EquipType ItemEquipType => EquipType.Body;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 42;
			item.height = 24;
		}
	}

	internal class AlejandroAkbal_Legs : PatreonItem
	{
		public override string SetName => "AlejandroAkbal";
		public override EquipType ItemEquipType => EquipType.Legs;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}

	internal class AlejandroAkbal_Back : PatreonItem
	{
		public override string SetName => "AlejandroAkbal";
		public override EquipType ItemEquipType => EquipType.Back;

		public override void SetDefaults() {
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
			item.accessory = true;
		}
	}
}
