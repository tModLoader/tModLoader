using System;
using System.Collections.Generic;
using Terraria.GameContent;

namespace Terraria.ModLoader;

/// <summary>
/// Allows for <see cref="NPCInteraction"/> chat buttons to be assigned to an NPC.
/// </summary>
public readonly struct NPCInteractionList : INPCInteractionList
{
	private readonly int npcNetId;
	private readonly NPCInteractionDatabase interactionDatabase;

	public NPCInteractionList(int npcNetId, NPCInteractionDatabase interactionDatabase)
	{
		this.npcNetId = npcNetId;
		this.interactionDatabase = interactionDatabase;
	}

    /// <summary>
    /// Returns the full <c>List&lt;NPCInteraction&gt;</c> of the NPC.
    /// </summary>
    /// <returns>An empty list if not found.</returns>
    public readonly List<NPCInteraction> GetInteractions()
	{
		return interactionDatabase.GetInteractionsForNPCID(npcNetId);
	}

	/// <summary>
	/// Searches the registered interactions to find the specified interaction type and returns the instance if found.
	/// <para>This method matches by the class type.
	/// <br/>Example: <c>new NPCInteractions.Actions.CloseChat()</c> will find any other type of CloseChat buttons.
	/// </para>
	/// </summary>
	/// <param name="interaction">The interaction to search for.
	/// <br/>Example: <c>new NPCInteractions.Actions.CloseChat()</c></param>
	/// <param name="index">The index that the interaction was found at. -1 if not found.</param>
	/// <returns><c>null</c> if not found.</returns>
	public readonly NPCInteraction FindInteractionByType(NPCInteraction interaction, out int index)
	{
		return interactionDatabase.FindInteractionByType(npcNetId, interaction.GetType(), out index);
	}

	/// <summary>
	/// Searches the registered interactions to find the specified interaction type and returns the instance if found.
	/// <para>This method matches by the class type.
	/// <br/>Example: <c>new NPCInteractions.Actions.CloseChat()</c> will find any other type of CloseChat buttons.
	/// </para>
	/// </summary>
	/// <param name="searchInteraction">The interaction to search for.
	/// <br/>Example: <c>new NPCInteractions.Actions.CloseChat()</c></param>
	/// <param name="foundInteraction">The interaction that was found. <c>null</c> if not found.</param>
	/// <param name="index">The index that the interaction was found at. -1 if not found.</param>
	/// <returns>True if found.</returns>
	public readonly bool TryFindInteractionByType(NPCInteraction searchInteraction, out NPCInteraction foundInteraction, out int index)
	{
		foundInteraction = interactionDatabase.FindInteractionByType(npcNetId, searchInteraction.GetType(), out index);
		if (foundInteraction == null) {
			return false;
		}
		return true;
	}

	/// <summary>
	/// Searches the registered interactions to find the specified interaction type and returns the instance if found.
	/// <para>This method matches by the class type.
	/// <br/>Example: <c>typeof(NPCInteractions.Actions.CloseChat)</c> will find any other type of CloseChat buttons.
	/// </para>
	/// </summary>
	/// <param name="interaction">The interaction to search for.
	/// <br/>Example: <c>typeof(NPCInteractions.Actions.CloseChat)</c></param>
	/// <param name="index">The index that the interaction was found at. -1 if not found.</param>
	/// <returns><c>null</c> if not found.</returns>
	public readonly NPCInteraction FindInteractionByType(Type interaction, out int index)
	{
		return interactionDatabase.FindInteractionByType(npcNetId, interaction, out index);
	}

	/// <summary>
	/// Searches the registered interactions to find the specified interaction type and returns the instance if found.
	/// <para>This method matches by the class type.
	/// <br/>Example: <c>typeof(NPCInteractions.Actions.CloseChat)</c> will find any other type of CloseChat buttons.
	/// </para>
	/// </summary>
	/// <param name="searchInteraction">The interaction to search for.
	/// <br/>Example: <c>typeof(NPCInteractions.Actions.CloseChat)</c></param>
	/// <param name="foundInteraction">The interaction that was found. <c>null</c> if not found.</param>
	/// <param name="index">The index that the interaction was found at. -1 if not found.</param>
	/// <returns>True if found.</returns>
	public readonly bool TryFindInteractionByType(Type searchInteraction, out NPCInteraction foundInteraction, out int index)
	{
		foundInteraction = interactionDatabase.FindInteractionByType(npcNetId, searchInteraction, out index);
		if (foundInteraction == null) {
			return false;
		}
		return true;
	}

	/// <summary>
	/// Searches the registered interactions to find the specified interaction instance and returns the instance if found.
	/// <para>This method matches by the exact instance.
	/// <br/>If you don't already have the instance, use <see cref="FindInteractionByType(NPCInteraction, out int)"/> or <see cref="FindInteractionByType(Type, out int)"/> instead.
	/// </para>
	/// </summary>
	/// <param name="interaction">The interaction to search for.</param>
	/// <param name="index">The index that the interaction was found at. -1 if not found.</param>
	/// <returns><c>null</c> if not found.</returns>
	public readonly NPCInteraction FindInteractionByInstance(NPCInteraction interaction, out int index)
	{
		return interactionDatabase.FindInteractionByInstance(npcNetId, interaction, out index);
	}

	/// <summary>
	/// Searches the registered interactions to find the specified interaction instance and returns the instance if found.
	/// <para>This method matches by the exact instance.
	/// <br/>If you don't already have the instance, use <see cref="FindInteractionByType(NPCInteraction, out int)"/> or <see cref="FindInteractionByType(Type, out int)"/> instead.
	/// </para>
	/// </summary>
	/// <param name="searchInteraction">The interaction to search for.</param>
	/// <param name="foundInteraction">The interaction that was found. <c>null</c> if not found.</param>
	/// <param name="index">The index that the interaction was found at. -1 if not found.</param>
	/// <returns>True if found.</returns>
	public readonly bool TryFindInteractionByInstance(NPCInteraction searchInteraction, out NPCInteraction foundInteraction, out int index)
	{
		foundInteraction = interactionDatabase.FindInteractionByInstance(npcNetId, searchInteraction, out index);
		if (foundInteraction == null) {
			return false;
		}
		return true;
	}

	/// <summary>
	/// Registers a button at the beginning of the list.
	/// </summary>
	/// <param name="interaction">The NPCInteraction to register.</param>
	/// <returns>The supplied NPCInteraction</returns>
	public readonly NPCInteraction Prepend(NPCInteraction interaction)
	{
		InsertAt(interaction, 0);
		return interaction;
	}

	/// <summary>
	/// Registers a button at the end of the list (after Happiness and Housing buttons if applicable).
	/// </summary>
	/// <param name="interaction">The NPCInteraction to register.</param>
	/// <returns>The supplied NPCInteraction</returns>
	public readonly NPCInteraction Append(NPCInteraction interaction)
	{
		interactionDatabase.RegisterAppend(npcNetId, interaction);
		return interaction;
	}

	/// <summary>
	/// Registers a button right after another button.
	/// <para>The <paramref name="interactionAfter"/> needs to be the exact instance of the button.
	/// <br/>If you don't already have the instance, use <see cref="FindInteractionByType(NPCInteraction, out int)"/> or <see cref="FindInteractionByType(Type, out int)"/> to get it.
	/// </para>
	/// </summary>
	/// <param name="interactionToRegister">The NPCInteraction to register.</param>
	/// <param name="interactionAfter">The NPCInteraction to search for.</param>
	/// <returns>The supplied NPCInteraction</returns>
	/// <remarks>If the <paramref name="interactionAfter"/> is not found, the <paramref name="interactionToRegister"/> will be added to the end of the list.</remarks>
	public readonly NPCInteraction InsertAfter(NPCInteraction interactionToRegister, NPCInteraction interactionAfter)
	{
		interactionDatabase.RegisterAfter(npcNetId, interactionToRegister, interactionAfter);
		return interactionToRegister;
	}

	/// <summary>
	/// Registers a button right before another button.
	/// <para>The <paramref name="interactionBefore"/> needs to be the exact instance of the button.
	/// <br/>If you don't already have the instance, use <see cref="FindInteractionByType(NPCInteraction, out int)"/> or <see cref="FindInteractionByType(Type, out int)"/> to get it.
	/// </para>
	/// </summary>
	/// <param name="interactionToRegister">The NPCInteraction to register.</param>
	/// <param name="interactionBefore">The NPCInteraction to search for.</param>
	/// <returns>The supplied NPCInteraction</returns>
	/// <remarks>If the <paramref name="interactionBefore"/> is not found, the <paramref name="interactionToRegister"/> will be added to the end of the list.</remarks>
	public readonly NPCInteraction InsertBefore(NPCInteraction interactionToRegister, NPCInteraction interactionBefore)
	{
		interactionDatabase.RegisterBefore(npcNetId, interactionToRegister, interactionBefore);
		return interactionToRegister;
	}

	/// <summary>
	/// Registers a button at the specified index. Buttons after the index will be shifted in the list.
	/// </summary>
	/// <param name="interaction">The NPCInteraction to register.</param>
	/// <param name="index">The index to insert the button at. The list starts at 0. So, the first button is index 0.</param>
	/// <returns>The supplied NPCInteraction</returns>
	public readonly NPCInteraction InsertAt(NPCInteraction interaction, int index)
	{
		interactionDatabase.RegisterAt(npcNetId, interaction, index);
		return interaction;
	}

    /// <summary>
    /// Removes a button from the list.
    /// <para>The <paramref name="interaction"/> needs to be the exact instance of the button.
    /// <br/>If you don't already have the instance, use <see cref="FindInteractionByType(NPCInteraction, out int)"/> or <see cref="FindInteractionByType(Type, out int)"/> to get it.
    /// </para>
    /// </summary>
    /// <param name="interaction">The NPCInteraction to register.</param>
    /// <returns>True if the button was removed successfully.</returns>
    public readonly bool Remove(NPCInteraction interaction)
	{
		return interactionDatabase.RemoveFromNPCNetId(npcNetId, interaction);
	}
}
