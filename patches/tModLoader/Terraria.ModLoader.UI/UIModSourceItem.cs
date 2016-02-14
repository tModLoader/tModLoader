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

namespace Terraria.ModLoader.UI
{
	internal class UIModSourceItem : UIPanel
	{
		private string mod;
		private Texture2D dividerTexture;
		private UIText modName;

		public UIModSourceItem(string mod, bool publishable)
		{
			this.mod = mod;
			this.BorderColor = new Color(89, 116, 213) * 0.7f;
			this.dividerTexture = TextureManager.Load("Images/UI/Divider");
			this.Height.Set(90f, 0f);
			this.Width.Set(0f, 1f);
			base.SetPadding(6f);
			this.modName = new UIText(Path.GetFileName(mod), 1f, false);
			this.modName.Left.Set(10f, 0f);
			this.modName.Top.Set(5f, 0f);
			base.Append(this.modName);
			UITextPanel button = new UITextPanel("Build", 1f, false);
			button.Width.Set(100f, 0f);
			button.Height.Set(30f, 0f);
			button.Left.Set(10f, 0f);
			button.Top.Set(40f, 0f);
			button.PaddingTop -= 2f;
			button.PaddingBottom -= 2f;
			button.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button.OnClick += new UIElement.MouseEvent(this.BuildMod);
			base.Append(button);
			UITextPanel button2 = new UITextPanel("Build + Reload", 1f, false);
			button2.CopyStyle(button);
			button2.Width.Set(200f, 0f);
			button2.Left.Set(150f, 0f);
			button2.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
			button2.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
			button2.OnClick += new UIElement.MouseEvent(this.BuildAndReload);
			base.Append(button2);
			if (publishable)
			{
				UITextPanel button3 = new UITextPanel("Publish", 1f, false);
				button3.CopyStyle(button2);
				button3.Width.Set(100f, 0f);
				button3.Left.Set(390f, 0f);
				button3.OnMouseOver += new UIElement.MouseEvent(FadedMouseOver);
				button3.OnMouseOut += new UIElement.MouseEvent(FadedMouseOut);
				button3.OnClick += new UIElement.MouseEvent(this.Publish);
				base.Append(button3);
			}
			base.OnDoubleClick += new UIElement.MouseEvent(this.BuildAndReload);
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

		private static void FadedMouseOver(UIMouseEvent evt, UIElement listeningElement)
		{
			Main.PlaySound(12, -1, -1, 1);
			((UIPanel)evt.Target).BackgroundColor = new Color(73, 94, 171);
		}

		private static void FadedMouseOut(UIMouseEvent evt, UIElement listeningElement)
		{
			((UIPanel)evt.Target).BackgroundColor = new Color(63, 82, 151) * 0.7f;
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
			Main.PlaySound(10, -1, -1, 1);
			try
			{
				TmodFile[] modFiles = ModLoader.FindMods();
				bool ok = false;
				TmodFile theTModFile = null;
				foreach (TmodFile tModFile in modFiles)
				{
					if (Path.GetFileName(tModFile.Name).Equals(@Path.GetFileName(mod) + @".tmod"))
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
				using (var stream = File.Open(filename, FileMode.Open))
				{
					var files = new[]
					{
						new IO.UploadFile
						{
							Name = "file",
							Filename = Path.GetFileName(filename),
							//    ContentType = "text/plain",
							Stream = stream
						}
					};
					BuildProperties bp = BuildProperties.ReadModFile(theTModFile);
					var values = new NameValueCollection
					{
						{ "displayname", bp.displayName },
						{ "name", Path.GetFileNameWithoutExtension(filename) },
						{ "version", bp.version },
						{ "author", bp.author },
						{ "homepage", bp.homepage },
						{ "description", bp.description },
						{ "steamid64", Steamworks.SteamUser.GetSteamID().ToString() },
						{ "modloaderversion", bp.modBuildVersion },
					};
					byte[] result = IO.UploadFile.UploadFiles(url, files, values);
					string s = System.Text.Encoding.UTF8.GetString(result, 0, result.Length);
					ErrorLogger.LogModPublish(s);
				}
			}
			catch (WebException e)
			{
				ErrorLogger.LogModBrowserException(e);
			}
		}
	}
}
