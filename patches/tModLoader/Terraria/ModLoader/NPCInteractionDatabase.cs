using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace Terraria.ModLoader;

/// <summary>
/// This holds and registers the <see cref="NPCInteractionList"/> data for each NPC.
/// <br/>The Close, Happiness, Housing, and Pet buttons are already predefined and can be used with <see cref="NPCInteractionDatabase.CloseButton"/>, etc.
/// </summary>
public class NPCInteractionDatabase
{
	/// <summary> Predefined Close button for NPC chat buttons. </summary>
	public static NPCInteraction CloseButton = new NPCInteractions.Actions.CloseChat();
	/// <summary> Predefined Happiness button for NPC chat buttons. </summary>
	public static NPCInteraction HappinessButton = new NPCInteractions.Actions.ReportHappiness();
	/// <summary> Predefined Housing button for NPC chat buttons. </summary>
	public static NPCInteraction HousingButton = new NPCInteractions.Actions.RequestHome();
	/// <summary> Predefined Pet button for NPC chat buttons. </summary>
	public static NPCInteraction PetButton = new NPCInteractions.Actions.PetAnimal();

	// int Key: NPC Type
	private readonly Dictionary<int, NPCInteractionList> _interactionDatabase = new Dictionary<int, NPCInteractionList>();

	/// <summary>
	/// Returns the NPCInteractionList of the NPC.
	/// </summary>
	/// <param name="type">The NPC to get the buttons for.</param>
	/// <returns><see langword="null"/> if not found.</returns>
	public NPCInteractionList GetInteractionList(int type)
	{
		if (_interactionDatabase.TryGetValue(type, out NPCInteractionList value))
			return value;

		return null;
	}

	/// <summary>
	/// Returns the full <c>List&lt;NPCInteractionList.Entry&gt;</c> of the NPC.
	/// </summary>
	/// <param name="type">The NPC to get the buttons for.</param>
	/// <returns><see langword="null"/> if not found.</returns>
	public IReadOnlyList<NPCInteractionList.Entry> GetInteractionEntries(int type)
	{
		if (_interactionDatabase.TryGetValue(type, out NPCInteractionList value))
			return value.Entries;

		return null;
	}

	/// <summary>
	/// Adds the NPC to the database and registers the provided interactions.
	/// </summary>
	/// <param name="type">The NPC to register.</param>
	/// <param name="interactions">List of interactions to pre-register</param>
	private void RegisterNewNPC(int type, params NPCInteraction[] interactions)
	{
		if (!_interactionDatabase.ContainsKey(type)) {
			_interactionDatabase[type] = new NPCInteractionList(type);
			foreach (NPCInteraction interaction in interactions) {
				_interactionDatabase[type].Append(interaction);
			}
		}
		else {
			throw new Exception($"NPCInteractionDatabase NPC type {type} was already registered.");
		}
	}

	internal void Populate()
	{
		RegisterSigns();
		RegisterVanilla();

		foreach (KeyValuePair<int, NPC> pair in ContentSamples.NpcsByNetId) {
			// Only register buttons for NPCs who can be spoken to.
			if (pair.Value.townNPC || (pair.Key > -1 && NPCID.Sets.ActsLikeTownNPC[pair.Key]) || (pair.Value.ModNPC != null && pair.Value.ModNPC.CanChat())) {
				// Only pre-register these buttons for modded NPCs because they are already registered for vanilla NPCs in RegisterVanilla().
				if (pair.Key >= NPCID.Count) {
					RegisterNewNPC(pair.Key, CloseButton);
					if (pair.Key > -1 && NPCID.Sets.IsTownPet[pair.Key]) { // Automatically add the Pet button to Town Pets.
						_interactionDatabase[pair.Key].Append(PetButton);
					}
					_interactionDatabase[pair.Key].Append(HappinessButton);
					_interactionDatabase[pair.Key].Append(HousingButton);
					NPCLoader.RegisterChatButtons(pair.Value, _interactionDatabase[pair.Key]);
				}
				else {
					NPCLoader.RegisterChatButtons(pair.Value, GetInteractionList(pair.Key));
				}
			}
		}
	}

	private void RegisterSigns()
	{
		// 0 was chosen because NPCInteraction.TalkNPCType returns 0 if not speaking to an NPC.
		RegisterNewNPC(0, CloseButton, new NPCInteractions.Actions.OpenSign());
	}

	private void RegisterVanilla()
	{
		RegisterNewNPC(NPCID.Guide,
			CloseButton,
			new NPCInteractions.Actions.GuideTip(),
			new NPCInteractions.Actions.GuideReverseCrafting(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Merchant,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(1)), // Equivalent to NPCInteractions.Shop("Terraria/Merchant/Shop")
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Nurse,
			new NPCInteractions.Actions.NurseHeal(),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Demolitionist,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(4)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.DyeTrader,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(12)),
			CloseButton,
			new NPCInteractions.Actions.DyeTraderRarePlant(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Angler,
			new NPCInteractions.Actions.AnglerQuest(),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.BestiaryGirl,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(23)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Dryad,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(3)),
			CloseButton,
			new NPCInteractions.Actions.StardewValleyBit(),
			new NPCInteractions.Actions.DryadPurification(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Painter,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(15)),
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(25), "GameUI.PainterDecor"), // Equivalent to NPCInteractions.Shop("Terraria/Painter/Decor", "GameUI.PainterDecor")
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Golfer,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(22)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.ArmsDealer,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(2)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.DD2Bartender,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(21)),
			CloseButton,
			new NPCInteractions.Actions.TavernkeepAdvice(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Stylist,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(18)),
			CloseButton,
			new NPCInteractions.Actions.StylistHairWindow(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.GoblinTinkerer,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(6)),
			CloseButton,
			new NPCInteractions.Actions.TinkererReforge(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.WitchDoctor,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(16)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Clothier,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(5)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Mechanic,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(8)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.PartyGirl,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(13)),
			CloseButton,
			new NPCInteractions.Actions.PartyGirlMusicSwap(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Wizard,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(7)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.TaxCollector,
			new NPCInteractions.Actions.TaxCollectorCollectTaxes(),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Truffle,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(10)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Pirate,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(17)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Steampunker,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(11)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Cyborg,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(14)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.SantaClaus,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(9)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Princess,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(24)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.TownCat,
			CloseButton,
			new NPCInteractions.Actions.PetAnimal(),
			HappinessButton, // Register the happiness button even though it'll never show up to match vanilla.
			HousingButton);

		RegisterNewNPC(NPCID.TownDog,
			CloseButton,
			new NPCInteractions.Actions.PetAnimal(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.TownBunny,
			CloseButton,
			new NPCInteractions.Actions.PetAnimal(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.TownSlimeCopper,
			CloseButton,
			new NPCInteractions.Actions.PetAnimal(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.TownSlimePurple,
			CloseButton,
			new NPCInteractions.Actions.PetAnimal(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.TownSlimeBlue,
			CloseButton,
			new NPCInteractions.Actions.PetAnimal(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.TownSlimeRed,
			CloseButton,
			new NPCInteractions.Actions.PetAnimal(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.TownSlimeYellow,
			CloseButton,
			new NPCInteractions.Actions.PetAnimal(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.TownSlimeOld,
			CloseButton,
			new NPCInteractions.Actions.PetAnimal(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.TownSlimeGreen,
			CloseButton,
			new NPCInteractions.Actions.PetAnimal(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.TownSlimeRainbow,
			CloseButton,
			new NPCInteractions.Actions.PetAnimal(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.OldMan,
			CloseButton,
			new NPCInteractions.Actions.OldManCurse(),
			HappinessButton, // Register the happiness and housing buttons even though they'll never show up to match vanilla.
			HousingButton);

		RegisterNewNPC(NPCID.TravellingMerchant,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(19)),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.SkeletonMerchant,
			NPCInteractions.Shop(NPCShopDatabase.GetShopNameFromVanillaIndex(20)),
			CloseButton,
			HappinessButton,
			HousingButton);
	}
}
