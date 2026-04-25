using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs;

public class GlobalNPCInteractions : GlobalNPC
{
	public override void RegisterChatButtons(NPC npc, NPCInteractionList interactions, NPCInteraction closeButton, NPCInteraction happinessButton, NPCInteraction housingButton) {
		// Here we can add additional chat buttons to Town NPCs.
		if (npc.type == NPCID.Guide) {
			// Add a shop button that open the Zoologist's shop.
			// Vanilla shops can specified with "Terraria/NPCName/Shop" ("Decor" for the Painter's second shop)
			// Modded shops can be specified with "ModName/NPCName/ShopName"
			interactions.InsertBefore(NPCInteractions.Shop("Terraria/BestiaryGirl/Shop", "Shop"), closeButton);

			// Here we are going to remove the Guide's tips button.
			// First, find the tip button using interactions.FindInteractionByType(Type interaction, out int index);
			//   This will match the buttons based on their class type.
			// There is also interactions.FindInteractionByInstance(NPCInteraction interaction, out int index)
			//   This will match the buttons based on the exact instance.
			NPCInteraction guideTip = interactions.FindInteractionByType(typeof(NPCInteractions.Actions.GuideTip), out _);
			interactions.Remove(guideTip);
		}
	}

	public override void OnChatButtonClicked(NPC npc, NPCInteraction interaction) {
		// With OnChatButtonClicked, we can do additional things when any chat button is clicked. The interaction is the type of button that was clicked.
		// We can use pattern matching to match the interaction type.
		if (npc.type == NPCID.Guide && interaction is NPCInteractions.Actions.GuideReverseCrafting) {
			ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral($"<{npc.FullName}> Simply place a material in the slot and I'll tell you everything you can craft with it!"), Color.LightGray);
		}
	}

	public override bool PreChatButtonClicked(NPC npc, NPCInteraction interaction) {
		// Here we can stop buttons from running their interaction.
		// We can use pattern matching to match the interaction type.
		if (npc.type == NPCID.Guide && interaction is NPCInteractions.Actions.CloseChat) {
			Main.npcChatText = "You can't close my chat window!";
			return false;
		}

		return base.PreChatButtonClicked(npc, interaction);
	}
}
