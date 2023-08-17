using Terraria.ModLoader;

namespace Terraria.DataStructures;

[Autoload(false)]
internal sealed class VanillaPlayerDrawLayer : PlayerDrawLayer
{
	/// <summary> The delegate of this method, which can either do the actual drawing or add draw data, depending on what kind of layer this is. </summary>
	public delegate void DrawFunc(ref PlayerDrawSet info);
	private readonly DrawFunc drawFunc;

	/// <summary> The delegate of this method, which can either do the actual drawing or add draw data, depending on what kind of layer this is. </summary>
	public delegate bool Condition(PlayerDrawSet info);
	private readonly Condition condition;

	private Transformation _transform;
	public override Transformation Transform => _transform;

	private readonly string _name;
	public override string Name => _name;

	private readonly bool _isHeadLayer;
	public override bool IsHeadLayer => _isHeadLayer;

	private readonly Position position;

	/// <summary> Creates a LegacyPlayerLayer with the given mod name, identifier name, and drawing action. </summary>
	public VanillaPlayerDrawLayer(string name, DrawFunc drawFunc, Transformation transform = null, bool isHeadLayer = false, Condition condition = null, Position position = null)
	{
		_name = name;
		this.drawFunc = drawFunc;
		this.condition = condition;
		this.position = position;
		_transform = transform;
		_isHeadLayer = isHeadLayer;
	}

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => condition?.Invoke(drawInfo) ?? true;

	public override Position GetDefaultPosition()
	{
		if (position != null)
			return position;

		// poor man's IndexOf because IReadOnlyList doesn't expose the method
		int index = 0;
		while (PlayerDrawLayers.FixedVanillaLayers[index] != this)
			index++;

		return new Between(
			index > 0 ? PlayerDrawLayers.FixedVanillaLayers[index - 1] : null,
			index < PlayerDrawLayers.FixedVanillaLayers.Count - 1 ? PlayerDrawLayers.FixedVanillaLayers[index + 1] : null);
	}

	protected override void Draw(ref PlayerDrawSet drawInfo) => drawFunc(ref drawInfo);
}
