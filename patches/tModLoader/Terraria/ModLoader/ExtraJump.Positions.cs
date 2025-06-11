namespace Terraria.ModLoader;

partial class ExtraJump
{
	public abstract class Position
	{
		public ExtraJump Target { get; protected init; }
	}

	public sealed class Before : Position
	{
		public Before(ExtraJump parent)
		{
			Target = parent;
		}
	}

	public sealed class After : Position
	{
		public After(ExtraJump parent)
		{
			Target = parent;
		}
	}

	protected static readonly Position BeforeMountJumps = new Before(BasiliskMount);
	protected static readonly Position MountJumpPosition = new After(BasiliskMount);
	protected static readonly Position BeforeBottleJumps = new Before(SandstormInABottle);
	protected static readonly Position AfterBottleJumps = new After(CloudInABottle);
}
