namespace Terraria.ModLoader;

partial class ModExtraJump
{
	public sealed class Between
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

	protected static Between BeforeFirstVanillaExtraJump => new Between(null, ExtraJumpLoader.FirstVanillaExtraJump);

	protected static Between AfterLastVanillaExtraJump => new Between(ExtraJumpLoader.LastVanillaExtraJump, null);
}
