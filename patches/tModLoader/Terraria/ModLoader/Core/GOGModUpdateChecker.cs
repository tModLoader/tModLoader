using Ionic.Zlib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Terraria.ModLoader.Core
{
	internal static class GOGModUpdateChecker
	{
		public static int ModUpdatesAvailable = 0;

		internal static void CheckModUpdates() {
			ModUpdatesAvailable = 0;

			try {
				Task.Factory.StartNew(() => {
					ServicePointManager.Expect100Continue = false;

					const string Url = "http://javid.ddns.net/tModLoader/listmods.php";

					var values = new NameValueCollection {
						{ "modloaderversion", BuildInfo.versionedName },
						{ "platform", ModLoader.CompressedPlatformRepresentation },
						{ "netversion", FrameworkVersion.Version.ToString() },
						{ "EarlyAutoUpdate", UI.ModBrowser.UIModBrowser.EarlyAutoUpdate.ToString() }
					};

					using var client = new WebClient();

					ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, policyErrors) => true;
					client.UploadValuesCompleted += UpdateCheckComplete;

					client.UploadValuesAsync(new Uri(Url), "POST", values);
				});
			}
			catch (Exception e) {
				// Fail silently, it doesn't really matter if the update indicator is missing
				Logging.tML.Debug("Error checking for mod updates: " + e.Message);
			}
		}

		private static void UpdateCheckComplete(object sender, UploadValuesCompletedEventArgs e) {
			if (e.Error != null) {
				// Fail silently, it doesn't really matter if the update indicator is missing
				Logging.tML.Debug("Error checking for mod updates.");
			}
			else if (!e.Cancelled) {
				var result = e.Result;
				string response = Encoding.UTF8.GetString(result);
				Task.Factory
					.StartNew(ModOrganizer.FindMods)
					.ContinueWith(task => {
						var installedMods = task.Result;

						JObject jsonObject;
						try {
							jsonObject = JObject.Parse(response);
						}
						catch (Exception e) {
							throw new Exception($"Bad JSON: {response}", e);
						}

						JArray modlist;
						string modlist_compressed = (string)jsonObject["modlist_compressed"];
						if (modlist_compressed != null) {
							byte[] data = Convert.FromBase64String(modlist_compressed);
							using var zip = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
							using var reader = new StreamReader(zip);

							modlist = JArray.Parse(reader.ReadToEnd());
						}
						else {
							// Fallback if needed.
							modlist = (JArray)jsonObject["modlist"];
						}

						// Iterate through all mods, and increment modUpdatesAvailable where a mod
						// has a higher available version than the installed version
						foreach (var mod in modlist.Children<JObject>()) {
							string version = (string)mod["version"];
							var installed = installedMods.FirstOrDefault(m => m.Name == (string)mod["name"]);

							if (installed != null) {
								var cVersion = new Version(version.Substring(1));
								if (cVersion > installed.modFile.Version) {
									ModUpdatesAvailable++;
								}
							}
						}
					}, TaskScheduler.Current);
			}
		}

	}
}
