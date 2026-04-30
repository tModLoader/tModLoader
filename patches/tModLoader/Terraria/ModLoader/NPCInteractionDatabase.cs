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
	/// <param name="npcNetId">The NPC to get the buttons for.</param>
	/// <returns><see langword="null"/> if not found.</returns>
	public NPCInteractionList GetInteractionListForNPCID(int npcNetId)
	{
		if (_interactionDatabase.TryGetValue(npcNetId, out NPCInteractionList value))
			return value;

		return null;
	}

	/// <summary>
	/// Returns the full <c>List&lt;NPCInteractionList.Entry&gt;</c> of the NPC.
	/// </summary>
	/// <param name="npcNetId">The NPC to get the buttons for.</param>
	/// <returns><see langword="null"/> if not found.</returns>
	public IReadOnlyList<NPCInteractionList.Entry> GetInteractionEntriesNPCID(int npcNetId)
	{
		if (_interactionDatabase.TryGetValue(npcNetId, out NPCInteractionList value))
			return value.Entries;

		return null;
	}

	/// <summary>
	/// Adds the NPC to the database and registers the provided interactions.
	/// </summary>
	/// <param name="npcNetId">The NPC to register.</param>
	/// <param name="interactions">List of interactions to pre-register</param>
	private void RegisterNewNPC(int npcNetId, params NPCInteraction[] interactions)
	{
		if (!_interactionDatabase.ContainsKey(npcNetId)) {
			_interactionDatabase[npcNetId] = new NPCInteractionList(npcNetId);
			foreach (NPCInteraction interaction in interactions) {
				_interactionDatabase[npcNetId].Append(interaction);
			}
		}
		else {
			throw new Exception($"NPCInteractionDatabase NPC type {npcNetId} was already registered.");
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
					NPCLoader.RegisterChatButtons(pair.Value, GetInteractionListForNPCID(pair.Key));
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
			new NPCInteractions.Actions.OpenShop(1), // Equivalent to NPCInteractions.Shop("Terraria/Merchant/Shop")
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Nurse,
			new NPCInteractions.Actions.NurseHeal(),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Demolitionist,
			new NPCInteractions.Actions.OpenShop(4),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.DyeTrader,
			new NPCInteractions.Actions.OpenShop(12),
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
			new NPCInteractions.Actions.OpenShop(23),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Dryad,
			new NPCInteractions.Actions.OpenShop(3),
			CloseButton,
			new NPCInteractions.Actions.StardewValleyBit(),
			new NPCInteractions.Actions.DryadPurification(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Painter,
			new NPCInteractions.Actions.OpenShop(15),
			new NPCInteractions.Actions.OpenShop(25, "GameUI.PainterDecor"),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Golfer,
			new NPCInteractions.Actions.OpenShop(22),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.ArmsDealer,
			new NPCInteractions.Actions.OpenShop(2),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.DD2Bartender,
			new NPCInteractions.Actions.OpenShop(21),
			CloseButton,
			new NPCInteractions.Actions.TavernkeepAdvice(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Stylist,
			new NPCInteractions.Actions.OpenShop(18),
			CloseButton,
			new NPCInteractions.Actions.StylistHairWindow(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.GoblinTinkerer,
			new NPCInteractions.Actions.OpenShop(6),
			CloseButton,
			new NPCInteractions.Actions.TinkererReforge(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.WitchDoctor,
			new NPCInteractions.Actions.OpenShop(16),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Clothier,
			new NPCInteractions.Actions.OpenShop(5),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Mechanic,
			new NPCInteractions.Actions.OpenShop(8),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.PartyGirl,
			new NPCInteractions.Actions.OpenShop(13),
			CloseButton,
			new NPCInteractions.Actions.PartyGirlMusicSwap(),
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Wizard,
			new NPCInteractions.Actions.OpenShop(7),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.TaxCollector,
			new NPCInteractions.Actions.TaxCollectorCollectTaxes(),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Truffle,
			new NPCInteractions.Actions.OpenShop(10),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Pirate,
			new NPCInteractions.Actions.OpenShop(17),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Steampunker,
			new NPCInteractions.Actions.OpenShop(11),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Cyborg,
			new NPCInteractions.Actions.OpenShop(14),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.SantaClaus,
			new NPCInteractions.Actions.OpenShop(9),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.Princess,
			new NPCInteractions.Actions.OpenShop(24),
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
			new NPCInteractions.Actions.OpenShop(19),
			CloseButton,
			HappinessButton,
			HousingButton);

		RegisterNewNPC(NPCID.SkeletonMerchant,
			new NPCInteractions.Actions.OpenShop(20),
			CloseButton,
			HappinessButton,
			HousingButton);
	}
}
