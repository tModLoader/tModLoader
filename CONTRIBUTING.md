# Please review te following:

## Your files use tab indendation (keep tabs) of size 4

`Visual Studio`

	Tools -> Options
	Text Editor -> C# -> Tabs
	Settings:
		Indenting: Smart
		Tab size: 4
		Indent size: 4
		Keep tabs CHECKED
    
![Example Visual Studio](https://i.imgur.com/1m8PLjn.png "Example Visual Studio")

`Notepad++`

	Tools -> Preferences
	Language -> C#
	Settings:
		[Default]
		Tab size: 4
		Replace by space UNCHECKED
		
![Example notepad++](https://i.imgur.com/kbF0CMu.png "Example Notepad++")

## You edit in the tModLoader solution, the chance is very slim you need another one!

To open this workspace, navigate to solutions/ and open tModLoader.sln

![Example solution](https://i.imgur.com/fLHUHgj.png "Example solution")

## You use our code patcher, so you only commit patches

To open our code patcher, run setup.bat from the main directory.
If you've worked in the tModLoader workspace, you press 'Diff tModLoader'
This will create new patches containing your changes in the patches/ folder

![Example patcher](https://i.imgur.com/Ltol24M.png "Example Patcher")

## You inspect your diffs (patches)

Before you commit or create a pull request, inspect your patches. Make sure nothing weird happened.
Do not auto format vanilla files when possible, it can break things.

![Diff example](https://i.imgur.com/jwu2GOG.png "Diff example")

