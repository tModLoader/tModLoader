using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent.UI.Elements;
using Terraria.Graphics;
using Terraria.UI;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using Terraria.ModLoader.IO;
using System.Linq;
using Terraria.Localization;

namespace Terraria.ModLoader.UI
{
	internal class UIModSourceItem : UIPanel
	{
		private string mod;
		private Texture2D dividerTexture;
		private UIText modName;
		private DateTime lastBuildTime;

		public UIModSourceItem(string mod, bool publishable, DateTime lastBuildTime)
		{
			this.mod = mod;
			this.lastBuildTime = lastBuildTime;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
			this.dividerTexture = TextureManager.Load("Images/UI/Divider");
			this.Height.Set(90f, 0f);
			this.Width.Set(0f, 1f);
			base.SetPadding(6f);
			this.modName = new UIText(Path.GetFileName(mod), 1f, false);
			this.modName.Left.Set(10f, 0f);
			this.modName.Top.Set(5f, 0f);
			base.Append(this.modName);
			UITextPanel<string> button = new UITextPanel<string>(Language.GetTextValue("tModLoader.MSBuild"), 1f, false);
			button.Width.Set(100f, 0f);
			button.Height.Set(30f, 0f);
			button.Left.Set(10f, 0f);
			button.Top.Set(40f, 0f);
			button.PaddingTop -= 2f;
			button.PaddingBottom -= 2f;
			button.OnMouseOver += UICommon.FadedMouseOver;
			button.OnMouseOut += UICommon.FadedMouseOut;
			button.OnClick += this.BuildMod;
			base.Append(button);
			UITextPanel<string> button2 = new UITextPanel<string>(Language.GetTextValue("tModLoader.MSBuildReload"), 1f, false);
			button2.CopyStyle(button);
			button2.Width.Set(200f, 0f);
			button2.Left.Set(150f, 0f);
			button2.OnMouseOver += UICommon.FadedMouseOver;
			button2.OnMouseOut += UICommon.FadedMouseOut;
			button2.OnClick += this.BuildAndReload;
			base.Append(button2);
			if (publishable)
			{
				UITextPanel<string> button3 = new UITextPanel<string>(Language.GetTextValue("tModLoader.MSPublish"), 1f, false);
				button3.CopyStyle(button2);
				button3.Width.Set(100f, 0f);
				button3.Left.Set(390f, 0f);
				button3.OnMouseOver += UICommon.FadedMouseOver;
				button3.OnMouseOut += UICommon.FadedMouseOut;
				button3.OnClick += this.Publish;
				base.Append(button3);
			}
			base.OnDoubleClick += this.BuildAndReload;
		}

		protected override void DrawSelf(SpriteBatch spriteBatch)
		{
			base.DrawSelf(spriteBatch);
			CalculatedStyle innerDimensions = base.GetInnerDimensions();
			Vector2 drawPos = new Vector2(innerDimensions.X + 5f, innerDimensions.Y + 30f);
			spriteBatch.Draw(this.dividerTexture, drawPos, null, Color.White, 0f, Vector2.Zero, new Vector2((innerDimensions.Width - 10f) / 8f, 1f), SpriteEffects.None, 0f);
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

		public override int CompareTo(object obj)
		{
			UIModSourceItem uIModSourceItem = obj as UIModSourceItem;
			if (uIModSourceItem == null)
			{
				return base.CompareTo(obj);
			}
			return uIModSourceItem.lastBuildTime.CompareTo(lastBuildTime);
		}

		private void BuildMod(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			ModLoader.modToBuild = this.mod;
			ModLoader.reloadAfterBuild = false;
			ModLoader.buildAll = false;
			Main.menuMode = Interface.buildModID;
		}

		private void BuildAndReload(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(10, -1, -1, 1);
			ModLoader.modToBuild = this.mod;
			ModLoader.reloadAfterBuild = true;
			ModLoader.buildAll = false;
			Main.menuMode = Interface.buildModID;
		}

		private void Publish(UIMouseEvent evt, UIElement listeningElement)
		{
			if (ModLoader.modBrowserPassphrase == "")
			{
				Main.menuMode = Interface.enterPassphraseMenuID;
				Interface.enterPassphraseMenu.SetGotoMenu(Interface.modSourcesID);
				return;
			}
			Main.PlaySound(10, -1, -1, 1);
			try
			{
				TmodFile[] modFiles = ModLoader.FindMods();
				bool ok = false;
				TmodFile theTModFile = null;
				foreach (TmodFile tModFile in modFiles)
				{
					if (Path.GetFileName(tModFile.path).Equals(@Path.GetFileName(mod) + @".tmod"))
					{
						ok = true;
						theTModFile = tModFile;
					}
				}
				if (!ok)
				{
					throw new Exception();
				}
				System.Net.ServicePointManager.Expect100Continue = false;
				string filename = @ModLoader.ModPath + @Path.DirectorySeparatorChar + @Path.GetFileName(mod) + @".tmod";
				string url = "http://javid.ddns.net/tModLoader/publishmod.php";
				byte[] result;
				using (var iconStream = theTModFile.HasFile("icon.png") ? new MemoryStream(theTModFile.GetFile("icon.png")) : null)
				using (var stream = File.Open(filename, FileMode.Open))
				{
					var files = new List<UploadFile>();
					files.Add(new IO.UploadFile
						{
							Name = "file",
							Filename = Path.GetFileName(filename),
							//    ContentType = "text/plain",
							Stream = stream
						}
					);
					if(iconStream != null)
					{
						files.Add(new IO.UploadFile
							{
								Name = "iconfile",
								Filename = "icon.png",
								Stream = iconStream
							}
						);
					}
					BuildProperties bp = BuildProperties.ReadModFile(theTModFile);
					var values = new NameValueCollection
					{
						{ "displayname", bp.displayName },
						{ "name", Path.GetFileNameWithoutExtension(filename) },
						{ "version", "v"+bp.version },
						{ "author", bp.author },
						{ "homepage", bp.homepage },
						{ "description", bp.description },
						{ "steamid64", ModLoader.SteamID64 },
						{ "modloaderversion", "tModLoader v"+theTModFile.tModLoaderVersion },
						{ "passphrase", ModLoader.modBrowserPassphrase },
						{ "modreferences", String.Join(", ", bp.modReferences.Select(x => x.mod)) },
						{ "modside", bp.side.ToFriendlyString() },
					};
					result = IO.UploadFile.UploadFiles(url, files, values);
				}
				int responseLength = result.Length;
				if (result.Length > 256 && result[result.Length - 256 - 1] == '~')
				{
					Array.Copy(result, result.Length - 256, theTModFile.signature, 0, 256);
					theTModFile.Save();
					responseLength -= 257;
				}
				string s = System.Text.Encoding.UTF8.GetString(result, 0, responseLength);
				ErrorLogger.LogModPublish(s);
			}
			catch (WebException e)
			{
				ErrorLogger.LogModBrowserException(e);
			}
		}
	}
}
