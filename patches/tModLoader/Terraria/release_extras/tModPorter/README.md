## tModPorter

tModPorter is a tool to help keep mods up to date with changes to the tModLoader API. 

- tModPorter runs on a ".csproj" file. You can drag and drop your .csproj onto tModPorter.bat, or launch it and drag your .csproj into the command line window.
- The .csproj file must be updated to the 1.4 format, this means it has a line in it like `<Import Project="..\tModLoader.targets" />`
    - Use the button in the tML mod development menu to upgrade your .csproj first if you need to.
    - Normally, your .csproj will need to be in the ModSources folder. You should open it in Visual Studio and make sure it doesn't have any warnings about unresolved references before you try and run this tool.
- tModPorter will only fix compile errors. No errors, no changes. tModPorter is meant to be safe to run at any time, but it will make .bak files whenever it changes a file.
- To disable the creation of .bak files (because you use git or have made your own backup), run it with the "-nobak" parameter. Eg "tModPorter.bat -nobak MyMod.csproj"
