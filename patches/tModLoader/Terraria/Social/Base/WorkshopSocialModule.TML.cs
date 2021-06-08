using System.Collections.Generic;
using System.Collections.Specialized;
using Terraria.IO;

namespace Terraria.Social.Base
{
	public abstract partial class WorkshopSocialModule : ISocialModule
	{
		public abstract void PublishMod(ModLoader.Core.TmodFile modFile, NameValueCollection data, WorkshopItemPublishSettings settings);

		public abstract bool TryGetInfoForMod(string fullPath, out FoundWorkshopEntryInfo info);

		public abstract List<string> GetListOfMods();
	}
}
