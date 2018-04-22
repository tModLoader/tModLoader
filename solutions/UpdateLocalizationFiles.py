
# Run this script after updating en-US.tModLoader.json with new keys. python 3.
# Also make sure the file encodings are UTF-8 not UTF-8-BOM.

filename = '../src/tModLoader/Terraria.Localization.Content.{0}.tModLoader.json'

languages = ['zh-Hans', 'ru-RU', 'pt-BR', 'pl-PL', 'it-IT', 'fr-FR', 'es-ES', 'de-DE']
for language in languages:
    #language = 'zh-Hans'
    otherLanguage = ''
    print("Updating:",language)
    with open(filename.format('en-US'), 'r', encoding='utf-8') as english, open(filename.format(language), 'r', encoding='utf-8') as other:
        enLines = english.readlines()
        
        # Skip empty whitespace and comment lines to end up with only json lines.
        otherLines = [x for x in other.readlines() if not (x.strip().startswith("//") or len(x.strip()) == 0)]

        otherIndex = 0
        for englishIndex, englishLine in enumerate(enLines):
            # For lines with key values pairs, copy translation or add commented translation placeholder.
            if englishLine.find(": ") != -1:
                if otherLines[otherIndex].startswith(englishLine[:englishLine.find(": ")]):
                    otherLanguage += otherLines[otherIndex]
                    otherIndex += 1
                else:
                    otherLanguage += "\t\t// " + englishLine.strip() + '\n'
            # Add English Comments back in
            elif englishLine.strip().startswith('//'):
                otherLanguage += englishLine
            # Add other json lines in. Also add in whitespace lines.
            else:
                otherLanguage += englishLine
                if len(englishLine.strip()) > 0:
                    otherIndex += 1
            #print(otherLanguage)
    # Save changes.
    with open(filename.format(language), 'w', encoding='utf-8') as output:
        output.write(otherLanguage)

print("Make sure to run Diff.")
input("Press Enter to continue...")