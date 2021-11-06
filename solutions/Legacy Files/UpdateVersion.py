import fileinput

def TypicalReplace(filename, searchterm, result):
    for line in fileinput.input(filename, inplace=True):
        if line.startswith(searchterm):
            print(searchterm + result)
        else:
            print(line, end='')

versionInput = input("Enter new version number separated by periods ('0.1.2.3'):  ").strip()
versionList = versionInput.split('.')
#versionNums = [int(x) for x in versionList]

TypicalReplace("CompleteRelease.bat", "set version=v", versionInput)
TypicalReplace("../ExampleMod/build.txt", "version = ", versionInput)
TypicalReplace("documentation/Doxyfile", "PROJECT_NUMBER         = ", versionInput)
TypicalReplace("../patches/tModLoader/Terraria.ModLoader/ModLoader.cs", "\t\tpublic static readonly Version version = new Version(", ', '.join(versionList) + ');')

with open('ReleaseExtras/version', 'w') as verisonFile:
    verisonFile.write('v' + versionInput)

input("Press Enter to continue...")