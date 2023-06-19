namespace Terraria.ModLoader;

partial class ExtraJump
{
	public abstract class Position { }

	public sealed class Before : Position
	{
		public ExtraJump Parent { get; }

		public Before(ExtraJump parent)
		{
			Parent = parent;
		}
	}

	public sealed class After : Position
	{
		public ExtraJump Parent { get; }

		public After(ExtraJump parent)
		{
			Parent = parent;
		}
	}

	protected static Position BeforeFirstVanillaExtraJump => new Before(ExtraJumpLoader.FirstVanillaExtraJump);

	protected static Position AfterLastVanillaExtraJump => new After(ExtraJumpLoader.LastVanillaExtraJump);
}
