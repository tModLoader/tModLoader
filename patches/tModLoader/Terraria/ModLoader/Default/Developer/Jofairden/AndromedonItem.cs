namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal abstract class AndromedonItem : DeveloperItem
	{
		public override string TooltipBrief => "Jofairden's ";
		public const int ShaderNumSegments = 8;
		public const int ShaderDrawOffset = 2;

		public sealed override void SetStaticDefaults() {
			DisplayName.SetDefault($"Andromedon {Name.Split('_')[1]}");
			Tooltip.SetDefault("The power of the Andromedon flows within you");
		}
	}
}
