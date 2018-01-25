using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.ModLoader.IO;
using Terraria.UI;
using System.Linq;
using Terraria.ID;

namespace Terraria.ModLoader.UI
{
	internal class UIModPackItem : UIPanel
	{
		// Name -- > x in list disabled
		// Super List             5 Total, 3 Loaded, 1 Disabled, 1 Missing
		// Enable Only this List, Add mods to enabled, See mods in list

		// X mods, 3 enabled, 2 disabled.      Enable only, Add Mods
		// More info? see list of mods.
		// user will reload if needed (added)

		// TODO update this list button.

		private readonly Texture2D dividerTexture;
		private readonly Texture2D innerPanelTexture;
		private readonly UIText modName;
		private readonly string[] mods;
		private readonly bool[] modMissing;
		private readonly int numMods;
		private readonly int numModsEnabled;
		private readonly int numModsDisabled;
		private readonly int numModsMissing;
		readonly UITextPanel<string> enableListButton;
		readonly UITextPanel<string> enableListOnlyButton;
		readonly UITextPanel<string> viewInModBrowserButton;
		private readonly UIImageButton deleteButton;
		private readonly string filename;

		public UIModPackItem(string name, string[] mods)
		{
			this.filename = name;
			this.mods = mods;
			this.numMods = mods.Length;
			modMissing = new bool[mods.Length];
			numModsEnabled = 0;
			numModsDisabled = 0;
			numModsMissing = 0;
			for (int i = 0; i < mods.Length; i++)
			{
				if (UIModPacks.mods.Contains(mods[i]))
				{
					if (ModLoader.IsEnabled(mods[i]))
					{
						numModsEnabled++;
					}
					else
					{
						numModsDisabled++;
					}
				}
				else
				{
					modMissing[i] = true;
					numModsMissing++;
				}
			}

			this.BorderColor = new Color(89, 116, 213) * 0.7f;
			this.dividerTexture = TextureManager.Load("Images/UI/Divider");
			this.innerPanelTexture = TextureManager.Load("Images/UI/InnerPanelBackground");
			this.Height.Set(126f, 0f);
			this.Width.Set(0f, 1f);
			base.SetPadding(6f);

			this.modName = new UIText(name, 1f, false);
			this.modName.Left.Set(10f, 0f);
			this.modName.Top.Set(5f, 0f);
			base.Append(this.modName);

			UITextPanel<string> viewListButton = new UITextPanel<string>("View List", 1f, false);
			viewListButton.Width.Set(100f, 0f);
			viewListButton.Height.Set(30f, 0f);
			viewListButton.Left.Set(430f, 0f);
			viewListButton.Top.Set(40f, 0f);
			viewListButton.PaddingTop -= 2f;
			viewListButton.PaddingBottom -= 2f;
			viewListButton.OnMouseOver += UICommon.FadedMouseOver;
			viewListButton.OnMouseOut += UICommon.FadedMouseOut;
			viewListButton.OnClick += ViewListInfo;
			base.Append(viewListButton);

			enableListButton = new UITextPanel<string>("Enable this List", 1f, false);
			enableListButton.Width.Set(100f, 0f);
			enableListButton.Height.Set(30f, 0f);
			enableListButton.Left.Set(273f, 0f);
			enableListButton.Top.Set(40f, 0f);
			enableListButton.PaddingTop -= 2f;
			enableListButton.PaddingBottom -= 2f;
			enableListButton.OnMouseOver += UICommon.FadedMouseOver;
			enableListButton.OnMouseOut += UICommon.FadedMouseOut;
			enableListButton.OnClick += EnableList;
			base.Append(enableListButton);

			enableListOnlyButton = new UITextPanel<string>("Enable only this List", 1f, false);
			enableListOnlyButton.Width.Set(100f, 0f);
			enableListOnlyButton.Height.Set(30f, 0f);
			enableListOnlyButton.Left.Set(75f, 0f);
			enableListOnlyButton.Top.Set(40f, 0f);
			enableListOnlyButton.PaddingTop -= 2f;
			enableListOnlyButton.PaddingBottom -= 2f;
			enableListOnlyButton.OnMouseOver += UICommon.FadedMouseOver;
			enableListOnlyButton.OnMouseOut += UICommon.FadedMouseOut;
			enableListOnlyButton.OnClick += EnableListOnly;
			base.Append(enableListOnlyButton);

			viewInModBrowserButton = new UITextPanel<string>("View Mods in Mod Browser", 1f, false);
			viewInModBrowserButton.Width.Set(50f, 0f);
			viewInModBrowserButton.Height.Set(30f, 0f);
			viewInModBrowserButton.Left.Set(75f, 0f);
			viewInModBrowserButton.Top.Set(80f, 0f);
			viewInModBrowserButton.PaddingTop -= 2f;
			viewInModBrowserButton.PaddingBottom -= 2f;
			viewInModBrowserButton.OnMouseOver += UICommon.FadedMouseOver;
			viewInModBrowserButton.OnMouseOut += UICommon.FadedMouseOut;
			viewInModBrowserButton.OnClick += ViewInModBrowser;
			base.Append(viewInModBrowserButton);

			deleteButton = new UIImageButton(TextureManager.Load("Images/UI/ButtonDelete"));
			deleteButton.Top.Set(40f, 0f);
			deleteButton.OnClick += this.DeleteButtonClick;
			base.Append(deleteButton);
		}

		private void DrawPanel(SpriteBatch spriteBatch, Vector2 position, float width)
		{
			spriteBatch.Draw(this.innerPanelTexture, position, new Rectangle?(new Rectangle(0, 0, 8, this.innerPanelTexture.Height)), Color.White);
			spriteBatch.Draw(this.innerPanelTexture, new Vector2(position.X + 8f, position.Y), new Rectangle?(new Rectangle(8, 0, 8, this.innerPanelTexture.Height)), Color.White, 0f, Vector2.Zero, new Vector2((width - 16f) / 8f, 1f), SpriteEffects.None, 0f);
			spriteBatch.Draw(this.innerPanelTexture, new Vector2(position.X + width - 8f, position.Y), new Rectangle?(new Rectangle(16, 0, 8, this.innerPanelTexture.Height)), Color.White);
		}

		private void DrawEnabledText(SpriteBatch spriteBatch, Vector2 drawPos)
		{
			string text = $"{numMods} Total, {numModsEnabled} Enabled, {numModsDisabled} Disabled, {numModsMissing} Missing";
			Color color = (numModsMissing > 0 ? Color.Red : (numModsDisabled > 0 ? Color.Yellow : Color.Green));

			Utils.DrawBorderString(spriteBatch, text, drawPos, color, 1f, 0f, 0f, -1);
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
			spriteBatch.Draw(this.dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);
			drawPos = new Vector2(innerDimensions.X + innerDimensions.Width - 355, innerDimensions.Y);
			this.DrawPanel(spriteBatch, drawPos, 350f);
			this.DrawEnabledText(spriteBatch, drawPos + new Vector2(15f, 5f));
			//if (this.enabled != ModLoader.ModLoaded(mod.name))
			//{
			//	drawPos += new Vector2(120f, 5f);
			//	Utils.DrawBorderString(spriteBatch, "Reload Required", drawPos, Color.White, 1f, 0f, 0f, -1);
			//}
			//string text = this.enabled ? "Click to Disable" : "Click to Enable";
			//drawPos = new Vector2(innerDimensions.X + innerDimensions.Width - 150f, innerDimensions.Y + 50f);
			//Utils.DrawBorderString(spriteBatch, text, drawPos, Color.White, 1f, 0f, 0f, -1);
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			base.MouseOver(evt);
			this.BackgroundColor = new Color(73, 94, 171);
			this.BorderColor = new Color(89, 116, 213);
		}

		public override void MouseOut(UIMouseEvent evt)
		{
			base.MouseOut(evt);
			this.BackgroundColor = new Color(63, 82, 151) * 0.7f;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
		}

		private void DeleteButtonClick(UIMouseEvent evt, UIElement listeningElement)
		{
			UIModPackItem modPackItem = ((UIModPackItem)listeningElement.Parent);
			Directory.CreateDirectory(UIModPacks.ModListSaveDirectory);
			string path = UIModPacks.ModListSaveDirectory + Path.DirectorySeparatorChar + modPackItem.filename + ".json";
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			Main.menuMode = Interface.modPacksMenuID;// should reload
		}

		private static void EnableList(UIMouseEvent evt, UIElement listeningElement)
		{
			UIModPackItem modListItem = (UIModPackItem)listeningElement.Parent;
			foreach (string modname in modListItem.mods)
			{
				if (UIModPacks.mods.Contains(modname))
					ModLoader.EnableMod(modname);
			}
			Main.menuMode = Interface.modPacksMenuID; // should reload, which should refresh enabled counts

			if (modListItem.numModsMissing > 0)
			{
				string missing = "The following mods were not found:\n";
				for (int i = 0; i < modListItem.mods.Length; i++)
				{
					if (modListItem.modMissing[i])
					{
						missing += modListItem.mods[i] + "\n";
					}
				}
				Interface.infoMessage.SetMessage(missing);
				Interface.infoMessage.SetGotoMenu(Interface.modPacksMenuID);
				Main.menuMode = Interface.infoMessageID;
			}
		}

		private static void ViewInModBrowser(UIMouseEvent evt, UIElement listeningElement)
		{
			UIModPackItem modListItem = ((UIModPackItem)listeningElement.Parent);
			Interface.modBrowser.Activate();
			Interface.modBrowser.filterTextBox.currentString = "";
			Interface.modBrowser.SpecialModPackFilter = modListItem.mods.ToList();
			Interface.modBrowser.SpecialModPackFilterTitle = "Modlist";// Too long: " + modListItem.modName.Text;
			Interface.modBrowser.updateFilterMode = UpdateFilter.All; // Set to 'All' so all mods from ModPack are visible
			Interface.modBrowser.uIToggleImage.setCurrentState((int)Interface.modBrowser.updateFilterMode);
			Interface.modBrowser.updateNeeded = true;
			Main.PlaySound(SoundID.MenuOpen);
			Main.menuMode = Interface.modBrowserID;
		}

		private static void EnableListOnly(UIMouseEvent evt, UIElement listeningElement)
		{
			UIModPackItem modListItem = (UIModPackItem)listeningElement.Parent;
			foreach (var item in UIModPacks.mods)
			{
				ModLoader.DisableMod(item);
			}
			foreach (string modname in modListItem.mods)
			{
				if (UIModPacks.mods.Contains(modname))
					ModLoader.EnableMod(modname);
			}
			Main.menuMode = Interface.reloadModsID; // should reload, which should refresh enabled counts

			if (modListItem.numModsMissing > 0)
			{
				string missing = "The following mods were not found:\n";
				for (int i = 0; i < modListItem.mods.Length; i++)
				{
					if (modListItem.modMissing[i])
					{
						missing += modListItem.mods[i] + "\n";
					}
				}
				Interface.infoMessage.SetMessage(missing);
				Interface.infoMessage.SetGotoMenu(Interface.reloadModsID);
				Main.menuMode = Interface.infoMessageID;
			}
		}

		private static void ViewListInfo(UIMouseEvent evt, UIElement listeningElement)
		{
			UIModPackItem modListItem = ((UIModPackItem)listeningElement.Parent);
			Main.PlaySound(10, -1, -1, 1);
			string message = "This list contains the following mods:\n";
			for (int i = 0; i < modListItem.mods.Length; i++)
			{
				message += modListItem.mods[i] + (modListItem.modMissing[i] ? " - Missing" : "") + "\n";
			}
			//Interface.infoMessage.SetMessage($"This list contains the following mods:\n{String.Join("\n", ((UIModListItem)listeningElement.Parent).mods)}");
			Interface.infoMessage.SetMessage(message);
			Interface.infoMessage.SetGotoMenu(Interface.modPacksMenuID);
			Main.menuMode = Interface.infoMessageID;
		}
	}
}
