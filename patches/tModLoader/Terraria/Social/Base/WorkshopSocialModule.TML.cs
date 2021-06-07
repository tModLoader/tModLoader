using System.Collections.Generic;
using Terraria.IO;

namespace Terraria.Social.Base
{
	public abstract partial class WorkshopSocialModule : ISocialModule
	{
		public abstract void PublishMod(string modSource, WorkshopItemPublishSettings settings);

		public abstract bool TryGetInfoForMod(string fullPath, out FoundWorkshopEntryInfo info);

		public abstract List<string> GetListOfMods();
	}
}
