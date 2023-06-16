namespace Terraria.ModLoader;

partial class ModExtraJump
{
	public abstract class Position { }

	public sealed class Between : Position
	{
		public ModExtraJump Dependency { get; }

		public ModExtraJump Dependent { get; }

		public Between() { }

		public Between(ModExtraJump dependency, ModExtraJump dependent)
		{
			Dependency = dependency;
			Dependent = dependent;
		}
	}

	public sealed class Before : Position
	{
		public ModExtraJump Parent { get; }

		public Before(ModExtraJump parent)
		{
			Parent = parent;
		}
	}

	public sealed class After : Position
	{
		public ModExtraJump Parent { get; }

		public After(ModExtraJump parent)
		{
			Parent = parent;
		}
	}

	protected static Position BeforeFirstVanillaExtraJump => new Before(ExtraJumpLoader.FirstVanillaExtraJump);

	protected static Position AfterLastVanillaExtraJump => new After(ExtraJumpLoader.LastVanillaExtraJump);
}
