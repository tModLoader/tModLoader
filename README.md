# tModLoader

## About

tModLoader is essentially a mod for Terraria that provides a way to load your own mods without having to work directly with Terraria's source code itself. This means you can easily make mods that are compatible with other people's mods and save yourself the trouble of having to decompile then recompile Terraria.exe. It is made to work for Terraria 1.3.

My goal for tModLoader it to make it simple as possible while giving the modder powerful control over the game. It is designed in a way as to minimize the effort required for me to update to future Terraria versions. If you either don't want to commit to this project or are not able to decompile Terraria, I am open to suggestions for hooks.

Download and installation instructions are on the forums thread.

[Forums Thread](http://forums.terraria.org/index.php?threads/1-3-tmodloader-a-modding-api.23726/)

Note that this repository will usually be ahead of the current released version.

## Contributing

**Huge thanks to [ChickenBones](https://github.com/Chicken-Bones) for creating the code patcher!**

### Getting the tModLoader code

If you want to contribute to this project, you'll have to download the code for tModLoader, including all the changes made to the Terraria source. This requires you to have purchased your own Terraria.exe first. Note that you *must* have the *Windows* version in order for this to work! (Creating patch files that operate on the Mac version would basically just be giving away the source code, due to how the decompiler works differently on it for some reason.)

In order to do get the tModLoader code, first clone the repository, then run the setup.bat file. If setup.bat doesn't work, you may have to unblock the files in the repository. Once tModLoader Dev Setup is open, just click on the Setup button and select the location of your vanilla Terraria.exe. **Warning:** decompiling will probably freeze your computer for a couple of hours. So you'll need to find something to do in real life until that's done.

When that's all done, you should have the tModLoader source in the src folder. Open the solutions folder then open the tModLoader solution.

### Committing changes

So you've made some changes to Terraria and want to commit them. Run setup.bat again, then *(Important) click on Format Code*. Select the src/tModLoader folder, then wait a bit for it to format. When that's done, click on Diff tModLoader. This will create patch files with the changes you've made. Finally, all you'll need to do is commit the patches folder.
