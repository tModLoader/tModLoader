using System;
using System.Diagnostics;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Terraria.ModLoader.UI
{
	public class UIWorldLoadCrash:UIState
	{
		public UIWorldLoadCrash(string message)
		{
			var area = new UIElement
			{
				Width	= {Percent	= 0.8f					},
				Top		= {Pixels	= +200					},
				Height	= {Pixels	= -210,	Percent = 1.0f	},
				HAlign	= 0.5f
			};

			var messageBox = new UIMessageBox(message)
			{
				Width	= {Percent	= 1.0f					},
				Height	= {Pixels	= -110,	Percent = 1.0f	},
				HAlign	= 0.5f
			};
			area.Append(messageBox);

			var backButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.BackToMainMenu"), 0.7f, true)
			{
				Width	= {Pixels	= -10,	Percent = 0.5f	},
				Height	= {Pixels	= +50					},
				Top		= {Pixels	= -108,	Percent = 1.0f	}
			};
			backButton.WithFadedMouseOver();
			backButton.OnClick += (UIMouseEvent evt, UIElement listeningElement)=>
			{
				Main.PlaySound(SoundID.MenuClose);
				Main.menuMode = 0;
			};
			area.Append(backButton);

			var openLogsButton = new UITextPanel<string>(Language.GetTextValue("tModLoader.OpenLogs"), 0.7f, true);
			openLogsButton.CopyStyle(backButton);
			openLogsButton.HAlign = 1f;
			openLogsButton.WithFadedMouseOver();
			openLogsButton.OnClick += (UIMouseEvent evt, UIElement listeningElement)=>
			{
				Main.PlaySound(SoundID.MenuOpen);
				Process.Start(Logging.LogPath);
			};
			area.Append(openLogsButton);

			Append(area);
		}
	}
}
