namespace Terraria.Social.Base;

public class ModWorkshopEntry : AWorkshopEntry
{
	public static string GetHeaderTextFor(ulong workshopEntryId, string[] tags, WorkshopItemPublicSettingId publicity, string previewImagePath)
		=> CreateHeaderJson("Mod", workshopEntryId, tags, publicity, previewImagePath);
}
