using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items
{
	public class ExampleDye : ModItem
	{
		public override void SetStaticDefaults() {
			// Avoid loading assets on dedicated servers. They don't use graphics cards.
			if (!Main.dedServ) {
				// The following code creates an effect (shader) reference and associates it with this item's type Id.
				GameShaders.Armor.BindShader(
					Item.type,
					new ArmorShaderData(Mod.Assets.Request<Effect>("Assets/Effects/ExampleEffect"), "ExampleDyePass") // Be sure to update the effect path and pass name here.
				);
			}

			Item.ResearchUnlockCount = 3;
		}

		public override void SetDefaults() {
			// Item.dye will already be assigned to this item prior to SetDefaults because of the above GameShaders.Armor.BindShader code in Load().
			// This code here remembers Item.dye so that information isn't lost during CloneDefaults.
			int dye = Item.dye;

			Item.CloneDefaults(ItemID.GelDye); // Makes the item copy the attributes of the item "Gel Dye" Change "GelDye" to whatever dye type you want.

			Item.dye = dye;
		}
	}
}
