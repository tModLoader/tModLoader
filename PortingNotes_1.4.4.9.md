# TODOs

## General

- Check why we alter DrawData.useDestinationRectangle
	https://github.com/tModLoader/tModLoader/commit/e38affeb6a121090555377c74eeed78c40280627

-	```cs
	//unknown justification for preventing odd window sizes. Causes excessive device resets. - ChickenBones
	//width &= 0x7FFFFFFE;
	//height &= 0x7FFFFFFE;
	```

## Player
- Reapply patch for `sItem.useStyle == 13` and `sItem.useStyle == 5`? Do we still want this now that `NetMessage.SendData(13` is sent as well?
```patch
+				// Added by TML. #ItemTimeOnAllClients
+				if (whoAmI != Main.myPlayer)
+					return;
```

## MessageBuffer
- `ModTile.ChestDrop` and `DresserDrop` code/patches are atrocious.

## TownNPCProfiles
- Might be a good time to get rid of NPCHeadLoader? At least for town NPCs?

## Tile(.TML).cs:
- Patches have been reimplemented, check that again.

## Porting Notes:
- `GrantPrefixBenefits` is only called if `Item.accessory` is `true`. This applies in mod accessory slots too now.
- `ModWaterStyle` now requires an additional texture, `_Slope`. See `ExampleWaterStyle` for details.
- Reforging is now implemented via `Item.ResetPrefix`. This sets `prefix` to 0 and then refreshes the item. Make sure any custom fields set by custom prefixes are not serialized independently.

## Formatter
- Not visiting into switch case blocks properly? See `ItemSlot.LeftClick`

## ChildSafety
- Factories, do they need support for modded ids? (Should be made into issue - Mutant)