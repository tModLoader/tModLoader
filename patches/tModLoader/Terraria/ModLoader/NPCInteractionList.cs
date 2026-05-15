using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;

namespace Terraria.ModLoader;

/// <summary>
/// Allows for <see cref="NPCInteraction"/> chat buttons to be assigned to an NPC.
/// </summary>
public class NPCInteractionList(int type)
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

	public int Type => type;

	private readonly List<Entry> _entries = new List<Entry>();

	/// <summary>
	/// All of the NPCInteractions for the NPC.
	/// </summary>
	public IEnumerable<NPCInteraction> Interactions => _entries.Select(e => e.NPCInteraction);

	/// <summary>
	/// All of the entries for this NPC which includes the NPCInteraction and if it is Enabled.
	/// </summary>
	public IReadOnlyList<Entry> Entries => _entries;

	/// <summary>
	/// Registers a button at the beginning of the list.
	/// </summary>
	/// <param name="interaction">The NPCInteraction to register.</param>
	/// <returns>The supplied NPCInteraction as an NPCInteractionList.Entry</returns>
	public Entry Prepend(NPCInteraction interaction)
	{
		Entry entry = new(interaction);
		_entries.Insert(0, entry);
		return entry;
	}

	/// <summary>
	/// Registers a button at the end of the list (after Happiness and Housing buttons if applicable).
	/// </summary>
	/// <param name="interaction">The NPCInteraction to register.</param>
	/// <returns>The supplied NPCInteraction as an NPCInteractionList.Entry</returns>
	public Entry Append(NPCInteraction interaction)
	{
		Entry entry = new(interaction);
		_entries.Add(entry);
		return entry;
	}

	/// <summary>
	/// Registers a button right after another button.
	/// <para>The <paramref name="interactionAfter"/> needs to be the exact instance of the button.
	/// <br/>The Close, Happiness, Housing, and Pet buttons are already predefined and can be used with <see cref="NPCInteractionDatabase.CloseButton"/>, etc.
	/// <br/>If you don't already have the instance, you can use <c>interactions.Interactions.OfType&lt;TheNPCInteractionYouWishToFind&gt;().FirstOrDefault()</c> or similar to get it.
	/// </para>
	/// </summary>
	/// <param name="interactionToRegister">The NPCInteraction to register.</param>
	/// <param name="interactionAfter">The NPCInteraction to search for.</param>
	/// <returns>The supplied NPCInteraction as an NPCInteractionList.Entry</returns>
	/// <remarks>If the <paramref name="interactionAfter"/> is not found, the <paramref name="interactionToRegister"/> will be added to the end of the list.</remarks>
	public Entry InsertAfter(NPCInteraction interactionToRegister, NPCInteraction interactionAfter)
	{
		Entry entry = new(interactionToRegister);
		int index = _entries.FindIndex(e => e.NPCInteraction.Equals(interactionAfter)); // Instances are the same.

		if (index is not -1) {
			_entries.Insert(index + 1, entry);
		}
		else { // If the interactionAfter is not found, add to the end of the list.
			_entries.Add(entry);
		}
		return entry;
	}

	/// <inheritdoc cref="InsertAfter(NPCInteraction, NPCInteraction)"/>
	public Entry InsertAfter(NPCInteraction interactionToRegister, Entry interactionAfter)
	{
		return InsertAfter(interactionToRegister, interactionAfter.NPCInteraction);
	}

	/// <summary>
	/// Registers a button right before another button.
	/// <para>The <paramref name="interactionBefore"/> needs to be the exact instance of the button.
	/// <br/>The Close, Happiness, Housing, and Pet buttons are already predefined and can be used with <see cref="NPCInteractionDatabase.CloseButton"/>, etc.
	/// <br/>If you don't already have the instance, you can use <c>interactions.Interactions.OfType&lt;TheNPCInteractionYouWishToFind&gt;().FirstOrDefault()</c> or similar to get it.
	/// </para>
	/// </summary>
	/// <param name="interactionToRegister">The NPCInteraction to register.</param>
	/// <param name="interactionBefore">The NPCInteraction to search for.</param>
	/// <returns>The supplied NPCInteraction as an NPCInteractionList.Entry</returns>
	/// <remarks>If the <paramref name="interactionBefore"/> is not found, the <paramref name="interactionToRegister"/> will be added to the end of the list.</remarks>
	public Entry InsertBefore(NPCInteraction interactionToRegister, NPCInteraction interactionBefore)
	{
		Entry entry = new(interactionToRegister);
		int index = _entries.FindIndex(e => e.NPCInteraction.Equals(interactionBefore)); // Instances are the same.

		if (index is not -1) {
			_entries.Insert(index, entry);
		}
		else { // If the interactionAfter is not found, add to the end of the list.
			_entries.Add(entry);
		}
		return entry;
	}

	/// <inheritdoc cref="InsertBefore(NPCInteraction, NPCInteraction)"/>
	public Entry InsertBefore(NPCInteraction interactionToRegister, Entry interactionBefore)
	{
		return InsertBefore(interactionToRegister, interactionBefore.NPCInteraction);
	}

	/// <summary>
	/// Disables an interaction for the current NPC.
	/// <para>The <paramref name="interaction"/> needs to be the exact instance of the button.
	/// <br/>The Close, Happiness, Housing, and Pet buttons are already predefined and can be used with <see cref="NPCInteractionDatabase.CloseButton"/>, etc.
	/// <br/>If you don't already have the instance, use <see cref="Disable(NPCInteraction)"/> or search for the instance first.
	/// </para>
	/// </summary>
	/// <param name="interaction">The NPCInteractionList.Entry to disable.</param>
	public void Disable(Entry interaction) // Could be static, but left it instanced so you can use the instanced interactions parameter.
	{
		interaction?.Enabled = false;
	}

	/// <summary>
	/// Disables an interaction for the current NPC.
	/// <para>The <paramref name="interaction"/> needs to be the exact instance of the button.
	/// <br/>The Close, Happiness, Housing, and Pet buttons are already predefined and can be used with <see cref="NPCInteractionDatabase.CloseButton"/>, etc.
	/// <br/>This method will find the first NPCInteraction by instance that matches and mark it as disabled.
	/// </para>
	/// </summary>
	/// <param name="interaction">The NPCInteraction to disable.</param>
	public void Disable(NPCInteraction interaction)
	{
		_entries.FirstOrDefault(e => e.NPCInteraction.Equals(interaction))?.Enabled = false;
	}

	// Included this method even though if you already have the Entry, you could just do `interaction.Enabled`
	/// <summary>
	/// If the interaction is enabled. Defaults to <see langword="true"/>.
	/// </summary>
	/// <param name="interaction">The NPCInteractionList.Entry to check.</param>
	/// <returns><see langword="true"/> if the button is enabled.</returns>
	public bool IsEnabled(Entry interaction)
	{
		return interaction?.Enabled ?? false;
	}

	/// <summary>
	/// If the interaction is enabled. Defaults to <see langword="true"/>.
	/// </summary>
	/// <param name="interaction">The NPCInteraction to check.</param>
	/// <returns><see langword="true"/> if the button is enabled.</returns>
	public bool IsEnabled(NPCInteraction interaction)
	{
		return _entries.FirstOrDefault(e  => e.NPCInteraction.Equals(interaction))?.Enabled ?? false;
	}
}
