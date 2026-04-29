using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace Terraria.ModLoader;

public class NPCInteractionDatabase
{
	private readonly Dictionary<int, List<NPCInteraction>> _interactionDatabase = new Dictionary<int, List<NPCInteraction>>();

	/// <summary>
	/// Returns the full <c>List&lt;NPCInteraction&gt;</c> of the NPC.
	/// </summary>
	/// <param name="npcNetId">The NPC to get the buttons for.</param>
	/// <returns>An empty list if not found.</returns>
	public List<NPCInteraction> GetInteractionsForNPCID(int npcNetId)
	{
		List<NPCInteraction> list = new List<NPCInteraction>();
		if (_interactionDatabase.TryGetValue(npcNetId, out List<NPCInteraction> value))
			list.AddRange(value);

		return list;
	}

	/// <inheritdoc cref="NPCInteractionList.FindInteractionByType(Type, out int)"/>
	public NPCInteraction FindInteractionByType(int npcNetId, Type interaction, out int index)
	{
		index = -1;
		if (_interactionDatabase.TryGetValue(npcNetId, out List<NPCInteraction> value)) {
			foreach (NPCInteraction item in value) {
				if (item.GetType() == interaction) { // Class types are the same.
					index = value.IndexOf(item);
					return item;
				}
			}
		}
		return null;
	}

	/// <inheritdoc cref="NPCInteractionList.FindInteractionByInstance(NPCInteraction, out int)"/>
	public NPCInteraction FindInteractionByInstance(int npcNetId, NPCInteraction interaction, out int index)
	{
		index = -1;
		if (_interactionDatabase.TryGetValue(npcNetId, out List<NPCInteraction> value)) {
			foreach (NPCInteraction item in value) {
				if (item.Equals(interaction)) { // Instances are the same.
					index = value.IndexOf(item);
					return item;
				}
			}
		}
		return null;
	}

	/// <inheritdoc cref="NPCInteractionList.InsertAfter(NPCInteraction, NPCInteraction)"/>
	public void RegisterAfter(int npcNetId, NPCInteraction interactionToRegister, NPCInteraction interactionAfter)
	{
		if (!_interactionDatabase.ContainsKey(npcNetId))
			_interactionDatabase[npcNetId] = new List<NPCInteraction>();

		int index = _interactionDatabase[npcNetId].IndexOf(interactionAfter);

		if (index is not -1) {
			_interactionDatabase[npcNetId].Insert(index + 1, interactionToRegister);
		}
		else { // If the interactionAfter is not found, add to the end of the list.
			_interactionDatabase[npcNetId].Add(interactionToRegister);
		}
	}

	/// <inheritdoc cref="NPCInteractionList.InsertBefore(NPCInteraction, NPCInteraction)"/>
	public void RegisterBefore(int npcNetId, NPCInteraction interactionToRegister, NPCInteraction interactionBefore)
	{
		if (!_interactionDatabase.ContainsKey(npcNetId))
			_interactionDatabase[npcNetId] = new List<NPCInteraction>();

		int index = _interactionDatabase[npcNetId].IndexOf(interactionBefore);

		if (index is not -1) {
			_interactionDatabase[npcNetId].Insert(index, interactionToRegister);
		}
		else { // If the interactionAfter is not found, add to the end of the list.
			_interactionDatabase[npcNetId].Add(interactionToRegister);
		}
	}

	/// <inheritdoc cref="NPCInteractionList.InsertAt(NPCInteraction, int)"/>
	public void RegisterAt(int npcNetId, NPCInteraction interactionToAdd, int index)
	{
		if (!_interactionDatabase.ContainsKey(npcNetId))
			_interactionDatabase[npcNetId] = new List<NPCInteraction>();

		index = Math.Clamp(index, 0, _interactionDatabase[npcNetId].Count);
		_interactionDatabase[npcNetId].Insert(index, interactionToAdd);
	}

	/// <inheritdoc cref="NPCInteractionList.Append(NPCInteraction)"/>
	public void RegisterAppend(int npcNetId, NPCInteraction interaction)
	{
		if (!_interactionDatabase.ContainsKey(npcNetId))
			_interactionDatabase[npcNetId] = new List<NPCInteraction>();

		_interactionDatabase[npcNetId].Add(interaction);
		//return interaction;
	}

	/// <inheritdoc cref="NPCInteractionList.Remove(NPCInteraction)"/>
	public bool RemoveFromNPCNetId(int npcNetId, NPCInteraction interaction)
	{
		if (_interactionDatabase.TryGetValue(npcNetId, out List<NPCInteraction> value)) {
			return value.Remove(interaction);
		}
		return false;
	}

	public void Populate()
	{
		// Use the same instance for all Town NPCs
		NPCInteraction closeButton = new NPCInteractions.Actions.CloseChat();
		NPCInteraction happinessButton =  new NPCInteractions.Actions.ReportHappiness();
		NPCInteraction housingButton = new NPCInteractions.Actions.RequestHome();
		RegisterSigns(closeButton);
		RegisterVanilla(closeButton, happinessButton, housingButton);

		foreach (KeyValuePair<int, NPC> pair in ContentSamples.NpcsByNetId) {
			// Only register buttons for NPCs who can be spoken to.
			if (pair.Value.townNPC || (pair.Key > -1 && NPCID.Sets.ActsLikeTownNPC[pair.Key]) || (pair.Value.ModNPC != null && pair.Value.ModNPC.CanChat())) {
				// Only pre-register these buttons for modded NPCs because they are already registered for vanilla NPCs in RegisterVanilla().
				if (pair.Key >= NPCID.Count) {
					RegisterAppend(pair.Key, closeButton);
					if (pair.Key > -1 && NPCID.Sets.IsTownPet[pair.Key]) { // Automatically add the Pet button to Town Pets.
						RegisterAppend(pair.Key, new NPCInteractions.Actions.PetAnimal());
					}
					RegisterAppend(pair.Key, happinessButton);
					RegisterAppend(pair.Key, housingButton);
				}
				NPCLoader.RegisterChatButtons(pair.Value, new NPCInteractionList(pair.Key, this), closeButton, happinessButton, housingButton);
			}
		}
	}

	private void RegisterSigns(NPCInteraction closeButton)
	{
		// 0 was chosen because NPCInteraction.TalkNPCType returns 0 if not speaking to an NPC.
		RegisterAppend(0, closeButton);
		RegisterAppend(0, new NPCInteractions.Actions.OpenSign());
	}

	public void RegisterVanilla(NPCInteraction closeButton, NPCInteraction happinessButton, NPCInteraction housingButton)
	{
		RegisterAppend(NPCID.Guide, closeButton);
		RegisterAppend(NPCID.Guide, new NPCInteractions.Actions.GuideTip());
		RegisterAppend(NPCID.Guide, new NPCInteractions.Actions.GuideReverseCrafting());
		RegisterAppend(NPCID.Guide, happinessButton);
		RegisterAppend(NPCID.Guide, housingButton);

		RegisterAppend(NPCID.Merchant, new NPCInteractions.Actions.OpenShop(1)); // Equivalent to NPCInteractions.Shop("Terraria/Merchant/Shop")
		RegisterAppend(NPCID.Merchant, closeButton);
		RegisterAppend(NPCID.Merchant, happinessButton);
		RegisterAppend(NPCID.Merchant, housingButton);

		RegisterAppend(NPCID.Nurse, new NPCInteractions.Actions.NurseHeal());
		RegisterAppend(NPCID.Nurse, closeButton);
		RegisterAppend(NPCID.Nurse, happinessButton);
		RegisterAppend(NPCID.Nurse, housingButton);

		RegisterAppend(NPCID.Demolitionist, new NPCInteractions.Actions.OpenShop(4));
		RegisterAppend(NPCID.Demolitionist, closeButton);
		RegisterAppend(NPCID.Demolitionist, happinessButton);
		RegisterAppend(NPCID.Demolitionist, housingButton);

		RegisterAppend(NPCID.DyeTrader, new NPCInteractions.Actions.OpenShop(12));
		RegisterAppend(NPCID.DyeTrader, closeButton);
		RegisterAppend(NPCID.DyeTrader, new NPCInteractions.Actions.DyeTraderRarePlant());
		RegisterAppend(NPCID.DyeTrader, happinessButton);
		RegisterAppend(NPCID.DyeTrader, housingButton);

		RegisterAppend(NPCID.Angler, new NPCInteractions.Actions.AnglerQuest());
		RegisterAppend(NPCID.Angler, closeButton);
		RegisterAppend(NPCID.Angler, happinessButton);
		RegisterAppend(NPCID.Angler, housingButton);

		RegisterAppend(NPCID.BestiaryGirl, new NPCInteractions.Actions.OpenShop(23));
		RegisterAppend(NPCID.BestiaryGirl, closeButton);
		RegisterAppend(NPCID.BestiaryGirl, happinessButton);
		RegisterAppend(NPCID.BestiaryGirl, housingButton);

		RegisterAppend(NPCID.Dryad, new NPCInteractions.Actions.OpenShop(3));
		RegisterAppend(NPCID.Dryad, closeButton);
		RegisterAppend(NPCID.Dryad, new NPCInteractions.Actions.StardewValleyBit());
		RegisterAppend(NPCID.Dryad, new NPCInteractions.Actions.DryadPurification());
		RegisterAppend(NPCID.Dryad, happinessButton);
		RegisterAppend(NPCID.Dryad, housingButton);

		RegisterAppend(NPCID.Painter, new NPCInteractions.Actions.OpenShop(15));
		RegisterAppend(NPCID.Painter, new NPCInteractions.Actions.OpenShop(25, "GameUI.PainterDecor"));
		RegisterAppend(NPCID.Painter, closeButton);
		RegisterAppend(NPCID.Painter, happinessButton);
		RegisterAppend(NPCID.Painter, housingButton);

		RegisterAppend(NPCID.Golfer, new NPCInteractions.Actions.OpenShop(22));
		RegisterAppend(NPCID.Golfer, closeButton);
		RegisterAppend(NPCID.Golfer, happinessButton);
		RegisterAppend(NPCID.Golfer, housingButton);

		RegisterAppend(NPCID.ArmsDealer, new NPCInteractions.Actions.OpenShop(2));
		RegisterAppend(NPCID.ArmsDealer, closeButton);
		RegisterAppend(NPCID.ArmsDealer, happinessButton);
		RegisterAppend(NPCID.ArmsDealer, housingButton);

		RegisterAppend(NPCID.DD2Bartender, new NPCInteractions.Actions.OpenShop(21));
		RegisterAppend(NPCID.DD2Bartender, closeButton);
		RegisterAppend(NPCID.DD2Bartender, new NPCInteractions.Actions.TavernkeepAdvice());
		RegisterAppend(NPCID.DD2Bartender, happinessButton);
		RegisterAppend(NPCID.DD2Bartender, housingButton);

		RegisterAppend(NPCID.Stylist, new NPCInteractions.Actions.OpenShop(18));
		RegisterAppend(NPCID.Stylist, closeButton);
		RegisterAppend(NPCID.Stylist, new NPCInteractions.Actions.StylistHairWindow());
		RegisterAppend(NPCID.Stylist, happinessButton);
		RegisterAppend(NPCID.Stylist, housingButton);

		RegisterAppend(NPCID.GoblinTinkerer, new NPCInteractions.Actions.OpenShop(6));
		RegisterAppend(NPCID.GoblinTinkerer, closeButton);
		RegisterAppend(NPCID.GoblinTinkerer, new NPCInteractions.Actions.TinkererReforge());
		RegisterAppend(NPCID.GoblinTinkerer, happinessButton);
		RegisterAppend(NPCID.GoblinTinkerer, housingButton);

		RegisterAppend(NPCID.WitchDoctor, new NPCInteractions.Actions.OpenShop(16));
		RegisterAppend(NPCID.WitchDoctor, closeButton);
		RegisterAppend(NPCID.WitchDoctor, happinessButton);
		RegisterAppend(NPCID.WitchDoctor, housingButton);

		RegisterAppend(NPCID.Clothier, new NPCInteractions.Actions.OpenShop(5));
		RegisterAppend(NPCID.Clothier, closeButton);
		RegisterAppend(NPCID.Clothier, happinessButton);
		RegisterAppend(NPCID.Clothier, housingButton);

		RegisterAppend(NPCID.Mechanic, new NPCInteractions.Actions.OpenShop(8));
		RegisterAppend(NPCID.Mechanic, closeButton);
		RegisterAppend(NPCID.Mechanic, happinessButton);
		RegisterAppend(NPCID.Mechanic, housingButton);

		RegisterAppend(NPCID.PartyGirl, new NPCInteractions.Actions.OpenShop(13));
		RegisterAppend(NPCID.PartyGirl, closeButton);
		RegisterAppend(NPCID.PartyGirl, new NPCInteractions.Actions.PartyGirlMusicSwap());
		RegisterAppend(NPCID.PartyGirl, happinessButton);
		RegisterAppend(NPCID.PartyGirl, housingButton);

		RegisterAppend(NPCID.Wizard, new NPCInteractions.Actions.OpenShop(7));
		RegisterAppend(NPCID.Wizard, closeButton);
		RegisterAppend(NPCID.Wizard, happinessButton);
		RegisterAppend(NPCID.Wizard, housingButton);

		RegisterAppend(NPCID.TaxCollector, new NPCInteractions.Actions.TaxCollectorCollectTaxes());
		RegisterAppend(NPCID.TaxCollector, closeButton);
		RegisterAppend(NPCID.TaxCollector, happinessButton);
		RegisterAppend(NPCID.TaxCollector, housingButton);

		RegisterAppend(NPCID.Truffle, new NPCInteractions.Actions.OpenShop(10));
		RegisterAppend(NPCID.Truffle, closeButton);
		RegisterAppend(NPCID.Truffle, happinessButton);
		RegisterAppend(NPCID.Truffle, housingButton);

		RegisterAppend(NPCID.Pirate, new NPCInteractions.Actions.OpenShop(17));
		RegisterAppend(NPCID.Pirate, closeButton);
		RegisterAppend(NPCID.Pirate, happinessButton);
		RegisterAppend(NPCID.Pirate, housingButton);

		RegisterAppend(NPCID.Steampunker, new NPCInteractions.Actions.OpenShop(11));
		RegisterAppend(NPCID.Steampunker, closeButton);
		RegisterAppend(NPCID.Steampunker, happinessButton);
		RegisterAppend(NPCID.Steampunker, housingButton);

		RegisterAppend(NPCID.Cyborg, new NPCInteractions.Actions.OpenShop(14));
		RegisterAppend(NPCID.Cyborg, closeButton);
		RegisterAppend(NPCID.Cyborg, happinessButton);
		RegisterAppend(NPCID.Cyborg, housingButton);

		RegisterAppend(NPCID.SantaClaus, new NPCInteractions.Actions.OpenShop(9));
		RegisterAppend(NPCID.SantaClaus, closeButton);
		RegisterAppend(NPCID.SantaClaus, happinessButton);
		RegisterAppend(NPCID.SantaClaus, housingButton);

		RegisterAppend(NPCID.Princess, new NPCInteractions.Actions.OpenShop(24));
		RegisterAppend(NPCID.Princess, closeButton);
		RegisterAppend(NPCID.Princess, happinessButton);
		RegisterAppend(NPCID.Princess, housingButton);

		RegisterAppend(NPCID.TownCat, closeButton);
		RegisterAppend(NPCID.TownCat, new NPCInteractions.Actions.PetAnimal());
		RegisterAppend(NPCID.TownCat, happinessButton); // Register the happiness button even though it'll never show up to match vanilla.
		RegisterAppend(NPCID.TownCat, housingButton);

		RegisterAppend(NPCID.TownDog, closeButton);
		RegisterAppend(NPCID.TownDog, new NPCInteractions.Actions.PetAnimal());
		RegisterAppend(NPCID.TownDog, happinessButton);
		RegisterAppend(NPCID.TownDog, housingButton);

		RegisterAppend(NPCID.TownBunny, closeButton);
		RegisterAppend(NPCID.TownBunny, new NPCInteractions.Actions.PetAnimal());
		RegisterAppend(NPCID.TownBunny, happinessButton);
		RegisterAppend(NPCID.TownBunny, housingButton);

		RegisterAppend(NPCID.TownSlimeCopper, closeButton);
		RegisterAppend(NPCID.TownSlimeCopper, new NPCInteractions.Actions.PetAnimal());
		RegisterAppend(NPCID.TownSlimeCopper, happinessButton);
		RegisterAppend(NPCID.TownSlimeCopper, housingButton);

		RegisterAppend(NPCID.TownSlimePurple, closeButton);
		RegisterAppend(NPCID.TownSlimePurple, new NPCInteractions.Actions.PetAnimal());
		RegisterAppend(NPCID.TownSlimePurple, happinessButton);
		RegisterAppend(NPCID.TownSlimePurple, housingButton);

		RegisterAppend(NPCID.TownSlimeBlue, closeButton);
		RegisterAppend(NPCID.TownSlimeBlue, new NPCInteractions.Actions.PetAnimal());
		RegisterAppend(NPCID.TownSlimeBlue, happinessButton);
		RegisterAppend(NPCID.TownSlimeBlue, housingButton);

		RegisterAppend(NPCID.TownSlimeRed, closeButton);
		RegisterAppend(NPCID.TownSlimeRed, new NPCInteractions.Actions.PetAnimal());
		RegisterAppend(NPCID.TownSlimeRed, happinessButton);
		RegisterAppend(NPCID.TownSlimeRed, housingButton);

		RegisterAppend(NPCID.TownSlimeYellow, closeButton);
		RegisterAppend(NPCID.TownSlimeYellow, new NPCInteractions.Actions.PetAnimal());
		RegisterAppend(NPCID.TownSlimeYellow, happinessButton);
		RegisterAppend(NPCID.TownSlimeYellow, housingButton);

		RegisterAppend(NPCID.TownSlimeOld, closeButton);
		RegisterAppend(NPCID.TownSlimeOld, new NPCInteractions.Actions.PetAnimal());
		RegisterAppend(NPCID.TownSlimeOld, happinessButton);
		RegisterAppend(NPCID.TownSlimeOld, housingButton);

		RegisterAppend(NPCID.TownSlimeGreen, closeButton);
		RegisterAppend(NPCID.TownSlimeGreen, new NPCInteractions.Actions.PetAnimal());
		RegisterAppend(NPCID.TownSlimeGreen, happinessButton);
		RegisterAppend(NPCID.TownSlimeGreen, housingButton);

		RegisterAppend(NPCID.TownSlimeRainbow, closeButton);
		RegisterAppend(NPCID.TownSlimeRainbow, new NPCInteractions.Actions.PetAnimal());
		RegisterAppend(NPCID.TownSlimeRainbow, happinessButton);
		RegisterAppend(NPCID.TownSlimeRainbow, housingButton);

		RegisterAppend(NPCID.OldMan, closeButton);
		RegisterAppend(NPCID.OldMan, new NPCInteractions.Actions.OldManCurse());
		RegisterAppend(NPCID.OldMan, happinessButton); // Register the happiness and housing buttons even though they'll never show up to match vanilla.
		RegisterAppend(NPCID.OldMan, housingButton);

		RegisterAppend(NPCID.TravellingMerchant, new NPCInteractions.Actions.OpenShop(19));
		RegisterAppend(NPCID.TravellingMerchant, closeButton);
		RegisterAppend(NPCID.TravellingMerchant, happinessButton);
		RegisterAppend(NPCID.TravellingMerchant, housingButton);

		RegisterAppend(NPCID.SkeletonMerchant, new NPCInteractions.Actions.OpenShop(20));
		RegisterAppend(NPCID.SkeletonMerchant, closeButton);
		RegisterAppend(NPCID.SkeletonMerchant, happinessButton);
		RegisterAppend(NPCID.SkeletonMerchant, housingButton);
	}
}
