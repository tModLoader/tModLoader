This file is a guide for the various classes of this folder. To view this readme file correctly formatted, please visit [ExampleMod/Content/CustomModType/README.md](https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod/Content/CustomModType/README.md).

# Custom ModType
This folder shows implementing a custom `ModType` (or new content type). 

There are many benefits to using a `ModType` class for making new content. The main benefit is that loading the content is handled using the same logic as existing content types, so other modders will be able to use them as expected as if they were provided by tModLoader itself. Other modders will simply need to inherit from the base class, no need for `Mod.Call` to interface with the mod providing the `ModType`. (They will, of course, also need to have a reference to this mod.) This also includes loading a primary texture, automatic localization registration, being able to use `ModContent.Find` and `ModContent.GetInstance`, support for custom ID sets, and support for manually loading multiple instances of a class.

# Classes
Here are the classes involved in implementing a complete custom `ModType`. Further details are included in comments contained within each file.

### ModVictoryPose
This class is the custom `ModType` that other mods will inherit from. Think of it like how modders inherit from `ModItem` to add a new item to the game. A Victory Pose is a special effect that plays after defeating a boss enemy.

### HandsUpVictoryPose, HandsUpWithFireworksVictoryPose, and NonAutoloadVictoryPose
These are all "content" added by this mod. They are the default victory poses available unless other mods add their own. `HandsUpVictoryPose` and `HandsUpWithFireworksVictoryPose` are autoloaded as normal, while `NonAutoloadVictoryPose` shows off manually loading multiple instances of a single `ModVictoryPose` class in `NonAutoloadVictoryPose.NonAutoloadVictoryPoseLoader`.

### VictoryPoseLoader 
This class handles tracking all registered `ModVictoryPose` content. `ModVictoryPose` calls `VictoryPoseLoader.Add` to assign an ID value and register the content.

This is the also the main API exposed and intended to be used by other mods. For example, other mods could use the API to trigger a specific ModVictoryPose manually, such as when they craft a specific item.

### VictoryPosePlayer 
This `ModPlayer` class handles applying and updating the active `ModVictoryPose`. It also handles the default trigger for starting a pose, which is defeating an enemy. The file also contains `PoseIconParticle`, a custom `IParticle` example which is used to render an image in the game world.

### VictoryPoseID and VictoryPoseID.Sets
`VictoryPoseID` and `VictoryPoseID.Sets` manage the ID sets for `ModVictoryPose`. Like with other content, it can be useful to be able to have "sets" for storing content-specific data.

# Showcase
A video showing `ModVictoryPose` in action can be seen on the [pull request description](https://github.com/tModLoader/tModLoader/pull/4611).