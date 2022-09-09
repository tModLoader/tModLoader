using Terraria.GameContent.UI.ResourceSets;

namespace Terraria.ModLoader
{
	public abstract class ResourceDrawSource : IResourceDrawSource
	{
		public string Context { get; protected set; }

		public virtual IPlayerResourcesDisplaySet DisplaySet { get; }

		public ResourceDrawSource(string? context = null) {
			Context = context;
		}
	}

	public abstract class ResourceDrawSource_Life : ResourceDrawSource {
		public ResourceDrawSource_Life(string? context = null) : base(context) { }
	}

	public abstract class ResourceDrawSource_Mana : ResourceDrawSource {
		public ResourceDrawSource_Mana(string? context = null) : base(context) { }
	}

	public class ResourceDrawSource_ClassicLife : ResourceDrawSource_Life {
		public override IPlayerResourcesDisplaySet DisplaySet => Main.ResourceSetsManager.GetDisplaySet("Default");

		public ResourceDrawSource_ClassicLife(string? context = null) : base(context) { }
	}

	public class ResourceDrawSource_ClassicMana : ResourceDrawSource_Mana {
		public override IPlayerResourcesDisplaySet DisplaySet => Main.ResourceSetsManager.GetDisplaySet("Default");

		public ResourceDrawSource_ClassicMana(string? context = null) : base(context) { }
	}

	public class ResourceDrawSource_FancyLife : ResourceDrawSource_Life {
		public override IPlayerResourcesDisplaySet DisplaySet => Main.ResourceSetsManager.GetDisplaySet("New");

		public ResourceDrawSource_FancyLife(string? context = null) : base(context) { }
	}

	public class ResourceDrawSource_FancyMana : ResourceDrawSource_Mana {
		public override IPlayerResourcesDisplaySet DisplaySet => Main.ResourceSetsManager.GetDisplaySet("new");

		public ResourceDrawSource_FancyMana(string? context = null) : base(context) { }
	}

	public class ResourceDrawSource_BarsLife : ResourceDrawSource_Life {
		public override IPlayerResourcesDisplaySet DisplaySet => Main.ResourceSetsManager.GetDisplaySet("HorizontalBars");

		public ResourceDrawSource_BarsLife(string? context = null) : base(context) { }
	}

	public class ResourceDrawSource_BarsMana : ResourceDrawSource_Mana {
		public override IPlayerResourcesDisplaySet DisplaySet => Main.ResourceSetsManager.GetDisplaySet("HorizontalBars");

		public ResourceDrawSource_BarsMana(string? context = null) : base(context) { }
	}
}
