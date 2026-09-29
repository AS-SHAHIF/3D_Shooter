import re

for path in ["Assets/Prefab/M1911.prefab", "Assets/M16A4/M16a4_prefab.prefab", "Assets/Prefab/Zombie.prefab"]:
    with open(path, 'r', encoding='utf-8', errors='ignore') as f:
        content = f.read()
    print(f"\n--- {path} ---")
    for line in content.split('\n'):
        if any(k in line for k in ["weaponDamage", "HP", "damage", "bulletDamage", "zombieDamage"]):
            print(" ", line.strip())
