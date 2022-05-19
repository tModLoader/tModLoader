using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.Core;
using Terraria.Social.Steam;
using Terraria.UI.Chat;

namespace Terraria.ModLoader.UI.ModBrowser
{
	internal class ModDownloadItem
	{
		public readonly string ModName;
		public readonly string DisplayName;
		public readonly string DisplayNameClean; // No chat tags: for search and sort functionality.
		public readonly string PublishId;
		public readonly bool HasUpdate;
		public readonly bool UpdateIsDowngrade;
		public LocalMod Installed { get; internal set; }
		public readonly string Version;

		internal readonly string Author;
		internal readonly string ModIconUrl;
		internal ModIconStatus ModIconStatus = ModIconStatus.UNKNOWN;
		internal readonly DateTime TimeStamp;
		internal readonly string ModReferences;
		internal readonly ModSide ModSide;
		internal readonly int Downloads;
		internal readonly int Hot;
		internal readonly string Homepage;
		internal readonly string ModloaderVersion;

		private bool IsInstalled => Installed != null;

		public ModDownloadItem(string displayName, string name, string version, string author, string modReferences, ModSide modSide, string modIconUrl, string publishId, int downloads, int hot, DateTime timeStamp, bool hasUpdate, bool updateIsDowngrade, LocalMod installed, string modloaderversion, string homepage) {
			ModName = name;
			DisplayName = displayName;
			DisplayNameClean = string.Join("", ChatManager.ParseMessage(displayName, Color.White).Where(x => x.GetType() == typeof(TextSnippet)).Select(x => x.Text));
			PublishId = publishId;

			Author = author;
			ModReferences = modReferences;
			ModSide = modSide;
			ModIconUrl = modIconUrl;
			Downloads = downloads;
			Hot = hot;
			Homepage = homepage;
			TimeStamp = timeStamp;
			HasUpdate = hasUpdate;
			UpdateIsDowngrade = updateIsDowngrade;
			Installed = installed;
			Version = version;
			ModloaderVersion = modloaderversion;
		}

		internal void InnerDownloadWithDeps() {
			var downloads = new HashSet<ModDownloadItem>() { this };
			downloads.Add(this);
			GetDependenciesRecursive(this, ref downloads);
			WorkshopHelper.ModManager.Download(downloads.ToList());
		}

		private IEnumerable<ModDownloadItem> GetDependencies() {
			return ModReferences.Split(',')
				.Select(WorkshopHelper.QueryHelper.FindModDownloadItem)
				.Where(item => item != null && (!item.IsInstalled || (item.HasUpdate && !item.UpdateIsDowngrade)));
		}

		private void GetDependenciesRecursive(ModDownloadItem item, ref HashSet<ModDownloadItem> set) {
			var deps = item.GetDependencies();
			set.UnionWith(deps);

			// Cyclic dependency should never happen, as it's not allowed
			//TODO: What if the same mod is a dependency twice, but different versions?

			foreach (var dep in deps) {
				GetDependenciesRecursive(dep, ref set);
			}
		}
	}
}