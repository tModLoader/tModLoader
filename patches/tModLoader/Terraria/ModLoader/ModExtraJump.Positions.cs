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

	public sealed class BeforeParent : Position
	{
		public ModExtraJump Parent { get; }

		public BeforeParent(ModExtraJump parent)
		{
			Parent = parent;
		}
	}

	public sealed class AfterParent : Position
	{
		public ModExtraJump Parent { get; }

		public AfterParent(ModExtraJump parent)
		{
			Parent = parent;
		}
	}

	protected static Position BeforeFirstVanillaExtraJump => new BeforeParent(ExtraJumpLoader.FirstVanillaExtraJump);

	protected static Position AfterLastVanillaExtraJump => new AfterParent(ExtraJumpLoader.LastVanillaExtraJump);
}
