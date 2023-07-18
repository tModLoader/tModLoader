using Terraria.DataStructures;

#nullable enable

namespace Terraria;

partial class Entity
{
	/// <summary> Should not makes changes to the game state. consider read only </summary>
	/// <returns> True if the entity can be shimmered false if not, and null if this type of entity can never undergo a shimmer operation </returns>
	public virtual bool? CanShimmer() => null;
	public virtual void OnShimmer() { }
}
