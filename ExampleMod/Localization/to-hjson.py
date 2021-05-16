import hjson
import os
import sys

def parse_lang_file(f):
    lines = []
    parsed = {}
    with open(lang_file, mode="r", encoding="utf-8") as f:
        lines = f.read().splitlines()

    for line in lines:
        split = line.split("=")

        if len(split) < 2:
            continue
        
        if line.strip().startswith("#"):
            continue

        key = split[0].strip().replace(" ", "_")
        val = "=".join(split[1:]).replace("\\n", "\n")

        if not val:
            continue

        key_split = key.split(".")

        current = parsed
        for k in key_split[:-1]:
            if k not in current:
                current[k] = {}
            
            current = current[k]
        
        current[key_split[-1]] = val

    return parsed

path = "./"
if len(sys.argv) > 1:
    path = sys.argv[1]

lang_files = [
    os.path.join(path, f) for f in os.listdir(path) if f.endswith(".lang") and os.path.isfile(os.path.join(path, f))
]

print(f"Processing { lang_files }")

for lang_file in lang_files:
    parsed = parse_lang_file(lang_file)

    hjson_converted = hjson.dumps(parsed)

    hjson_lang_file: str = lang_file[:-4] + "hjson"
    with open(hjson_lang_file, mode="w", encoding="utf-8") as f:
        f.write(hjson_converted)

print("Done!")