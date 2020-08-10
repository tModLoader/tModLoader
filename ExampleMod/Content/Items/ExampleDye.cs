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
			//Avoid loading assets on dedicated servers. They don't use graphics cards.
			if (!Main.dedServ) {
				//The following code creates an effect (shader) reference and associates it with this item's type Id.
				GameShaders.Armor.BindShader(
					item.type,
					new ArmorShaderData(new Ref<Effect>(Mod.GetEffect("Assets/Effects/ExampleEffect").Value), "ExampleDyePass") //Be sure to update the effect path and pass name here.
				);
			}
		}

		public override void SetDefaults() {
			// item.dye will already be assigned to this item prior to SetDefaults because of the above GameShaders.Armor.BindShader code in Load(). 
			// This code here remembers item.dye so that information isn't lost during CloneDefaults.
			byte dye = item.dye;

			item.CloneDefaults(ItemID.GelDye); // Makes the item copy the attributes of the item "Gel Dye" Change "GelDye" to whatever dye type you want.

			item.dye = dye;
		}
	}
}
