using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Terraria.ID;

namespace Terraria.ModLoader.Default.Patreon;

[AutoloadEquip(EquipType.Head)]
internal class Tantamount_Head : PatreonItem
{
	public override void OnCreated(ItemCreationContext context)
	{
		base.OnCreated(context);

		if (context is InitializationItemCreationContext) {
			// Use the _Head texture for the accessories' Face equip slot.
			EquipLoader.AddEquipTexture(Mod, $"{Texture}_Head", EquipType.Face, item: this);
		}
	}

	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ArmorIDs.Head.Sets.DrawFullHair[Item.headSlot] = true;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.accessory = true;
		Item.Size = new Vector2(26);
	}
}

[AutoloadEquip(EquipType.Body)]
internal class Tantamount_Body : PatreonItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ArmorIDs.Body.Sets.HidesArms[Item.bodySlot] = true;
		ArmorIDs.Body.Sets.HidesHands[Item.bodySlot] = false;
		ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = false;
		ArmorIDs.Body.Sets.shouldersAreAlwaysInTheBack[Item.bodySlot] = true;
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.Size = new Vector2(26, 24);
	}
}

[AutoloadEquip(EquipType.Legs)]
internal class Tantamount_Legs : PatreonItem
{
	public override void SetDefaults()
	{
		base.SetDefaults();
		Item.Size = new Vector2(22, 18);
	}
}

[AutoloadEquip(EquipType.Wings)]
internal class Tantamount_Wings : PatreonItem
{
	public override void SetStaticDefaults()
	{
		base.SetStaticDefaults();

		ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(150, 7f);
	}

	public override void SetDefaults()
	{
		base.SetDefaults();

		Item.vanity = false;
		Item.Size = new Vector2(24, 26);
		Item.accessory = true;
	}
}
