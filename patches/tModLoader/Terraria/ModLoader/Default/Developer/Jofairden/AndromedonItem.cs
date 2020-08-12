namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal abstract class AndromedonItem : DeveloperItem
	{
		public override string TooltipBrief => "Jofairden's ";
		public sealed override string SetName => "PowerRanger";
		public const int ShaderNumSegments = 8;
		public const int ShaderDrawOffset = 2;

		public override string Texture => $"ModLoader/Developer.Jofairden.{SetName}_{EquipTypeSuffix}";

		public sealed override void SetStaticDefaults() {
			DisplayName.SetDefault($"Andromedon {EquipTypeSuffix}");
			Tooltip.SetDefault("The power of the Andromedon flows within you");
		}
	}
}
