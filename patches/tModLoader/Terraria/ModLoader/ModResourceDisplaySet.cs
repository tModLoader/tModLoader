using Terraria.GameContent.UI;

namespace Terraria.ModLoader
{

	public abstract class ModResourceDisplaySet : ModType, IPlayerResourcesDisplaySet
	{
		public int Type { get; internal set; }

		protected sealed override void Register() {
			ModTypeLookup<ModResourceDisplaySet>.Register(this);
			Type = ResourceDisplaySetLoader.Add(this);
		}

		/// <summary>
		/// The visible name shown in the settings menu. If this isn't overwritten then it defaults to the <see cref="Mod"/>'s display name.
		/// </summary>
		public virtual string DisplayName => Mod.Name;

		public string SetName => $"{Mod.Name}:{Name}";

		/// <summary>
		/// Constantly called when this set is selected. Drawn at the very beginning of <see cref="Main.GUIBarsDrawInner"/>.
		/// </summary>
		public virtual void Draw() { }

		/// <summary>
		/// Constantly called while the player isn't a ghost and the set is being drawn. <br />
		/// You'll have to manually check for when an element of the set is being hovered. <br/>
		/// Use this method for logic that should apply only when the set is being hovered
		/// (i.e. drawing health values using MouseText).
		/// </summary>
		public virtual void TryToHover() { }
	}
}
