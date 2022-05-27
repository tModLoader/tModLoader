**Make a backup before using this tool, either by using git and committing your code, or by copying your mod's source
code to some other place**

This tool lets you port `1.3` mods to `1.4`, it is not perfect, but it can port many things.

---

How to use:
1) Download the tModPorter repo, open `tModPorter.sln` and build the solution.
2) Install tModLoader 1.4 (you can find more info about this on the [tModLoader discord](https://tmodloader.net/discord))
3) Open tModLoader 1.4, go to the Mod Sources menu and click the `Open Sources` button to open the Mod Sources folder
4) Copy your mod's source to the Mod Sources folder (i.e. copy `ModLoader/Mod Sources/MyMod`
   to `tModLoader/Mod Sources/MyMod`)
5) Open tModLoader again and go to the Mod Sources menu. You will see your mod with a little exclamation mark
   (saying `Upgrade .csproj file`). Click that button so that your mod can work with the 1.4 classes (and tModPorter can
   correctly port your mod).
6) Drag and drop your mod's .csproj file (usually named `YourModName.csproj`) to the tModPorter .exe file (that you
   either got from building tModPorter yourself, or from a release if there's one available)

If you manually built tModPorter, the exe should be in `tModPorter/tModPorter.Console/bin/Debug/net6.0/tModPorter.Console.exe`.

You need to install the .NET 6 SDK to build and run tModPorter.
