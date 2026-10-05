import re
import json

html_file = r"C:\Users\Administrator\.gemini\antigravity-ide\brain\24be54a6-9f38-4690-829f-ef1d03a0bdc1\.system_generated\steps\18\content.md"

with open(html_file, 'r', encoding='utf-8') as f:
    content = f.read()

# Pattern to extract table rows. This is a heuristic approach matching the specific table structure.
pattern = re.compile(r'<tr>\s*<td>.*?</td>\s*<td>.*?</td>\s*<td>(.*?)</td>\s*<td>.*?</td>\s*<td>.*?</td>\s*<td>(.*?)</td>\s*<td>.*?>(.*?)<.*?</td>\s*<td>.*?</td>\s*</tr>', re.DOTALL)

matches = pattern.findall(content)

# Dictionaries for our localizations
localization = {}

for en_name, tw_name, cn_name in matches:
    en_name = en_name.strip()
    tw_name = tw_name.strip()
    # sometimes there are nested tags or different structures, clean them
    tw_name = re.sub(r'<[^>]+>', '', tw_name).strip()
    cn_name = re.sub(r'<[^>]+>', '', cn_name).strip()
    
    # Create the Zone ID by converting to uppercase, replacing spaces and quotes with underscores
    zone_id = en_name.upper().replace(' ', '_').replace("'", "").replace("-", "_")
    
    if zone_id and zone_id not in localization:
        localization[zone_id] = {
            "en-US": en_name,
            "zh-CN": cn_name,
            "zh-TW": tw_name
        }

# Special mapping for composite terror zones or aliases
# We will just write the base ones for now

output_file = r"f:\za\tool\D2TZtool\Data\localization.json"
import os
os.makedirs(os.path.dirname(output_file), exist_ok=True)

with open(output_file, 'w', encoding='utf-8') as f:
    json.dump(localization, f, ensure_ascii=False, indent=2)

print(f"Parsed {len(localization)} zones to {output_file}")
