using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Terraria.GameContent;

public static partial class NPCInteractions
{
	public static partial class Actions
	{

	}

	/// <summary>
	/// Assigns a shop chat button to the npcType.
	/// </summary>
	/// <param name="shopName">The name of the shop to open.
	/// <br/>This string will need to match the string used to create the <see cref="NPCShop"/> in <see cref="ModNPC.AddShops"/>.
	/// <br/>Defaults to "Shop"
	/// <para>The full name of the shop can be used instead to assign a shop from a different NPC to this NPC.
	/// <br/>Example: <c>"Terraria/Merchant/Shop"</c> will open the Merchant's shop.
	/// <br/>Example: <c>"ExampleMod/ExamplePerson/Shop"</c> will open the Example Person's shop from Example Mod.
	/// <br/>Note: if the shop is not found, the button will open an empty shop.
	/// </para></param>
	/// <param name="customTextKey">The localization key for the display name of the button.
	/// <br/>If not set, the button will be translated to say "Shop". </param>
	public static NPCInteraction Shop(string shopName = "Shop", string customTextKey = null)
	{
		return new Actions.OpenShop(shopName, customTextKey);
	}
}
