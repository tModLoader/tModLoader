
Identify mods universally using ids or slugs, but in a uri pattern to define "source"
E.g. "steam://12345"
Or in alternative use the ModName "internal AssemblyName" if unique!!! (better option if it is searchable)

Provde ModDownloadItem with "UpdateInfo" system, needed when installing or other things on mods, now ModDownloadItem MUST be cached and no 2 of them with same name should exist at the same time, I'm fine with this, but this means that anyone "storing" a ModDownloadItem somewere will make queries inconsistent if they are reenumerated!

Might cosider to have the full store cached? this will solve problem above, and only first load could use the "continuous list" that updates while downloading. This implies that any modbrowser change will require a tML restart tho.

Get web page, info etc will probaby be better if inside the ModDownloadItem?
This tho needs to be checked on behaviour for local mods (need to convert local mod to download mod to show info etc???).

`CalculateWorkshopDeps` relies on the mod list to be fully downloaded to resolve ModName -> PublishId, so no sense in having dependencies stored as ids, modNames are required anyway.
