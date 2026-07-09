
import json
import json5
import os.path

# This file formats the json files and notifies if any have changed. The files downloaded from steam are not indented and are hard to diff.

steamLanguages = ['english', 'german', 'italian', 'french', 'spanish', 'russian', 'schinese', 'brazilian', 'polish']

missings = []
anyFileNeedsUploading = False

def DuplicateOutputForForSimilarLanguages(newLanguage, newLanguageDisplay, steamLanguage, steamTagData):
    print(f"Duplicating {steamLanguage} to {newLanguageDisplay}")
    steamTagData['language'] = newLanguage
    outputFilename = "SteamUploadStorePage/storepage_275117_{0}.json".format(newLanguage)
    outputString = json.dumps(steamTagData, indent=4, ensure_ascii=False)
    print("Updated File:", outputFilename)
    with open(outputFilename, "w", encoding='utf-8') as tagFileNew:
        tagFileNew.write(outputString)

for index, steamLanguage in enumerate(steamLanguages):
    filename = "SteamUploadStorePage/storepage_275117_{0}.json".format(steamLanguage)

    if not os.path.isfile(filename):
        continue

    print("Updating:", steamLanguage)
    missing = 0

    with open(filename, 'r', encoding='utf-8') as tagFile:
        originalString = tagFile.read()
        steamTagData = json5.loads(originalString)

    longDescriptionFilename = "storepage_{0}_LongDescription.txt".format(steamLanguage)
    try:
        with open(longDescriptionFilename, 'r', encoding='utf-8') as longDescriptionFile:
            longDescriptionText = longDescriptionFile.read()
            steamTagData["app[content][about]"] = longDescriptionText
    except FileNotFoundError:
        print("Error: Corresponding LongDescription file not found.")
    
    # Remove empty data
    steamTagData = {k: v for k, v in steamTagData.items() if v not in [None, "", [], {}]}

    outputString = json.dumps(steamTagData, indent=4, ensure_ascii=False) # json.dump instead of json5.dump for quoted keys

    if outputString != originalString:
        print("Updated File:", filename)
        anyFileNeedsUploading = True
        
        with open(filename, "w", encoding='utf-8') as tagFileNew:
            tagFileNew.write(outputString)

            if steamLanguage == "spanish":
                DuplicateOutputForForSimilarLanguages("latam", "latin america spanish", steamLanguage, steamTagData)

            if steamLanguage == "brazilian":
                DuplicateOutputForForSimilarLanguages("portuguese", "portuguese", steamLanguage, steamTagData)

if anyFileNeedsUploading:
    print("Make sure to upload the Output files to steam. (Any file in the output mentioned as updated file)")
else:
    print("No files changed, no need to upload any files.")
input("Press Enter to continue...")
