using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleResearchPresent : ModItem
	{

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Example Present");
			Tooltip.SetDefault("Contains a random accessory! Try researching it for infinite of it!");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = ItemLoader.ItemCount;
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.GoodieBag);
		}

		/*this allows for the present to be researched even when you already have infinite of them*/
        public override bool? CanResearch()
        {
			return true;
        }

        public override void OnResearched(bool fullyResearched)
        {
			/*If you managed to be lucky enough to get all presents, it will give you all
			 accessories*/
			if (fullyResearched)
			{
				for(int i = 1; i < ItemLoader.ItemCount; i++)
                {
					Item tr = new Item(i);
                    if (tr.accessory)
                    {
						CreativeUI.ResearchItem(tr);
					}
                }
				Main.NewText("You got all accessories!");
				return;
			}
			else
			{
				int j = this.Item.stack;
				for (int i = 0 ; i < 1000; i++)
				{
					int tp = Main.rand.Next(1, ItemLoader.ItemCount);
					Item toResearch = new Item(tp);
					if (toResearch.accessory)
					{
						CreativeUI.ResearchItem(toResearch);
						j--;
						if(j <= 0)
							return;
					}
				}
				Main.NewText("No new accessory was inside...");
			}
        }

    }
}
