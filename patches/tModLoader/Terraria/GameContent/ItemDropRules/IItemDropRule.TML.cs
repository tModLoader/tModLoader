namespace Terraria.GameContent.ItemDropRules;

partial interface IItemDropRule
{
	bool Disabled { get; }

	void Disable();
}
