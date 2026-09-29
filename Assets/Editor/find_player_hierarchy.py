import re

scene_path = r"Assets/Scenes/SampleScene.unity"

with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# Find all GameObjects with 'Player' in name
gos = re.findall(r'--- !u!1 &(\d+)\nGameObject:(.*?)(?=\n--- !u!|\Z)', content, re.DOTALL)
for go_id, block in gos:
    name_m = re.search(r'm_Name:\s*(.*)', block)
    name = name_m.group(1).strip() if name_m else ""
    if "player" in name.lower():
        print(f"\n==========================================")
        print(f"GameObject: '{name}' (ID: {go_id})")
        comp_refs = re.findall(r'component:\s*\{fileID:\s*(\d+)\}', block)
        for cr in comp_refs:
            comp_block_m = re.search(r'--- !u!(\d+) &' + cr + r'\n(.*?)(?=\n--- !u!|\Z)', content, re.DOTALL)
            if comp_block_m:
                ctype = comp_block_m.group(1)
                cbody = comp_block_m.group(2)
                script_m = re.search(r'm_Script:\s*\{fileID:\s*\d+,\s*guid:\s*([0-9a-fA-F]+)', cbody)
                guid = script_m.group(1) if script_m else ""
                print(f"  - Comp Type: {ctype:3} (ID: {cr}) GUID: {guid}")
                # Print variables if it's MonoBehaviour
                if ctype == "114":
                    lines = cbody.split('\n')
                    for l in lines[:15]:
                        if any(k in l for k in ['_speed', 'speed', '_gravity', 'groundCheck', 'GrounLayerMask', 'HP', 'mouseSensitivity']):
                            print(f"      {l.strip()}")
