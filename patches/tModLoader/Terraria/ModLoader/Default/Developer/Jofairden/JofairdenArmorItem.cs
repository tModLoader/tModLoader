namespace Terraria.ModLoader.Default.Developer.Jofairden
{
	internal abstract class JofairdenArmorItem : DeveloperItem
	{
		public override string TooltipBrief => "Jofairden's ";

		public sealed override void SetStaticDefaults() {
			DisplayName.SetDefault($"Andromedon {Name.Split('_')[1]}");
			Tooltip.SetDefault("The power of the Andromedon flows within you");
		}
	}
}
