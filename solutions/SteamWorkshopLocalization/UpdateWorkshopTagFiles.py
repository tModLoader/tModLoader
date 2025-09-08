
# Run this script after updating en-US\tModLoader.json with new keys. python 3.
# Also make sure the file encodings are UTF-8 not UTF-8-BOM.

import json
import json5
import os.path

tModLoaderLocalizationFilename = '../../src/tModLoader/Terraria/Localization/Content/{0}/tModLoader.json'

# Map steams identifiers to our keys.
steamToLocalizationKey = {
  #"language":"english",
  #"appid":1281930,
  "category_0":"TagsCategoryModFeatures",
  "category_1":"TagsCategoryModSide",
  "category_2":"TagsCategoryTModLoaderVersion",
  #"category_3":"LegacyMenu.103", # Handled manually below
  "tag_197618":"TagsQoL", #"quality of life",
  "tag_197621":"TagsContent", #"new content",
  "tag_275098":"TagsLibrary", #"library",
  "tag_307061":"TagsUtility", #"Utilities",
  "tag_307062":"TagsGameplay", #"Gameplay Tweaks",
  "tag_307063":"TagsAudio", #"Audio Tweaks",
  "tag_307064":"TagsVisual", #"Visual Tweaks",
  "tag_307066":"TagsGen", #"Custom World Gen",
  #"tag_307067":"Both",
  #"tag_307172":"Client",
  #"tag_307173":"Server",
  #"tag_307174":"NoSync",
  #"tag_348888":"1.4.4",
  #"tag_349568":"1.4.3"
  "tag_362262":"TagsLanguage_English",
  "tag_362263":"TagsLanguage_German",
  "tag_362264":"TagsLanguage_Italian",
  "tag_362265":"TagsLanguage_French",
  "tag_362266":"TagsLanguage_Spanish",
  "tag_362267":"TagsLanguage_Russian",
  "tag_362268":"TagsLanguage_Chinese",
  "tag_362269":"TagsLanguage_Portuguese",
  "tag_362270":"TagsLanguage_Polish",
  "tag_558042":"TagsTranslation",
}

# English will be updated directly, if ever needed. -- actually, not sure what the website text input changes yet, internal or display.
languages = ['en-US', 'de-DE', 'it-IT', 'fr-FR', 'es-ES', 'ru-RU', 'zh-Hans', 'pt-BR', 'pl-PL']
steamLanguages = ['english', 'german', 'italian', 'french', 'spanish', 'russian', 'schinese', 'brazilian', 'polish']
TagsCategoryLanguage = ['Language', 'Sprache', 'Lingua', 'Langue', 'Idioma', 'Язык', '语言', 'Idioma', 'Język'] # not in tModLoader.json, so just do it manually for simplicity

# TODO: what to do about latam-spanishlatinamerica, sc_schinese-steamsimplifiedchinese, tchinese-traditionalchinese, portuguese-portugalportuguese?
# should we copy the results from the similar language? Not sure why there are 3 chinese either, steam website only has 2 options.
missings = []
anyFileNeedsUploading = False

# For each language, update the steamtagfile:
# open the template, open the tmod file
# for each entry in the mapping, update the json if exists
# write out steam tag json

for index, (language, steamLanguage) in enumerate(zip(languages, steamLanguages)):
    print("Updating:", language)
    missing = 0

    with open("workshop_tags_1281930_{0}.json".format(steamLanguage), 'r', encoding='utf-8') as tagFile:
        steamTagData = json5.load(tagFile)
    with open(tModLoaderLocalizationFilename.format(language), 'r', encoding='utf-8') as english:
        tModLoaderJsonData = json5.load(english) # use json5 to load to support comments

    for key, value in steamToLocalizationKey.items():
        # key is steam
        if value in tModLoaderJsonData["tModLoader"]:
            temp = tModLoaderJsonData["tModLoader"][value]
            steamTagData[key] = temp
        else:
            missing += 1
    
    steamTagData['category_3'] = TagsCategoryLanguage[index]

    if missing > 0:
        missings.append( (language, missing) )
        print("Missing:", missing)

    outputFilename = "workshop_tags_1281930_{0}_Output.json".format(steamLanguage)
    outputString = json.dumps(steamTagData, indent=4, ensure_ascii=False) # json.dump instead of json5.dump for quoted keys

    originalString = ""
    if os.path.isfile(outputFilename):
        with open(outputFilename, 'r', encoding='utf-8') as tagFileExisting:
            originalString = tagFileExisting.read()

    if outputString != originalString:
        print("Updated File:", outputFilename)
        anyFileNeedsUploading = True
        
        with open(outputFilename, "w", encoding='utf-8') as tagFileNew:
            tagFileNew.write(outputString)

with open('./TranslationsNeeded.txt', 'w', encoding='utf-8') as output:
    if len(missings) == 0:
        output.write('All Translations up-to-date!')
    else:
        for entry in missings: 
            output.write(str(entry[0]) + " " + str(entry[1]) + "\n")
if anyFileNeedsUploading:
    print("Make sure to upload the Output files to steam. (Any file in the output mentioned as updated file)")
else:
    print("No files changed, no need to upload any files.")
input("Press Enter to continue...")
