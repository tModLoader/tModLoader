namespace Terraria.ModLoader;

partial class ModExtraJump
{
	public abstract class Position { }

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
