import re

scene_path = r"Assets/Scenes/SampleScene.unity"

with open(scene_path, 'r', encoding='utf-8', errors='ignore') as f:
    content = f.read()

# Find Player GameObject
go_matches = re.finditer(r'--- !u!1 &(\d+)\nGameObject:(.*?)(?=\n--- !u!|\Z)', content, re.DOTALL)
player_go_id = None
player_block = None

for m in go_matches:
    go_id = m.group(1)
    block = m.group(2)
    name_m = re.search(r'm_Name:\s*(.*)', block)
    if name_m and name_m.group(1).strip() == "Player":
        player_go_id = go_id
        player_block = block
        print(f"Found Player GameObject &{go_id}")
        break

if player_block:
    comp_ids = re.findall(r'component:\s*\{fileID:\s*(\d+)\}', player_block)
    print(f"Player Components count: {len(comp_ids)}")
    for cid in comp_ids:
        comp_m = re.search(r'--- !u!(\d+) &' + cid + r'\n(.*?)(?=\n--- !u!|\Z)', content, re.DOTALL)
        if comp_m:
            comp_type = comp_m.group(1)
            comp_body = comp_m.group(2)
            print(f"\n--- Component type {comp_type} &{cid} ---")
            print(comp_body[:400])
