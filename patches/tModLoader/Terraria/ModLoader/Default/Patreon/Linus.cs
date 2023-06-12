using Microsoft.Xna.Framework;
using ReLogic.Content;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace Terraria.ModLoader.Default.Patreon
{
    [AutoloadEquip(EquipType.Head)]
    internal class Linus_Head : PatreonItem
    {
		public override LocalizedText Tooltip => this.GetLocalization(nameof(Tooltip), () => "");

		public override void SetDefaults() {
            base.SetDefaults();

			Item.width = 34;
			Item.height = 30;
        }
    }

    [AutoloadEquip(EquipType.Body)]
	internal class Linus_Body : PatreonItem
    {
		public override LocalizedText Tooltip => this.GetLocalization(nameof(Tooltip), () => "");

		public override void SetDefaults() {
            base.SetDefaults();

			Item.width = 34;
			Item.height = 24;
        }
    }

    [AutoloadEquip(EquipType.Legs)]
	internal class Linus_Legs : PatreonItem
    {
		public override LocalizedText Tooltip => this.GetLocalization(nameof(Tooltip), () => "");

		public override void SetDefaults() {
            base.SetDefaults();

			Item.width = 22;
			Item.height = 18;
        }
    }

    [AutoloadEquip(EquipType.Wings)]
	internal class Linus_Wings : PatreonItem
    {
		public override LocalizedText Tooltip => this.GetLocalization(nameof(Tooltip), () => "");

		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();

			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(150, 7f);
		}

        public override void SetDefaults() {
            base.SetDefaults();

			Item.vanity = false;
			Item.width = 32;
			Item.height = 32;
			Item.accessory = true;
        }
    }
}
