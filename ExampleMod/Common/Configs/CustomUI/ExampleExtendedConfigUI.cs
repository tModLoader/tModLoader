using Microsoft.Xna.Framework;
using Terraria.ModLoader.Config.UI;

namespace ExampleMod.Common.Configs.CustomUI
{
	/// <summary>
	/// This is a custom UIModConfig that we can modify.
	/// See ModConfigShowcaseMisc.cs to see how to set custom UI.
	/// </summary>
	public class ExampleExtendedConfigUI : UIModConfig
	{
		public override void OnActivate() {
			base.OnActivate();

			SetMessage("Hello from Example Mod!", Color.DeepPink);

			// Currently, not much can be modified, but more functionality will be added in the future.
		}
	}
}
