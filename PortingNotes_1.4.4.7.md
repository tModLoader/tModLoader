# TODOs

## General

- Axe of regrowth needs a lookup to the sapling type of a given modtree

- Check why we alter DrawData.useDestinationRectangle
	https://github.com/tModLoader/tModLoader/commit/e38affeb6a121090555377c74eeed78c40280627

-	```cs
	//unknown justification for preventing odd window sizes. Causes excessive device resets. - ChickenBones
	//width &= 0x7FFFFFFE;
	//height &= 0x7FFFFFFE;
	```
## Main
- Check `NPC.DrawTownAttackSwing` and `DrawTownAttackGun`, may need a consistency update with the new `GetItemDrawFrame` method
- Investigate new `BindSettingsTo`

## ItemSlot
- Preserve modded data in journey duplication menu. See `JourneyDuplicationItemCreationContext` for all code-paths. 
Also need to use `CanStack`/`TryStackItems`. See similar hooks in shop buy. Should we have a creation context for shop purchasing too? Perhaps that could replace `PostBuyItem`?

## Projectile
- Update `ArmorPenetration` vanilla values to match switch case after `int num6 = (int)Main.player[owner].armorPenetration`

## Player
- Add hook for `RefreshInfoAccsFromItemType`
- Replace `ProjectileLoader.SingleGrappleHook` with a `ProjectileID.Sets`
- `dropItemCheck` patches should use the `Item` overload of `NewItem`. Need to check for loss of modded data in multiplayer? OnSpawn hooks will have the wrong item instance.
- Add `ItemLoader.ConsumeItem` check to `QuickHeal` and `QuickMana`
- Check `PlayerIO`, make sure `favourited` flag is saved in void vault
- Check all usages of void bag (`bank4`)
- Make sure loadout serialization doesn't save modded data to the vanilla .plr
- Rename `MaxBuffs` to `maxBuffs` to minimise vanilla patches
- Reapply patch for `sItem.useStyle == 13` and `sItem.useStyle == 5`? Do we still want this now that `NetMessage.SendData(13` is sent as well?
```patch
+				// Added by TML. #ItemTimeOnAllClients
+				if (whoAmI != Main.myPlayer)
+					return;
```

## MessageBuffer
- `ModTile.ChestDrop` and `DresserDrop` code/patches are atrocious.

## TownNPCProfiles
- Investigate with the new shimmer profiles. 
	- Give ExampleNPC and ExampleTravellingMerchant a shimmer variant/texture
	- Involves adding modded IO for NPC.ShimmeredTownNPCs
	- Might be a good time to get rid of NPCHeadLoader? At least for town NPCs?

## Tile(.TML).cs:
- Patches have been reimplemented, check that again.
- Replace `ModTile.OpenDoorID` and `ClosedDoorID` with sets

## TileID.tML.cs
- `CanBeSatOnForNPCs` -> `CanNPCsSitOn`

## NPC
- Remove `NPCSpawnInfo.PlanteraDefeated`, replacement is `NPC.downedPlantBoss && Main.hardMode`

## Porting Notes:
- `GrantPrefixBenefits` is only called if `Item.accessory` is `true`. This applies in mod accessory slots too now.
- `ModWaterStyle` now requires an additional texture, `_Slope`. See `ExampleWaterStyle` for details.
- Reforging is now implemented via `Item.ResetPrefix`. This sets `prefix` to 0 and then refreshes the item. Make sure any custom fields set by custom prefixes are not serialized independently.

## WorldGen.cs:
- TileLoader.Drop can probably be moved to `Item.NewItem` with `GetItemSource_FromTileBreak`
- Revert some of the public field changes. A lot of them aren't meant to be public and were just blanket changed in the past. We have WorldGenVars now

## MysticLogFairies, SandStorm, SkyManager, WorldGen, Main and Deerclops AI
- Could benefit from a Main variable which counts number of full ticks of dayRate which have progressed this tick.
- No need for _timePass variables

## MessageID.cs
- Convert all `Obsolete` (not `Old`) entries to tModPorter refactors and remove.

## Item.cs
- Make FindOwner code use Player.GetItemGrabRange

## Formatter
- Not visiting into switch case blocks properly? See `ItemSlot.LeftClick`

## ChildSafety
- Factories, do they need support for modded ids?