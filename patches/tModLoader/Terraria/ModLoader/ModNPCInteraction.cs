using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace Terraria.ModLoader;

/// <summary>
/// Base class for Mod NPC interactions. Automatically registered when the mod loads.
/// </summary>
public abstract class ModNPCInteraction : NPCInteraction, ILoadable
{
	public Mod Mod { get; internal set; }

	/// <summary>
	/// The NPC type this interaction applies to.
	/// </summary>
	public abstract int ForNPCType { get; }

	/// <summary>
	/// Whether to show an exclamation mark icon next to this interaction button.
	/// </summary>
	public override bool ShowExcalmation => false;

	/// <summary>
	/// Condition for when this interaction should appear. Default checks if talking to target NPC.
	/// </summary>
	public override bool Condition() => TalkNPCType == ForNPCType;

	/// <summary>
	/// The text displayed on the interaction button.
	/// </summary>
	public abstract override string GetText();

	/// <summary>
	/// What happens when the player clicks the interaction button.
	/// </summary>
	public abstract override void Interact();

	/// <summary>
	/// Optional: Display a coin value next to the interaction button.
	/// </summary>
	public override bool TryAddCoins(ref Color chatColor, out int coinValue)
	{
		coinValue = 0;
		return false;
	}

	void ILoadable.Load(Mod mod)
	{
		Mod = mod;
		NPCInteractions.All.Add(this);
	}

	void ILoadable.Unload() { }
}