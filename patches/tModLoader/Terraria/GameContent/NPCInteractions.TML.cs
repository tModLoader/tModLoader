using Terraria.Audio;
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
		Register(new Actions.CloseChat(NPCID.Guide));
		Register(new Actions.CloseChat(NPCID.Merchant));
		Register(new Actions.CloseChat(NPCID.Nurse));
		Register(new Actions.CloseChat(NPCID.Demolitionist));
		Register(new Actions.CloseChat(NPCID.DyeTrader));
		Register(new Actions.CloseChat(NPCID.Angler));
		Register(new Actions.CloseChat(NPCID.BestiaryGirl));
		Register(new Actions.CloseChat(NPCID.Dryad));
		Register(new Actions.CloseChat(NPCID.Painter));
		Register(new Actions.CloseChat(NPCID.Golfer));
		Register(new Actions.CloseChat(NPCID.ArmsDealer));
		Register(new Actions.CloseChat(NPCID.DD2Bartender));
		Register(new Actions.CloseChat(NPCID.Stylist));
		Register(new Actions.CloseChat(NPCID.GoblinTinkerer));
		Register(new Actions.CloseChat(NPCID.WitchDoctor));
		Register(new Actions.CloseChat(NPCID.Clothier));
		Register(new Actions.CloseChat(NPCID.Mechanic));
		Register(new Actions.CloseChat(NPCID.PartyGirl));
		Register(new Actions.CloseChat(NPCID.Wizard));
		Register(new Actions.CloseChat(NPCID.TaxCollector));
		Register(new Actions.CloseChat(NPCID.Truffle));
		Register(new Actions.CloseChat(NPCID.Pirate));
		Register(new Actions.CloseChat(NPCID.Steampunker));
		Register(new Actions.CloseChat(NPCID.Cyborg));
		Register(new Actions.CloseChat(NPCID.SantaClaus));
		Register(new Actions.CloseChat(NPCID.Princess));
		Register(new Actions.CloseChat(NPCID.TownCat));
		Register(new Actions.CloseChat(NPCID.TownDog));
		Register(new Actions.CloseChat(NPCID.TownBunny));
		Register(new Actions.CloseChat(NPCID.TownSlimeCopper));
		Register(new Actions.CloseChat(NPCID.TownSlimePurple));
		Register(new Actions.CloseChat(NPCID.TownSlimeBlue));
		Register(new Actions.CloseChat(NPCID.TownSlimeRed));
		Register(new Actions.CloseChat(NPCID.TownSlimeYellow));
		Register(new Actions.CloseChat(NPCID.TownSlimeOld));
		Register(new Actions.CloseChat(NPCID.TownSlimeGreen));
		Register(new Actions.CloseChat(NPCID.TownSlimeRainbow));
		Register(new Actions.CloseChat(NPCID.OldMan));
		Register(new Actions.CloseChat(NPCID.TravellingMerchant));
		Register(new Actions.CloseChat(NPCID.SkeletonMerchant));
	}

	private static void RegisterVanillaHappinessButtons()
	{
		Register(new Actions.ReportHappiness(NPCID.Guide));
		Register(new Actions.ReportHappiness(NPCID.Merchant));
		Register(new Actions.ReportHappiness(NPCID.Nurse));
		Register(new Actions.ReportHappiness(NPCID.Demolitionist));
		Register(new Actions.ReportHappiness(NPCID.DyeTrader));
		Register(new Actions.ReportHappiness(NPCID.Angler));
		Register(new Actions.ReportHappiness(NPCID.BestiaryGirl));
		Register(new Actions.ReportHappiness(NPCID.Dryad));
		Register(new Actions.ReportHappiness(NPCID.Painter));
		Register(new Actions.ReportHappiness(NPCID.Golfer));
		Register(new Actions.ReportHappiness(NPCID.ArmsDealer));
		Register(new Actions.ReportHappiness(NPCID.DD2Bartender));
		Register(new Actions.ReportHappiness(NPCID.Stylist));
		Register(new Actions.ReportHappiness(NPCID.GoblinTinkerer));
		Register(new Actions.ReportHappiness(NPCID.WitchDoctor));
		Register(new Actions.ReportHappiness(NPCID.Clothier));
		Register(new Actions.ReportHappiness(NPCID.Mechanic));
		Register(new Actions.ReportHappiness(NPCID.PartyGirl));
		Register(new Actions.ReportHappiness(NPCID.Wizard));
		Register(new Actions.ReportHappiness(NPCID.TaxCollector));
		Register(new Actions.ReportHappiness(NPCID.Truffle));
		Register(new Actions.ReportHappiness(NPCID.Pirate));
		Register(new Actions.ReportHappiness(NPCID.Steampunker));
		Register(new Actions.ReportHappiness(NPCID.Cyborg));
		Register(new Actions.ReportHappiness(NPCID.SantaClaus));
		Register(new Actions.ReportHappiness(NPCID.Princess));
		// Town Pets, Town Slimes, Old Man, Traveling Merchant, and Skeleton Merchant are excluded.
	}

	private static void RegisterVanillaHousingButtons()
	{
		Register(new Actions.RequestHome(NPCID.Guide));
		Register(new Actions.RequestHome(NPCID.Merchant));
		Register(new Actions.RequestHome(NPCID.Nurse));
		Register(new Actions.RequestHome(NPCID.Demolitionist));
		Register(new Actions.RequestHome(NPCID.DyeTrader));
		Register(new Actions.RequestHome(NPCID.Angler));
		Register(new Actions.RequestHome(NPCID.BestiaryGirl));
		Register(new Actions.RequestHome(NPCID.Dryad));
		Register(new Actions.RequestHome(NPCID.Painter));
		Register(new Actions.RequestHome(NPCID.Golfer));
		Register(new Actions.RequestHome(NPCID.ArmsDealer));
		Register(new Actions.RequestHome(NPCID.DD2Bartender));
		Register(new Actions.RequestHome(NPCID.Stylist));
		Register(new Actions.RequestHome(NPCID.GoblinTinkerer));
		Register(new Actions.RequestHome(NPCID.WitchDoctor));
		Register(new Actions.RequestHome(NPCID.Clothier));
		Register(new Actions.RequestHome(NPCID.Mechanic));
		Register(new Actions.RequestHome(NPCID.PartyGirl));
		Register(new Actions.RequestHome(NPCID.Wizard));
		Register(new Actions.RequestHome(NPCID.TaxCollector));
		Register(new Actions.RequestHome(NPCID.Truffle));
		Register(new Actions.RequestHome(NPCID.Pirate));
		Register(new Actions.RequestHome(NPCID.Steampunker));
		Register(new Actions.RequestHome(NPCID.Cyborg));
		Register(new Actions.RequestHome(NPCID.SantaClaus));
		Register(new Actions.RequestHome(NPCID.Princess));
		Register(new Actions.RequestHome(NPCID.TownCat));
		Register(new Actions.RequestHome(NPCID.TownDog));
		Register(new Actions.RequestHome(NPCID.TownBunny));
		Register(new Actions.RequestHome(NPCID.TownSlimeCopper));
		Register(new Actions.RequestHome(NPCID.TownSlimePurple));
		Register(new Actions.RequestHome(NPCID.TownSlimeBlue));
		Register(new Actions.RequestHome(NPCID.TownSlimeRed));
		Register(new Actions.RequestHome(NPCID.TownSlimeYellow));
		Register(new Actions.RequestHome(NPCID.TownSlimeOld));
		Register(new Actions.RequestHome(NPCID.TownSlimeGreen));
		Register(new Actions.RequestHome(NPCID.TownSlimeRainbow));
		// Old Man, Traveling Merchant, and Skeleton Merchant are excluded.
	}
}