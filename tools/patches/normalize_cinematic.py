import os

path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\PixarSplashCinematic.cs"
with open(path, "r", encoding="utf-8") as f:
    text = f.read()

# Filter out empty lines if they were artificially doubled
raw_lines = text.splitlines()
cleaned_lines = []
last_empty = False
for line in raw_lines:
    is_empty = len(line.strip()) == 0
    if is_empty:
        if not last_empty:
            cleaned_lines.append("")
        last_empty = True
    else:
        cleaned_lines.append(line.rstrip())
        last_empty = False

print(f"Original lines: {len(raw_lines)}, Cleaned lines: {len(cleaned_lines)}")

with open(path, "wb") as f:
    f.write("\r\n".join(cleaned_lines).encode("utf-8"))
print("Normalized line spacing in PixarSplashCinematic.cs!")
