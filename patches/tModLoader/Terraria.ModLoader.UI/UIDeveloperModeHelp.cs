using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	internal class UIDeveloperModeHelp : UIState
	{
		//private UIList missingRequirements;
		private UIPanel backPanel;
		private UIElement modCompilePanel;
		private UIElement dotnet46Panel;
		private UIElement assembliesPanel;
		private UIElement tutorialPanel;

		public override void OnInitialize()
		{
			UIElement area = new UIElement();
			area.Width.Set(0f, 0.8f);
			area.Top.Set(200f, 0f);
			area.Height.Set(-240f, 1f);
			area.HAlign = 0.5f;

			backPanel = new UIPanel();
			backPanel.Width.Set(0f, 1f);
			backPanel.Height.Set(-90f, 1f);
			backPanel.BackgroundColor = new Color(33, 43, 79) * 0.8f;
			area.Append(backPanel);

			//missingRequirements = new UIList();
			//missingRequirements.Width.Set(-25f, 1f);
			//missingRequirements.Height.Set(0f, 1f);
			//missingRequirements.ListPadding = 5f;
			//backPanel.Append(missingRequirements);

			//UIScrollbar missingRequirementsScrollbar = new UIScrollbar();
			//missingRequirementsScrollbar.SetView(100f, 1000f);
			//missingRequirementsScrollbar.Height.Set(-20, 1f);
			//missingRequirementsScrollbar.VAlign = 0.5f;
			//missingRequirementsScrollbar.HAlign = 1f;
			//backPanel.Append(missingRequirementsScrollbar);
			//missingRequirements.SetScrollbar(missingRequirementsScrollbar);

			UITextPanel<string> heading = new UITextPanel<string>(Language.GetTextValue("tModLoader.MenuEnableDeveloperMode"), 0.8f, true);
			heading.HAlign = 0.5f;
			heading.Top.Set(-45f, 0f);
			heading.SetPadding(15f);
			heading.BackgroundColor = new Color(73, 94, 171);
			area.Append(heading);

			UITextPanel<string> backButton = new UITextPanel<string>(Language.GetTextValue("UI.Back"), 0.7f, true);
			backButton.Width.Set(-10f, 0.5f);
			backButton.Height.Set(50f, 0f);
			backButton.Left.Set(0, 0f);
			backButton.VAlign = 1f;
			backButton.Top.Set(-30f, 0f);
			backButton.OnMouseOver += UICommon.FadedMouseOver;
			backButton.OnMouseOut += UICommon.FadedMouseOut;
			backButton.OnClick += BackClick;
			area.Append(backButton);

			UITextPanel<string> refreshButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.Retry"), 0.7f, true);
			refreshButton.Width.Set(-10f, 0.5f);
			refreshButton.Height.Set(50f, 0f);
			refreshButton.Left.Set(0, .5f);
			refreshButton.VAlign = 1f;
			refreshButton.Top.Set(-30f, 0f);
			refreshButton.OnMouseOver += UICommon.FadedMouseOver;
			refreshButton.OnMouseOut += UICommon.FadedMouseOut;
			refreshButton.OnClick += RefreshClick;
			area.Append(refreshButton);

			Append(area);
		}

		public override void OnActivate()
		{
			backPanel.RemoveAllChildren();

			Texture2D icon = Texture2D.FromStream(Main.instance.GraphicsDevice, Assembly.GetExecutingAssembly().GetManifestResourceStream("Terraria.ModLoader.UI.ButtonExclamation.png"));

			var modCompileMessage = new UIMessageBox("\n      " + Language.GetTextValue("tModLoader.EDMModCompileFolderNeededMessage"));
			modCompileMessage.Width.Set(0, .5f);
			modCompileMessage.Height.Set(0, .5f);
			modCompileMessage.OnDoubleClick += (a, b) =>
			{
				//System.Diagnostics.Process.Start("http://javid.ddns.net/tModLoader/DirectModDownloadListing.php");
			};
			modCompileMessage.SetScrollbar(new UIScrollbar());
			if (ModLoader.DotNetAssembliesCheck())
			{
				UIImage warningIcon = new UIImage(icon);
				warningIcon.Left.Set(5, 0f);
				warningIcon.Top.Set(5, 0f);
				modCompileMessage.Append(warningIcon);
			}
			backPanel.Append(modCompileMessage);

			var dotnet46Message = new UIMessageBox("\n      " + Language.GetTextValue("tModLoader.EDMDotNet46NeededMessage"));
			dotnet46Message.Width.Set(0, .5f);
			dotnet46Message.Height.Set(0, .5f);
			dotnet46Message.Top.Set(0, .5f);
			dotnet46Message.OnDoubleClick += (a, b) =>
			{
				//System.Diagnostics.Process.Start("http://javid.ddns.net/tModLoader/DirectModDownloadListing.php");
			};
			dotnet46Message.SetScrollbar(new UIScrollbar());
			if (ModLoader.DotNetAssembliesCheck())
			{
				UIImage warningIcon = new UIImage(icon);
				warningIcon.Left.Set(5, 0f);
				warningIcon.Top.Set(5, 0f);
				dotnet46Message.Append(warningIcon);
			}
			backPanel.Append(dotnet46Message);

			var assembliesMessage = new UIMessageBox("\n      " + Language.GetTextValue("tModLoader.EDMReferencesAssembliesNeededMessage"));
			assembliesMessage.Width.Set(0, .5f);
			assembliesMessage.Height.Set(0, .5f);
			assembliesMessage.Left.Set(0, .5f);
			assembliesMessage.OnDoubleClick += (a, b) =>
			{
				//System.Diagnostics.Process.Start("http://javid.ddns.net/tModLoader/DirectModDownloadListing.php");
			};
			assembliesMessage.SetScrollbar(new UIScrollbar());
			if (ModLoader.DotNetAssembliesCheck())
			{
				UIImage warningIcon = new UIImage(icon);
				warningIcon.Left.Set(5, 0f);
				warningIcon.Top.Set(5, 0f);
				assembliesMessage.Append(warningIcon);
			}
			backPanel.Append(assembliesMessage);

			var tutorialMessage = new UIMessageBox("\n      " + Language.GetTextValue("tModLoader.EDMTutorialNeededMessage"));
			tutorialMessage.Width.Set(0, .5f);
			tutorialMessage.Height.Set(0, .5f);
			tutorialMessage.Left.Set(0, .5f);
			tutorialMessage.Top.Set(0, .5f);
			tutorialMessage.OnDoubleClick += (a, b) =>
			{
				System.Diagnostics.Process.Start("https://github.com/blushiemagic/tModLoader/wiki/Basic-tModLoader-Modding-Guide");
			};
			tutorialMessage.SetScrollbar(new UIScrollbar());
			backPanel.Append(tutorialMessage);
		}

		private void BackClick(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(SoundID.MenuClose);
			Main.menuMode = 0;
		}

		private void RefreshClick(UIMouseEvent evt, UIElement listeningElement)
		{
			ModLoader.ResetDeveloperMode();
			Main.PlaySound(SoundID.MenuClose);
			if (ModLoader.DeveloperMode)
				Main.menuMode = 0;
			else
				Main.menuMode = Interface.developerModeHelpID;
		}
	}
}
