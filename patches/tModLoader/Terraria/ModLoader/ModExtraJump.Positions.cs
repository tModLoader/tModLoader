namespace Terraria.ModLoader;

partial class ModExtraJump
{
	public sealed class Between
	{
		public ModExtraJump Before { get; }

		public ModExtraJump After { get; }

		public Between() { }

		public Between(ModExtraJump before, ModExtraJump after)
		{
			Before = before;
			After = after;
		}
	}

	protected static Between BeforeFirstVanillaExtraJump => new Between(null, ExtraJumpLoader.FirstVanillaExtraJump);

	protected static Between AfterLastVanillaExtraJump => new Between(ExtraJumpLoader.LastVanillaExtraJump, null);
}
