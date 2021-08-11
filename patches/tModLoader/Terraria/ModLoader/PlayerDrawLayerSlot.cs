using System;
using Terraria.DataStructures;

namespace Terraria.ModLoader
{
	[Autoload(false)]
	public class PlayerDrawLayerSlot : PlayerDrawLayer
	{
		public PlayerDrawLayer Layer { get; }
		public Multiple.Condition Condition { get; }

		private readonly int _slot;

		public override string Name => $"{Layer.Name}_slot{_slot}";

		internal PlayerDrawLayerSlot(PlayerDrawLayer layer, Multiple.Condition cond, int slot) {
			Layer = layer;
			Condition = cond;
			_slot = slot;
		}

		public override Position GetDefaultPosition() => throw new NotImplementedException();

		protected override void Draw(ref PlayerDrawSet drawInfo) => Layer.DrawWithTransformationAndChildren(ref drawInfo);

		protected internal override void ResetVisiblity(PlayerDrawSet drawInfo) {
			foreach (var child in Layer.ChildrenBefore)
				child.ResetVisiblity(drawInfo);

			base.ResetVisiblity(drawInfo);

			foreach (var child in Layer.ChildrenAfter)
				child.ResetVisiblity(drawInfo);
		}

		public override bool GetDefaultVisiblity(PlayerDrawSet drawInfo) => Condition(drawInfo);
	}
}
