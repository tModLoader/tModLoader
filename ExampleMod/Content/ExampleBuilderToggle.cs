using Terraria.ModLoader;

namespace ExampleMod.Content
{
	public class ExampleBuilderToggle : BuilderToggle
	{
		public override string Texture => "ExampleMod/Content/ExampleInfoDisplay";
		public override int NumberOfStates => 4;

		public override void SetDefaults() {

		}

		public override bool Active() => true;

		public override string DisplayValue() {
			string text = "";

			switch (CurrentState) {
				case 0:
					text = "Some random text";
					break;
				case 1:
					text = "Yup, still no ideas";
					break;
				case 2:
					text = "Hold on, how many states are there??";
					break;
				case 3:
					text = "Ahah, we better stop now";
					break;
			}

			return text;
		}
	}

	public class ExampleBuilderToggle2 : ExampleBuilderToggle { }
	public class ExampleBuilderToggle3 : ExampleBuilderToggle { }
	public class ExampleBuilderToggle4 : ExampleBuilderToggle { }
	public class ExampleBuilderToggle5 : ExampleBuilderToggle { }
	public class ExampleBuilderToggle6 : ExampleBuilderToggle { }

	// public class GlobalToggle : GlobalBuilderToggle
	// {
	// 	public override void ModifyNumberOfStates(BuilderToggle builderToggle, ref int numberOfStates) {
	// 		if (builderToggle == BuilderToggle.RulerGrid) {
	// 			numberOfStates = 3;
	// 		}
	// 	}
	//
	// 	public override void ModifyDisplayValue(BuilderToggle builderToggle, ref string displayValue) {
	// 		if (builderToggle == BuilderToggle.RulerGrid) {
	// 			if (builderToggle.CurrentState == 2) {
	// 				displayValue = "This is a third new state!";
	// 			}
	// 			else
	// 				displayValue = $"Yeah okay {builderToggle.CurrentState}";
	// 		}
	// 	}
	// }
}