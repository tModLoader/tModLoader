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

## Item
- Remove `CanBurnInLava` hook

## ItemSlot
- Preserve modded data in journey duplication menu. See `JourneyDuplicationItemCreationContext` for all code-paths. 
Also need to use `CanStack`/`TryStackItems`. See similar hooks in shop buy. Should we have a creation context for shop purchasing too? Perhaps that could replace `PostBuyItem`?

## NPC
- Remove `NPCHeadLoader.GetNPCFromHeadSlot`
- Rename `NPCLoader.ScaleExpertStats`
- NPCLoader.CanHitNPC should return a plain bool. The only use of "override true" below is to force the skeleton merchant to be hit by skeletons
- The pile of NPCLoader.TownNPCAttack... hooks hurts me
- Maintainability pass? Check for `NPCLoader`, `CombinedHooks`, `BuffLoader`, `PlayerLoader`

## Projectile
- Move `NPCLoader.CanBeHitByProjectile`, `ProjectileLoader.CanHitNPC` and `PlayerLoader.CanHitNPCWithProj` into `CombinedHooks`
- Update `ArmorPenetration` vanilla values to match switch case after `int num6 = (int)Main.player[owner].armorPenetration`

## Player
- Add hook for `RefreshInfoAccsFromItemType`
- Add `ItemLoader.ConsumeItem` check to `QuickHeal` and `QuickMana`
- Move `OnHit` and `ModifyHit` into `CombinedHooks`
- Check `PlayerIO`, make sure `favourited` flag is saved in void vault
- Check all usages of void bag (`bank4`)
- Make sure loadout serialization doesn't save modded data to the vanilla .plr
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
- No need for `AlternateLegacyNPCProfile` since all vanilla NPCs now have profiles
	

## Recipe.cs
- `Item.PopulateMaterialCache()` do we need this anymore?
- do we need to clone `notDecraftable`

## Tile(.TML).cs:
- Patches have been reimplemented, check that again.
- Replace `ModTile.OpenDoorID` and `ClosedDoorID` with sets

## Porting Notes:
- `GrantPrefixBenefits` is only called if `Item.accessory` is `true`. This applies in mod accessory slots too now.
- `ModWaterStyle` now requires an additional texture, `_Slope`. See `ExampleWaterStyle` for details.
- Reforging is now implemented via `Item.ResetPrefix`. This sets `prefix` to 0 and then refreshes the item. Make sure any custom fields set by custom prefixes are not serialized independently.

## WorldGen.cs:
- TileLoader.Drop can probably be moved to `Item.NewItem` with `GetItemSource_FromTileBreak`
- Comment on Convert needs to be updated for new biome types, `BiomeConversionID` now exists