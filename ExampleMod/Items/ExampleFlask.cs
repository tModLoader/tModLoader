using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Items
{
	public class ExampleFlask : ModItem
	{
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Imbues Melee Weapons with Ethereal Flames\nThis is an example modded flask");
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 32;
            item.useStyle = ItemUseStyleID.EatingUsing;
            item.useAnimation = 15;
            item.useTime = 15;
            item.useTurn = true;
            item.UseSound = SoundID.Item3;
            item.maxStack = 30;
            item.consumable = true;
            item.rare = ItemRarityID.Cyan;
            item.value = Item.buyPrice(gold: 1);
            item.buffType = ModContent.BuffType<Buffs.EtherealFlamesImbue>(); //Applies the Ethereal Flames Imbue Buff
            item.buffTime = 72000; //Applies the Ethereal Flames Imbue Buff for 20 minutes
        }
    }
}
