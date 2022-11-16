using System.Collections.Generic;
using System.Collections.Specialized;
using Terraria.ModLoader.Core;

namespace Terraria.Social.Base;

public abstract partial class WorkshopSocialModule
{
	public abstract bool PublishMod(TmodFile modFile, NameValueCollection data, WorkshopItemPublishSettings settings);

	public abstract bool TryGetInfoForMod(TmodFile modFile, out FoundWorkshopEntryInfo info);

	public abstract List<string> GetListOfMods();
}
