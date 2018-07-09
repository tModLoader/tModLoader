using System.Linq;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class Orian_Head : PatreonItem
	{
		public override string PatreonName => "Orian";
		public override EquipType PatreonEquipType => EquipType.Head;

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.modItem?.GetType() == typeof(Orian_Head)
			       && body.modItem?.GetType() == typeof(Orian_Body)
			       && legs.modItem?.GetType() == typeof(Orian_Legs);
		}

		public override void UpdateArmorSet(Player player)
		{
			PatronModPlayer.Player(player).OrianSet = true;
		}

		public override void UpdateVanitySet(Player player)
		{
			PatronModPlayer.Player(player).OrianSet = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 24;
			item.height = 24;
		}
	}

	internal class Orian_Body : PatreonItem
	{
		public override string PatreonName => "Orian";
		public override EquipType PatreonEquipType => EquipType.Body;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 30;
			item.height = 20;
		}
	}

	internal class Orian_Legs : PatreonItem
	{
		public override string PatreonName => "Orian";
		public override EquipType PatreonEquipType => EquipType.Legs;

		public override void SetDefaults()
		{
			base.SetDefaults();
			item.width = 22;
			item.height = 18;
		}
	}
}