This file is a guide for the various classes of this folder. To view this readme file correctly formatted, please visit [ExampleMod/Common/CustomVisualEquipType/README.md](https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Common/CustomVisualEquipType/README.md).

# Custom Visual EquipType
This folder shows implementing a custom visual EquipType.

A visual EquipType is essentially a `PlayerDrawLayer` that is shared by several items corresponding to a position on the player character. For example, all boots accessories in the game use the `EquipType.Shoes` equipment slot. When the player wears multiple boot accessories, the game will only render the latest visible boots rather than layering all the equipment textures over each other.

# Classes
Here are the classes involved in implementing a complete custom visual EquipType. Further details are included in comments contained within each file. Note that the approaches used in this implementation differs from the vanilla equipment slot code to keep things easy to understand. Most notably we do not assign specific "slot IDs", such as `Item.backSlot` or `Item.shoeSlot`, but rather the item type itself it used.

### EarringsLoader
This class manages loading and tracking the equipment textures of items using this custom EquipType.

### EarringsGlobalItem
This class applies the custom earring EquipType values of the accessory items and the corresponding dye/shader items to `EarringsPlayer` so they are available in `EarringsDrawLayer`.

### EarringsPlayer
This class stores which earring will be drawn, and with which dye/shader.

### EarringsDrawLayer
This is a `PlayerDrawLayer` class that renders the earrings EquipType.

### AutoloadEquip_EarringAttribute
It attribute will cause an annotated `ModItem` class to attempt to autoload an earrings equipment texture for this custom EquipType. This works even in other mods. This is similar to how `AutoloadEquip` is used to autoload textures for vanilla EquipType.

### RubyEarrings and SapphireEarrings
These classes are vanity accessories that use our custom earring EquipType. `SapphireEarrings` calls `EarringsLoader.AddEarringTexture` to load the texture and `RubyEarrings` uses `[AutoloadEquip_Earring]` to do the same. Also note how the vanilla item `Angler Earrings` is augmented with an earrings equipment texture as well in `EarringsLoader`.

# Showcase
A video showing how multiple accessories using the same custom EquipType works as expected even with dyes can be seen on the [pull request](https://github.com/tModLoader/tModLoader/pull/4606#issuecomment-2803296613).