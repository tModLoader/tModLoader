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
        public override bool? CanResearch() {
			return true;
        }

        public override void OnResearched(bool fullyResearched) {
			/*If you managed to be lucky enough to get all presents, it will give you all
			 accessories*/
			if (fullyResearched) {
				for (int i = 1; i < ItemLoader.ItemCount; i++) {
                    if (ContentSamples.ItemsByType[i].accessory) {
						CreativeUI.ResearchItem(i, out bool researched);
					}
                }
				Main.NewText("You got all accessories!");
				return;
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
