using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Common.GlobalNPCs;

// This example showcases adding additional buttons to existing NPC.
public class GlobalNPCInteractions : GlobalNPC
{
	public override void RegisterChatButtons(NPC npc, NPCInteractionList interactions) {
		// Here we can add additional chat buttons to Town NPCs.
		if (npc.type == NPCID.Guide) {
			// Add a shop button that open the Zoologist's shop.
			// Vanilla shops can specified with "Terraria/NPCName/Shop" ("Decor" for the Painter's second shop)
			// Modded shops can be specified with "ModName/NPCName/ShopName" (The ShopName is typically "Shop")
			interactions.InsertBefore(NPCInteractions.Shop("Terraria/BestiaryGirl/Shop", "Shop"), NPCInteractionDatabase.CloseButton);

			// Here we are going to disable the Guide's tips button.
			// First, find the tip button using interactions.TryFindInteractionByType(Type searchInteraction, out NPCInteractionList.Entry foundInteraction);
			//   This will match the buttons based on their class type.
			// There is also interactions.TryFindInteractionByInstance(NPCInteraction searchInteraction, out NPCInteractionList.Entry foundInteraction)
			//   This will match the buttons based on the exact instance.
			if (interactions.TryFindInteractionByType(typeof(NPCInteractions.Actions.GuideTip), out NPCInteractionList.Entry foundInteraction)) {
				interactions.Disable(foundInteraction);
			}
			// This is the same thing as above but doesn't have the built in null check that TryFind does.
			// NPCInteractionList.Entry guideTip = interactions.FindInteractionByType(typeof(NPCInteractions.Actions.GuideTip));
			// interactions.Disable(guideTip);
		}
	}

	public override void OnChatButtonClicked(NPC npc, NPCInteraction interaction) {
		// With OnChatButtonClicked, we can do additional things when any chat button is clicked. The interaction is the type of button that was clicked.
		if (npc.type == NPCID.Guide && interaction is NPCInteractions.Actions.GuideReverseCrafting) {
			// OnChatButtonClicked only runs for the local player who clicked the button. Any multiplayer functionality will need to be synced with a packet.
			Main.NewText($"<{npc.FullName}> Simply place a material in the slot and I'll tell you everything you can craft with it!");
		}
	}

	public override bool PreChatButtonClicked(NPC npc, NPCInteraction interaction) {
		// Here we can stop buttons from running their interaction.
		if (npc.type == NPCID.Guide && interaction is NPCInteractions.Actions.CloseChat) {
			Main.npcChatText = "You can't close my chat window!";
			return false;
		}

		return base.PreChatButtonClicked(npc, interaction);
	}
}
