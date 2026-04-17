using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace Terraria.GameContent;

public static partial class NPCInteractions
{
	public static partial class Actions
	{
		public class OpenShop : NPCInteraction
		{
			private int _shopIndex; // Vanilla shop index
			private string _shopName; // String shop name
			private string _shopFullName; // String full shop name, if set.
			private int _npcType;
			private string _customTextKey;

			public OpenShop(int npcType, int shopIndex, string customTextKey = null)
			{
				_npcType = npcType;
				_shopIndex = shopIndex;
				_customTextKey = customTextKey;
				_shopName = null;
				_shopFullName = null;
			}

			public OpenShop(int npcType, string shopName, string customTextKey = null)
			{
				// Split the shopName by the / and assign _shopName to be the last part of the string.
				// Example: Terraria/Painter/Shop
				//		_shopName = Shop
				//		_shopFullName = Terraria/Painter/Shop
				// Example: Shop
				//		_shopName = Shop
				//		_shopFullName = Shop
				string[] splitName = shopName.Split('/');
				_npcType = npcType;
				_shopIndex = -1;
				_customTextKey = customTextKey;
				_shopName = splitName[^1];
				_shopFullName = shopName;
			}

			public override bool Condition() => base.TalkNPCType == _npcType;

			public override string GetText()
			{
				if (_customTextKey != null)
					return Language.GetTextValue(_customTextKey);

				return Lang.inter[28].Value;
			}

			public override void Interact()
			{
				// If vanilla shop, open by ID.
				if (_shopIndex != -1) {
					Main.instance.OpenShop(NPCShopDatabase.GetShopNameFromVanillaIndex(_shopIndex));
				}
				// If the full name was entered, use that.
				else if (_shopFullName != _shopName) {
					Main.instance.OpenShop(_shopFullName);
				}
				// Else, get the full name.
				else if (_shopName != null) {
					Main.instance.OpenShop(NPCShopDatabase.GetShopName(LocalPlayer.TalkNPC.type, _shopName));
				}
			}
		}
	}

	/// <summary>
	/// Assigns a shop chat button to the npcType.
	/// </summary>
	/// <param name="npcType">The NPC type to assign the button to. For your <see cref="ModNPC"/>, simply use <c>Type</c> or <see cref="NPC.type"/></param>
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
	public static void Shop(int npcType, string shopName = "Shop", string customTextKey = null)
	{
		Register(new Actions.OpenShop(npcType, shopName, customTextKey));
	}

	/// <summary>
	/// A helper that registers a "Close" button to the npcType.
	/// <br/>Short for <c>NPCInteractions.Register(new NPCInteractions.Actions.CloseChat(Type))</c>
	/// </summary>
	/// <param name="npcType">The NPC type to assign the button to. For your <see cref="ModNPC"/>, simply use <c>Type</c> or <see cref="NPC.type"/></param>
	public static void CloseChat(int npcType)
	{
		Register(new Actions.CloseChat(npcType));
	}
	/// <summary>
	/// A helper that registers a "Happiness" button to the npcType.
	/// <br/>Short for <c>NPCInteractions.Register(new NPCInteractions.Actions.ReportHappiness(Type))</c> 
	/// </summary>
	/// <param name="npcType">The NPC type to assign the button to. For your <see cref="ModNPC"/>, simply use <c>Type</c> or <see cref="NPC.type"/></param>
	public static void ReportHappiness(int npcType)
	{
		Register(new Actions.ReportHappiness(npcType));
	}
	/// <summary>
	/// A helper that registers a "Housing" button to the npcType.
	/// <br/>Short for <c>NPCInteractions.Register(new NPCInteractions.Actions.RequestHome(Type))</c> 
	/// </summary>
	/// <param name="npcType">The NPC type to assign the button to. For your <see cref="ModNPC"/>, simply use <c>Type</c> or <see cref="NPC.type"/></param>
	public static void RequestHome(int npcType)
	{
		Register(new Actions.RequestHome(npcType));
	}

	public static void InitializeVanillaGlobal()
	{
		for (int i = 0; i < NPCID.Count; i++) {
			bool skipCloseChat = true;
			bool skipReportHappiness = true;
			bool skipRequestHome = true;
			NPCLoader.SetChatButtons(i, ref skipCloseChat, ref skipReportHappiness, ref skipRequestHome);
		}
	}

	public static void InitializeModded()
	{
		for (int i = NPCID.Count; i < NPCLoader.NPCCount; i++) {
			bool skipCloseChat = false;
			bool skipReportHappiness = false;
			bool skipRequestHome = false;
			NPCLoader.SetChatButtons(i, ref skipCloseChat, ref skipReportHappiness, ref skipRequestHome);

			if (!skipCloseChat)
				Register(new Actions.CloseChat(i));
			if (!skipReportHappiness)
				Register(new Actions.ReportHappiness(i));
			if (!skipRequestHome)
				Register(new Actions.RequestHome(i));
		}
	}

	public static void Unload()
	{
		All.Clear();
		Initialize();
	}

	private static void RegisterVanillaCloseButtons()
	{
		CloseChat(NPCID.Guide);
		CloseChat(NPCID.Merchant);
		CloseChat(NPCID.Nurse);
		CloseChat(NPCID.Demolitionist);
		CloseChat(NPCID.DyeTrader);
		CloseChat(NPCID.Angler);
		CloseChat(NPCID.BestiaryGirl);
		CloseChat(NPCID.Dryad);
		CloseChat(NPCID.Painter);
		CloseChat(NPCID.Golfer);
		CloseChat(NPCID.ArmsDealer);
		CloseChat(NPCID.DD2Bartender);
		CloseChat(NPCID.Stylist);
		CloseChat(NPCID.GoblinTinkerer);
		CloseChat(NPCID.WitchDoctor);
		CloseChat(NPCID.Clothier);
		CloseChat(NPCID.Mechanic);
		CloseChat(NPCID.PartyGirl);
		CloseChat(NPCID.Wizard);
		CloseChat(NPCID.TaxCollector);
		CloseChat(NPCID.Truffle);
		CloseChat(NPCID.Pirate);
		CloseChat(NPCID.Steampunker);
		CloseChat(NPCID.Cyborg);
		CloseChat(NPCID.SantaClaus);
		CloseChat(NPCID.Princess);
		CloseChat(NPCID.TownCat);
		CloseChat(NPCID.TownDog);
		CloseChat(NPCID.TownBunny);
		CloseChat(NPCID.TownSlimeCopper);
		CloseChat(NPCID.TownSlimePurple);
		CloseChat(NPCID.TownSlimeBlue);
		CloseChat(NPCID.TownSlimeRed);
		CloseChat(NPCID.TownSlimeYellow);
		CloseChat(NPCID.TownSlimeOld);
		CloseChat(NPCID.TownSlimeGreen);
		CloseChat(NPCID.TownSlimeRainbow);
		CloseChat(NPCID.OldMan);
		CloseChat(NPCID.TravellingMerchant);
		CloseChat(NPCID.SkeletonMerchant);
	}

	private static void RegisterVanillaHappinessButtons()
	{
		ReportHappiness(NPCID.Guide);
		ReportHappiness(NPCID.Merchant);
		ReportHappiness(NPCID.Nurse);
		ReportHappiness(NPCID.Demolitionist);
		ReportHappiness(NPCID.DyeTrader);
		ReportHappiness(NPCID.Angler);
		ReportHappiness(NPCID.BestiaryGirl);
		ReportHappiness(NPCID.Dryad);
		ReportHappiness(NPCID.Painter);
		ReportHappiness(NPCID.Golfer);
		ReportHappiness(NPCID.ArmsDealer);
		ReportHappiness(NPCID.DD2Bartender);
		ReportHappiness(NPCID.Stylist);
		ReportHappiness(NPCID.GoblinTinkerer);
		ReportHappiness(NPCID.WitchDoctor);
		ReportHappiness(NPCID.Clothier);
		ReportHappiness(NPCID.Mechanic);
		ReportHappiness(NPCID.PartyGirl);
		ReportHappiness(NPCID.Wizard);
		ReportHappiness(NPCID.TaxCollector);
		ReportHappiness(NPCID.Truffle);
		ReportHappiness(NPCID.Pirate);
		ReportHappiness(NPCID.Steampunker);
		ReportHappiness(NPCID.Cyborg);
		ReportHappiness(NPCID.SantaClaus);
		ReportHappiness(NPCID.Princess);
		// Town Pets, Town Slimes, Old Man, Traveling Merchant, and Skeleton Merchant are excluded.
	}

	private static void RegisterVanillaHousingButtons()
	{
		RequestHome(NPCID.Guide);
		RequestHome(NPCID.Merchant);
		RequestHome(NPCID.Nurse);
		RequestHome(NPCID.Demolitionist);
		RequestHome(NPCID.DyeTrader);
		RequestHome(NPCID.Angler);
		RequestHome(NPCID.BestiaryGirl);
		RequestHome(NPCID.Dryad);
		RequestHome(NPCID.Painter);
		RequestHome(NPCID.Golfer);
		RequestHome(NPCID.ArmsDealer);
		RequestHome(NPCID.DD2Bartender);
		RequestHome(NPCID.Stylist);
		RequestHome(NPCID.GoblinTinkerer);
		RequestHome(NPCID.WitchDoctor);
		RequestHome(NPCID.Clothier);
		RequestHome(NPCID.Mechanic);
		RequestHome(NPCID.PartyGirl);
		RequestHome(NPCID.Wizard);
		RequestHome(NPCID.TaxCollector);
		RequestHome(NPCID.Truffle);
		RequestHome(NPCID.Pirate);
		RequestHome(NPCID.Steampunker);
		RequestHome(NPCID.Cyborg);
		RequestHome(NPCID.SantaClaus);
		RequestHome(NPCID.Princess);
		RequestHome(NPCID.TownCat);
		RequestHome(NPCID.TownDog);
		RequestHome(NPCID.TownBunny);
		RequestHome(NPCID.TownSlimeCopper);
		RequestHome(NPCID.TownSlimePurple);
		RequestHome(NPCID.TownSlimeBlue);
		RequestHome(NPCID.TownSlimeRed);
		RequestHome(NPCID.TownSlimeYellow);
		RequestHome(NPCID.TownSlimeOld);
		RequestHome(NPCID.TownSlimeGreen);
		RequestHome(NPCID.TownSlimeRainbow);
		// Old Man, Traveling Merchant, and Skeleton Merchant are excluded.
	}
}