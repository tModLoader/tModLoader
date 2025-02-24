
# Run this script after updating en-US.tModLoader.json with new keys. python 3.
# Also make sure the file encodings are UTF-8 not UTF-8-BOM.

filename = '../src/tModLoader/Terraria/Localization/Content/{0}/tModLoader.json'

languages = ['zh-Hans', 'ru-RU', 'pt-BR', 'pl-PL', 'it-IT', 'fr-FR', 'es-ES', 'de-DE']
missings = []
for language in languages:
    #language = 'zh-Hans'
    otherLanguage = ''
    missing = 0
    print("Updating:",language)
    with open(filename.format('en-US'), 'r', encoding='utf-8') as english, open(filename.format(language), 'r', encoding='utf-8') as other:
        enLines = english.readlines()
        
        # Preserve Credits (comment lines on first few lines)
        otherLinesAll = other.readlines()
        for otherLine in otherLinesAll:
            if otherLine.startswith('//'):
                otherLanguage += otherLine
            else:
                break
                
        # Skip empty whitespace and comment lines to end up with only json lines.
        otherLines = [x for x in otherLinesAll if not (x.strip().startswith("//") or len(x.strip()) == 0)]
        # Store in dictionary for easy retrieval
        otherLinesDict = dict((k.strip(), v.strip()) for k,v in (line.strip().split(':', 1) for line in otherLines if line.find(':') != -1))

        for englishIndex, englishLine in enumerate(enLines):
            # Add English Comments back in
            if englishLine.strip().startswith('//'):
                otherLanguage += englishLine
            # For lines with key values pairs, copy translation or add commented translation placeholder.
            elif englishLine.find(": ") != -1 and len(englishLine.split('"')) >= 4:
                translationKey = englishLine[:englishLine.find(": ")].strip()
                if translationKey in otherLinesDict:
                    otherLanguage += "\t\t" + translationKey + ": " + otherLinesDict[translationKey] + '\n'
                else:
                    otherLanguage += "\t\t// " + englishLine.strip() + '\n'
                    missing += 1
            # Add other json lines in. Also add in whitespace lines.
            else:
                otherLanguage += englishLine
            #print(otherLanguage)
    # Save changes.
    if missing > 0:
        missings.append( (language, missing) )
        print("Missing:",missing)
    with open(filename.format(language), 'w', encoding='utf-8') as output:
        output.write(otherLanguage)

with open('./TranslationsNeeded.txt', 'w', encoding='utf-8') as output:
    if len(missings) == 0:
        output.write('All Translations up-to-date!')
    else:
        for entry in missings: 
            output.write(str(entry[0]) + " " + str(entry[1]) + "\n")