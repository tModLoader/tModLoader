using System;
using System.Linq;
using Terraria.GameContent;
using Terraria.Localization;

namespace Terraria.ModLoader
{
	[Autoload(false)]
	public abstract class VanillaBuilderToggle : BuilderToggle
	{
		public override string Texture => "Terraria/Images/UI/BuilderIcons";
		public override string DisplayValue() => "";
		public override int NumberOfStates => 2;
	}

	public class RulerLineBuilderToggle : VanillaBuilderToggle
	{
		public override bool Active() => Main.player[Main.myPlayer].rulerLine;
		public override int CurrentState => Main.player[Main.myPlayer].builderAccStatus[0];
	}

	public class RulerGridBuilderToggle : VanillaBuilderToggle
	{
		public override bool Active() => Main.player[Main.myPlayer].rulerGrid;
		public override int CurrentState => Main.player[Main.myPlayer].builderAccStatus[1];
	}

	public class AutoActuateBuilderToggle : VanillaBuilderToggle
	{
		public override bool Active() => Main.player[Main.myPlayer].autoActuator;
		public override int CurrentState => Main.player[Main.myPlayer].builderAccStatus[2];
	}

	public class AutoPaintBuilderToggle : VanillaBuilderToggle
	{
		public override bool Active() => Main.player[Main.myPlayer].autoPaint;
		public override int CurrentState => Main.player[Main.myPlayer].builderAccStatus[3];
	}

	[Autoload(false)]
	public abstract class WireVisibilityBuilderToggle : VanillaBuilderToggle
	{
		public override int NumberOfStates => 3;
		public override bool Active() => Main.player[Main.myPlayer].InfoAccMechShowWires;
	}

	public class RedWireVisibilityBuilderToggle : WireVisibilityBuilderToggle
	{
		public override int CurrentState => Main.player[Main.myPlayer].builderAccStatus[4];
	}

	public class BlueWireVisibilityBuilderToggle : WireVisibilityBuilderToggle
	{
		public override int CurrentState => Main.player[Main.myPlayer].builderAccStatus[5];
	}

	public class GreenWireVisibilityBuilderToggle : WireVisibilityBuilderToggle
	{
		public override int CurrentState => Main.player[Main.myPlayer].builderAccStatus[6];
	}

	public class YellowWireVisibilityBuilderToggle : WireVisibilityBuilderToggle
	{
		public override int CurrentState => Main.player[Main.myPlayer].builderAccStatus[7];
	}

	public class HideAllWiresBuilderToggle : WireVisibilityBuilderToggle
	{
		public override int NumberOfStates => 2;
		public override int CurrentState => Main.player[Main.myPlayer].builderAccStatus[8];
	}

	public class ActuatorsVisibilityBuilderToggle : WireVisibilityBuilderToggle
	{
		public override int CurrentState => Main.player[Main.myPlayer].builderAccStatus[9];
	}

	public class BlockSwapBuilderToggle : VanillaBuilderToggle
	{
		public override bool Active() => true;
		public override int CurrentState => Main.player[Main.myPlayer].builderAccStatus[10];
	}

	public class TorchBiomeBuilderToggle : VanillaBuilderToggle
	{
		public override bool Active() => Main.player[Main.myPlayer].unlockedBiomeTorches;
		public override int CurrentState => Main.player[Main.myPlayer].builderAccStatus[11];
	}
}