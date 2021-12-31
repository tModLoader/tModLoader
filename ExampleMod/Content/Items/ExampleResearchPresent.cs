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
        public override bool PreItemResearch(ref CreativeUI.ItemSacrificeResult result)
        {
			/*First, find out how many presents you would still need to research before having infinte of them.*/	
			int toResearch = CreativeUI.GetAmountToResearch(Item, out bool fullyResearched);
			/*If you managed to be lucky enough to get infinite presents, it will give you all accessories*/
			if (fullyResearched || toResearch <= Item.stack) {
				result = CreativeUI.ItemSacrificeResult.SacrificedAndDone;
				for (int i = 1; i < ItemLoader.ItemCount; i++) {
                    if (ContentSamples.ItemsByType[i].accessory) {
						CreativeUI.ResearchItem(i, out bool researched);
					}
                }
				Main.NewText("You got all accessories!");
				/*In the fully researched case, we don't want for the items to go trought the research system again, but we still want to decrease the
				item stack by one*/
				if (fullyResearched) {
					//We always lose a present when researching them, even if you already had infinite of them.
					Item.stack -= 1;
					// We return false to prevent the UI from running the research again
					return false;
                }
				//otherwise, we want it to be researched by the system as normal
				return true;
			} else {
				int count = 0;
				for(int j = Item.stack; j > 0; j--) {
					if (GetRandomAccessoryToLearn()){
						count++;
					}
				}
				if (count == 0){
					Main.NewText("No new accessory...");
				} else {
					Main.NewText("Learned "+ count +" new accessor" + (count == 1? "y": "ies") + " !");
				}
			}
			//We return true here because we want the presents to be added to the existing Sacrificed list when the full case did not occurr.
			return true;
		}

        private bool GetRandomAccessoryToLearn() {
			for (int i = 0; i < 1000; i++) {
				int tp = Main.rand.Next(1, ItemLoader.ItemCount);
				if (ContentSamples.ItemsByType[tp].accessory) {
					CreativeUI.ResearchItem(tp, out bool didResearch);
					if (!didResearch){
						return true;
					}
				}
			}
			return false;
		}
    }
}
