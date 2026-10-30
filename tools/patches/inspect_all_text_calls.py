import re

with open(r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs", "r", encoding="utf-8") as f:
    code = f.read()

# Pattern for CreateUIText calls
pattern = r'CreateUIText\s*\(\s*"([^"]+)"\s*,\s*([^,]+),\s*([^,]+),\s*"([^"]*)"\s*,\s*(\d+)'
matches = re.findall(pattern, code)

print(f"Total CreateUIText calls: {len(matches)}")
print(f"{'Text Name':32} | {'Font Var':18} | {'Size':4} | {'Sample Text':40}")
print("-" * 105)
for name, parent, font_var, sample, size in matches:
    print(f"{name:32} | {font_var.strip():18} | {size:4} | {sample[:40]}")
