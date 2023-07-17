namespace Terraria.ModLoader;

partial class ExtraJump
{
	public abstract class Position { }

	public sealed class Before : Position
	{
		public ExtraJump Target { get; }

		public Before(ExtraJump parent)
		{
			Target = parent;
		}
	}

	public sealed class After : Position
	{
		public ExtraJump Target { get; }

		public After(ExtraJump parent)
		{
			Target = parent;
		}
	}

	protected static Position BeforeFirstVanillaExtraJump => new Before(ExtraJumpLoader.FirstVanillaExtraJump);

	protected static Position AfterLastVanillaExtraJump => new After(ExtraJumpLoader.LastVanillaExtraJump);
}
