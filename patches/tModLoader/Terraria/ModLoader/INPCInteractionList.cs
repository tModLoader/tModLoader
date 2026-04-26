using System;
using System.Collections.Generic;
using Terraria.GameContent;

namespace Terraria.ModLoader;

public interface INPCInteractionList
{
	List <NPCInteraction> GetInteractions();

	NPCInteraction Prepend(NPCInteraction interaction);

	NPCInteraction Append(NPCInteraction interaction);

	NPCInteraction InsertAfter(NPCInteraction interactionToAdd, NPCInteraction interactionAfter);

	NPCInteraction InsertBefore(NPCInteraction interactionToAdd, NPCInteraction interactionBefore);

	NPCInteraction InsertAt(NPCInteraction interaction, int index);

	bool Remove(NPCInteraction interaction);
}
