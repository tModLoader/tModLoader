using System.Linq;
using Microsoft.Xna.Framework;

namespace Terraria.ModLoader.Default.Patreon
{
	internal class Orian_Head : PatreonItem
	{
		public override string PatreonName => "Orian";
		public override EquipType PatreonEquipType => EquipType.Head;

		public override bool IsVanitySet(int head, int body, int legs)
		{
			return head == mod.GetEquipSlot($"{PatreonName}_{EquipType.Head}", EquipType.Head)
				   && body == mod.GetEquipSlot($"{PatreonName}_{EquipType.Body}", EquipType.Body)
				   && legs == mod.GetEquipSlot($"{PatreonName}_{EquipType.Legs}", EquipType.Legs);
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