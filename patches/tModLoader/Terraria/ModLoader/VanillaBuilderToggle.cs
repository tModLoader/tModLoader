﻿using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria.GameContent;
using Terraria.Localization;

namespace Terraria.ModLoader;

[Autoload(false)]
public abstract class VanillaBuilderToggle : BuilderToggle
{
	public override string Texture => "Terraria/Images/UI/BuilderIcons";
	public override int NumberOfStates => 2;
	public override string DisplayValue() => "";

	public override Color DisplayColorTexture() {
		return CurrentState == 0 ? Color.White : new Color(127, 127, 127);
	}
}

public class RulerLineBuilderToggle : VanillaBuilderToggle
{
	public override bool Active() => Main.player[Main.myPlayer].rulerLine;

	public override string DisplayValue() {
		string text = "";
		switch (CurrentState) {
			case 0:
				text = Language.GetTextValue("GameUI.RulerOn");
				break;
			case 1:
				text = Language.GetTextValue("GameUI.RulerOff");
				break;
		}

		return text;
	}
}

public class RulerGridBuilderToggle : VanillaBuilderToggle
{
	public override bool Active() => Main.player[Main.myPlayer].rulerGrid;

	public override string DisplayValue() {
		string text = "";
		switch (CurrentState) {
			case 0:
				text = Language.GetTextValue("GameUI.MechanicalRulerOn");
				break;
			case 1:
				text = Language.GetTextValue("GameUI.MechanicalRulerOff");
				break;
		}

		return text;
	}
}

public class AutoActuateBuilderToggle : VanillaBuilderToggle
{
	public override bool Active() => Main.player[Main.myPlayer].autoActuator;

	public override string DisplayValue() {
		string text = "";
		switch (CurrentState) {
			case 0:
				text = Language.GetTextValue("GameUI.ActuationDeviceOn");
				break;
			case 1:
				text = Language.GetTextValue("GameUI.ActuationDeviceOff");
				break;
		}

		return text;
	}
}

public class AutoPaintBuilderToggle : VanillaBuilderToggle
{
	public override bool Active() => Main.player[Main.myPlayer].autoPaint;

	public override string DisplayValue() {
		string text = "";
		switch (CurrentState) {
			case 0:
				text = Language.GetTextValue("GameUI.PaintSprayerOn");
				break;
			case 1:
				text = Language.GetTextValue("GameUI.PaintSprayerOff");
				break;
		}

		return text;
	}
}

[Autoload(false)]
public abstract class WireVisibilityBuilderToggle : VanillaBuilderToggle
{
	public override int NumberOfStates => 3;
	public override bool Active() => Main.player[Main.myPlayer].InfoAccMechShowWires;

	public override string DisplayValue() {
		string text = "";
		switch (Type) {
			case 4:
				text = Language.GetTextValue("Game.RedWires");
				break;
			case 5:
				text = Language.GetTextValue("Game.BlueWires");
				break;
			case 6:
				text = Language.GetTextValue("Game.GreenWires");
				break;
			case 7:
				text = Language.GetTextValue("Game.YellowWires");
				break;
			case 9:
				text = Language.GetTextValue("Game.Actuators");
				break;
		}

		string text2 = "";
		switch (CurrentState) {
			case 0:
				text2 = Language.GetTextValue("GameUI.Bright");
				break;
			case 1:
				text2 = Language.GetTextValue("GameUI.Normal");
				break;
			case 2:
				text2 = Language.GetTextValue("GameUI.Faded");
				break;
			case 3: //Should never reach here but vanilla has it
				Language.GetTextValue("GameUI.Hidden");
				break;
		}

		return $"{text}: {text2}";
	}

	public override Color DisplayColorTexture() {
		Color color = default;
		switch (CurrentState) {
			case 0:
				color = Color.White;
				break;
			case 1:
				color = new Color(127, 127, 127);
				break;
			case 2:
				color = new Color(127, 127, 127).MultiplyRGBA(new Color(0.66f, 0.66f, 0.66f));
				break;
			case 3: //Should never reach here but vanilla has it
				color = new Color(127, 127, 127).MultiplyRGBA(new Color(0.33f, 0.33f, 0.33f));
				break;
		}

		return color;
	}
}

public class RedWireVisibilityBuilderToggle : WireVisibilityBuilderToggle { }

public class BlueWireVisibilityBuilderToggle : WireVisibilityBuilderToggle { }

public class GreenWireVisibilityBuilderToggle : WireVisibilityBuilderToggle { }

public class YellowWireVisibilityBuilderToggle : WireVisibilityBuilderToggle { }

public class HideAllWiresBuilderToggle : WireVisibilityBuilderToggle
{
	public override int NumberOfStates => 2;

	public override string DisplayValue() {
		string text = "";
		switch (CurrentState) {
			case 0:
				text = Language.GetTextValue("GameUI.WireModeForced");
				break;
			case 1:
				text = Language.GetTextValue("GameUI.WireModeNormal");
				break;
		}

		return text;
	}
}

public class ActuatorsVisibilityBuilderToggle : WireVisibilityBuilderToggle { }

public class BlockSwapBuilderToggle : VanillaBuilderToggle
{
	public override string Texture => "Terraria/Images/UI/BlockReplace_0";
	public override bool Active() => true;

	public override string DisplayValue() {
		string text = "";
		switch (CurrentState) {
			case 0:
				text = Language.GetTextValue("GameUI.BlockReplacerOn");
				break;
			case 1:
				text = Language.GetTextValue("GameUI.BlockReplacerOff");
				break;
		}

		return text;
	}

	public override Color DisplayColorTexture() => Color.White;
}

public class TorchBiomeBuilderToggle : VanillaBuilderToggle
{
	public override string Texture => "Terraria/Images/Extra_211";
	public override bool Active() => Main.player[Main.myPlayer].unlockedBiomeTorches;

	public override string DisplayValue() {
		string text = "";
		switch (CurrentState) {
			case 0:
				text = Language.GetTextValue("GameUI.TorchTypeSwapperOn");
				break;
			case 1:
				text = Language.GetTextValue("GameUI.TorchTypeSwapperOff");
				break;
		}

		return text;
	}

	public override Color DisplayColorTexture() => Color.White;
}