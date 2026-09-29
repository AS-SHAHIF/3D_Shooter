import re

fbx_meta_path = r"Assets/GLASSOFCOINS - FPS low poly fps/Machine_Gun_01/smg2(sub-machine gun).fbx.meta"
with open(fbx_meta_path, 'r', encoding='utf-8', errors='ignore') as f:
    meta_content = f.read()

print("--- FBX Import Settings ---")
for line in meta_content.split('\n'):
    if any(k in line for k in ["globalScale", "scaleFactor", "unitConversionFactor", "importBlendShapes"]):
        print(" ", line.strip())

prefab_path = r"Assets/GLASSOFCOINS - FPS low poly fps/Machine_Gun_01/smg2(sub-machine gun)_prefab/smg2(sub-machine gun)_prefab.prefab"
with open(prefab_path, 'r', encoding='utf-8', errors='ignore') as f:
    prefab_content = f.read()

print("\n--- Prefab Root Transforms ---")
tr_matches = re.finditer(r'--- !u!4 &(\d+)\nTransform:(.*?)(?=\n--- !u!|\Z)', prefab_content, re.DOTALL)
for m in list(tr_matches)[:5]:
    print(m.group(0)[:200])
