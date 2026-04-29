using System;
using System.Collections.Generic;
using Terraria.GameContent;

namespace Terraria.ModLoader;

/// <summary>
/// Allows for <see cref="NPCInteraction"/> chat buttons to be assigned to an NPC.
/// </summary>
public readonly struct NPCInteractionList(int npcNetId)
{
	/// <summary>
	/// This contains the NPCInteraction as well as if the interaction is enabled.
	/// </summary>
	/// <param name="interaction">The stored NPCInteraction.</param>
	/// <param name="enabled">If the interaction is enabled. Defaults to <see langword="true"/>.</param>
	/// <remarks>The contents can be viewed, but not assigned to.</remarks>
	public class Entry(NPCInteraction interaction, bool enabled = true)
	{
		/// <summary>The stored NPCInteraction.</summary>
		public NPCInteraction NPCInteraction { get; internal set; } = interaction;
		/// <summary>
		/// If the interaction is enabled.
		/// <br/>Use <c>interactions.Disable(interaction)</c> to disable.
		/// <br/>Defaults to <see langword="true"/>.
		/// </summary>
		public bool Enabled { get; internal set; } = enabled;

		// Added for debugging convenience. NPCInteraction gets printed as the full namespace + name.
		public override string ToString() => $"(NPCInteraction: {NPCInteraction}, Enabled: {Enabled})";
	}

	public readonly int NPCNetID => npcNetId;

	private readonly List<Entry> _interactionEntries = new List<Entry>();

	/// <summary>
	/// Returns the full <c>List&lt;NPCInteraction&gt;</c> of the NPC.
	/// </summary>
	/// <returns>An empty list if not found.</returns>
	public readonly List<NPCInteraction> GetInteractions()
	{
		List<NPCInteraction> list = new();
		foreach (Entry entry in _interactionEntries) {
			list.Add(entry.NPCInteraction);
		}
		return list;
	}

	public readonly List<Entry> GetInteractionEntries()
	{
		return _interactionEntries;
	}

	/// <summary>
	/// Searches the registered interactions to find the specified interaction type and returns the instance if found.
	/// <para>This method matches by the class type.
	/// <br/>Example: <see cref="NPCInteractionDatabase.CloseButton"/> or <c>new NPCInteractions.Actions.CloseChat()</c> will find any other type of CloseChat buttons.
	/// </para>
	/// </summary>
	/// <param name="interaction">The interaction to search for.
	/// <br/>Example: <see cref="NPCInteractionDatabase.CloseButton"/> or <c>new NPCInteractions.Actions.CloseChat()</c></param>
	/// <returns><see langword="null"/> if not found.</returns>
	public readonly Entry FindInteractionByType(NPCInteraction interaction)
	{
		return FindInteractionByType(interaction.GetType());
	}

	/// <summary>
	/// Searches the registered interactions to find the specified interaction type and returns the instance if found.
	/// <para>This method matches by the class type.
	/// <br/>Example: <see cref="NPCInteractionDatabase.CloseButton"/> or <c>new NPCInteractions.Actions.CloseChat()</c> will find any other type of CloseChat buttons.
	/// </para>
	/// </summary>
	/// <param name="searchInteraction">The interaction to search for.
	/// <br/>Example: <see cref="NPCInteractionDatabase.CloseButton"/> or <c>new NPCInteractions.Actions.CloseChat()</c></param>
	/// <param name="foundInteraction">The interaction that was found. <c>null</c> if not found.</param>
	/// <returns><see langword="true"/> if found.</returns>
	public readonly bool TryFindInteractionByType(NPCInteraction searchInteraction, out Entry foundInteraction)
	{
		foundInteraction = FindInteractionByType(searchInteraction.GetType());
		if (foundInteraction == null) {
			return false;
		}
		return true;
	}

	/// <summary>
	/// Searches the registered interactions to find the specified interaction type and returns the instance if found.
	/// <para>This method matches by the class type.
	/// <br/>Example: <see cref="NPCInteractionDatabase.CloseButton"/> or <c>typeof(NPCInteractions.Actions.CloseChat)</c> will find any other type of CloseChat buttons.
	/// </para>
	/// </summary>
	/// <param name="interaction">The interaction to search for.
	/// <br/>Example: <see cref="NPCInteractionDatabase.CloseButton"/> or <c>typeof(NPCInteractions.Actions.CloseChat)</c></param>
	/// <returns><see langword="null"/> if not found.</returns>
	public readonly Entry FindInteractionByType(Type interaction)
	{
		foreach (Entry entry in _interactionEntries) {
			if (entry.NPCInteraction.GetType() == interaction) { // Class types are the same.
				return entry;
			}
		}
		return null;
	}

	/// <summary>
	/// Searches the registered interactions to find the specified interaction type and returns the instance if found.
	/// <para>This method matches by the class type.
	/// <br/>Example: <see cref="NPCInteractionDatabase.CloseButton"/> or <c>typeof(NPCInteractions.Actions.CloseChat)</c> will find any other type of CloseChat buttons.
	/// </para>
	/// </summary>
	/// <param name="searchInteraction">The interaction to search for.
	/// <br/>Example: <see cref="NPCInteractionDatabase.CloseButton"/> or <c>typeof(NPCInteractions.Actions.CloseChat)</c></param>
	/// <param name="foundInteraction">The interaction that was found. <c>null</c> if not found.</param>
	/// <returns><see langword="true"/> if found.</returns>
	public readonly bool TryFindInteractionByType(Type searchInteraction, out Entry foundInteraction)
	{
		foundInteraction = FindInteractionByType(searchInteraction);
		if (foundInteraction == null) {
			return false;
		}
		return true;
	}

	/// <summary>
	/// Searches the registered interactions to find the specified interaction instance and returns the instance if found.
	/// <para>This method matches by the exact instance.
	/// <br/>If you don't already have the instance, use <see cref="FindInteractionByType(NPCInteraction)"/> or <see cref="FindInteractionByType(Type)"/> instead.
	/// </para>
	/// </summary>
	/// <param name="interaction">The interaction to search for.</param>
	/// <returns><see langword="null"/> if not found.</returns>
	public readonly Entry FindInteractionByInstance(NPCInteraction interaction)
	{
		foreach (Entry entry in _interactionEntries) {
			if (entry.NPCInteraction.Equals(interaction)) { // Instances are the same.
				return entry;
			}
		}
		return null;
	}

	/// <summary>
	/// Searches the registered interactions to find the specified interaction instance and returns the instance if found.
	/// <para>This method matches by the exact instance.
	/// <br/>If you don't already have the instance, use <see cref="FindInteractionByType(NPCInteraction)"/> or <see cref="FindInteractionByType(Type)"/> instead.
	/// </para>
	/// </summary>
	/// <param name="searchInteraction">The interaction to search for.</param>
	/// <param name="foundInteraction">The interaction that was found. <c>null</c> if not found.</param>
	/// <returns><see langword="true"/> if found.</returns>
	public readonly bool TryFindInteractionByInstance(NPCInteraction searchInteraction, out Entry foundInteraction)
	{
		foundInteraction = FindInteractionByInstance(searchInteraction);
		if (foundInteraction == null) {
			return false;
		}
		return true;
	}

	/// <summary>
	/// Registers a button at the beginning of the list.
	/// </summary>
	/// <param name="interaction">The NPCInteraction to register.</param>
	/// <returns>The supplied NPCInteraction as an NPCInteractionList.Entry</returns>
	public readonly Entry Prepend(NPCInteraction interaction)
	{
		Entry entry = new(interaction);
		_interactionEntries.Insert(0, entry);
		return entry;
	}

	/// <summary>
	/// Registers a button at the end of the list (after Happiness and Housing buttons if applicable).
	/// </summary>
	/// <param name="interaction">The NPCInteraction to register.</param>
	/// <returns>The supplied NPCInteraction as an NPCInteractionList.Entry</returns>
	public readonly Entry Append(NPCInteraction interaction)
	{
		Entry entry = new(interaction);
		_interactionEntries.Add(entry);
		return entry;
	}

	/// <summary>
	/// Registers a button right after another button.
	/// <para>The <paramref name="interactionAfter"/> needs to be the exact instance of the button.
	/// <br/>The Close, Happiness, Housing, and Pet buttons are already predefined and can be used with <see cref="NPCInteractionDatabase.CloseButton"/>, etc.
	/// <br/>If you don't already have the instance, use <see cref="FindInteractionByType(NPCInteraction)"/> or <see cref="FindInteractionByType(Type)"/> to get it.
	/// </para>
	/// </summary>
	/// <param name="interactionToRegister">The NPCInteraction to register.</param>
	/// <param name="interactionAfter">The NPCInteraction to search for.</param>
	/// <returns>The supplied NPCInteraction as an NPCInteractionList.Entry</returns>
	/// <remarks>If the <paramref name="interactionAfter"/> is not found, the <paramref name="interactionToRegister"/> will be added to the end of the list.</remarks>
	public readonly Entry InsertAfter(NPCInteraction interactionToRegister, NPCInteraction interactionAfter)
	{
		Entry entry = new(interactionToRegister);
		int index = -1;
		foreach (Entry searchEntry in _interactionEntries) {
			if (searchEntry.NPCInteraction.Equals(interactionAfter)) { // Instances are the same.
				index = _interactionEntries.IndexOf(searchEntry);
				break;
			}
		}

		if (index is not -1) {
			_interactionEntries.Insert(index + 1, entry);
		}
		else { // If the interactionAfter is not found, add to the end of the list.
			_interactionEntries.Add(entry);
		}
		return entry;
	}

	/// <inheritdoc cref="InsertAfter(NPCInteraction, NPCInteraction)"/>
	public readonly Entry InsertAfter(NPCInteraction interactionToRegister, Entry interactionAfter)
	{
		return InsertAfter(interactionToRegister, interactionAfter.NPCInteraction);
	}

	/// <summary>
	/// Registers a button right before another button.
	/// <para>The <paramref name="interactionBefore"/> needs to be the exact instance of the button.
	/// <br/>The Close, Happiness, Housing, and Pet buttons are already predefined and can be used with <see cref="NPCInteractionDatabase.CloseButton"/>, etc.
	/// <br/>If you don't already have the instance, use <see cref="FindInteractionByType(NPCInteraction)"/> or <see cref="FindInteractionByType(Type)"/> to get it.
	/// </para>
	/// </summary>
	/// <param name="interactionToRegister">The NPCInteraction to register.</param>
	/// <param name="interactionBefore">The NPCInteraction to search for.</param>
	/// <returns>The supplied NPCInteraction as an NPCInteractionList.Entry</returns>
	/// <remarks>If the <paramref name="interactionBefore"/> is not found, the <paramref name="interactionToRegister"/> will be added to the end of the list.</remarks>
	public readonly Entry InsertBefore(NPCInteraction interactionToRegister, NPCInteraction interactionBefore)
	{
		Entry entry = new(interactionToRegister);
		int index = -1;
		foreach (Entry searchEntry in _interactionEntries) {
			if (searchEntry.NPCInteraction.Equals(interactionBefore)) { // Instances are the same.
				index = _interactionEntries.IndexOf(searchEntry);
				break;
			}
		}

		if (index is not -1) {
			_interactionEntries.Insert(index, entry);
		}
		else { // If the interactionAfter is not found, add to the end of the list.
			_interactionEntries.Add(entry);
		}
		return entry;
	}

	/// <inheritdoc cref="InsertBefore(NPCInteraction, NPCInteraction)"/>
	public readonly Entry InsertBefore(NPCInteraction interactionToRegister, Entry interactionBefore)
	{
		return InsertBefore(interactionToRegister, interactionBefore.NPCInteraction);
	}

	/// <summary>
	/// Disables an interaction for the current NPC.
	/// <para>The <paramref name="interaction"/> needs to be the exact instance of the button.
	/// <br/>The Close, Happiness, Housing, and Pet buttons are already predefined and can be used with <see cref="NPCInteractionDatabase.CloseButton"/>, etc.
	/// <br/>If you don't already have the instance, use <see cref="FindInteractionByType(NPCInteraction)"/> or <see cref="FindInteractionByType(Type)"/> to get it.
	/// </para>
	/// </summary>
	/// <param name="interaction">The NPCInteractionList.Entry to disable.</param>
	public readonly void Disable(Entry interaction) // Could be static, but left it instanced so you can use the instanced interactions parameter.
	{
		interaction?.Enabled = false;
	}

	/// <summary>
	/// Enables an interaction for the current NPC.
	/// <para>The <paramref name="interaction"/> needs to be the exact instance of the button.
	/// <br/>The Close, Happiness, Housing, and Pet buttons are already predefined and can be used with <see cref="NPCInteractionDatabase.CloseButton"/>, etc.
	/// <br/>If you don't already have the instance, use <see cref="FindInteractionByType(NPCInteraction)"/> or <see cref="FindInteractionByType(Type)"/> to get it.
	/// </para>
	/// </summary>
	/// <param name="interaction">The NPCInteractionList.Entry to enable.</param>
	/// <remarks>Interactions are enabled by default, so this is only needed if the interaction was disabled.</remarks>
	public readonly void Enable(Entry interaction)
	{
		interaction?.Enabled = true;
	}

	// Included this method even though if you already have the Entry, you could just do `interaction.Enabled`
	/// <summary>
	/// If the interaction is enabled. Defaults to <see langword="true"/>.
	/// </summary>
	/// <param name="interaction">The NPCInteractionList.Entry to check.</param>
	/// <returns><see langword="true"/> if the button is enabled.</returns>
	public readonly bool IsEnabled(Entry interaction)
	{
		return interaction.Enabled;
	}
}
