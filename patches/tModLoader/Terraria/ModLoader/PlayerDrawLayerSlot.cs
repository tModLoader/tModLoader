using System;
using Terraria.DataStructures;

namespace Terraria.ModLoader;

[Autoload(false)]
public class PlayerDrawLayerSlot : PlayerDrawLayer
{
	public PlayerDrawLayer Layer { get; }
	public Multiple.Condition Condition { get; }

	private readonly int _slot;

	public override string Name => $"{Layer.Name}_slot{_slot}";

	internal PlayerDrawLayerSlot(PlayerDrawLayer layer, Multiple.Condition cond, int slot)
	{
		Layer = layer;
		Mod = Layer.Mod;
		Condition = cond;
		_slot = slot;
		AddChildAfter(Layer);
	}

	public override Position GetDefaultPosition() => throw new NotImplementedException();

	protected override void Draw(ref PlayerDrawSet drawInfo) { }

	public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) => Condition(drawInfo);
}
