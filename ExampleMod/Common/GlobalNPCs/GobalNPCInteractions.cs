using System.Linq;
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
			// This way matches the type of the interaction and returns the first that matches or null if not found.
			// If the interaction wasn't found, nothing happens.
			NPCInteraction guideTipNPCInteraction = interactions.Interactions.OfType<NPCInteractions.Actions.GuideTip>().FirstOrDefault();
			interactions.Disable(guideTipNPCInteraction); // If the instance is null (aka not found), Disable won't do anything.

			// Alternate way: this way does the same thing, but searches the Entries instead and returns the NPCInteractionList.Entry if found.
			// NPCInteractionList.Entry guideTipEntry = interactions.Entries.Where(e => e.NPCInteraction.GetType() == typeof(NPCInteractions.Actions.GuideTip)).FirstOrDefault();
			// interactions.Disable(guideTipEntry); // If the instance is null (aka not found), Disable won't do anything.
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
