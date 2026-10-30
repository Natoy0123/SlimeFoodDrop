import os

# 1. Update PixarSplashCinematic.cs
cs_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\PixarSplashCinematic.cs"
with open(cs_path, "r", encoding="utf-8") as f:
    text = f.read()

text = text.replace("float floorY = -180f;", "float floorY = -150f;", 1) # In UpdateBloopShadow
text = text.replace("float floorY = -180f;", "float floorY = -160f;", 1) # In UpdateFoodShadow
text = text.replace("SpawnSplashTrailDroplet(float x, float y = -180f)", "SpawnSplashTrailDroplet(float x, float y = -150f)", 1)

with open(cs_path, "wb") as f:
    f.write(text.encode("utf-8"))

print("Updated floorY in PixarSplashCinematic.cs!")

# 2. Update BuildMainGameScene.cs
build_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(build_path, "r", encoding="utf-8") as f:
    btext = f.read()

btext = btext.replace('new Vector2(-460f, -180f)', 'new Vector2(-460f, -160f)')
btext = btext.replace('new Vector2(-550f, -180f)', 'new Vector2(-550f, -150f)')
btext = btext.replace('new Vector2(0f, -180f)', 'new Vector2(0f, -150f)')
btext = btext.replace('new Vector2(40f, -180f)', 'new Vector2(40f, -150f)')

# Also in stageFloorImg position
btext = btext.replace('Image stageFloorImg = CreateUISpriteIcon("Stage_Floor", stageObj.transform, stageFloorSprite, new Vector2(0f, -220f)',
                      'Image stageFloorImg = CreateUISpriteIcon("Stage_Floor", stageObj.transform, stageFloorSprite, new Vector2(0f, -200f)')

with open(build_path, "wb") as f:
    f.write(btext.encode("utf-8"))

print("Updated shadow positions in BuildMainGameScene.cs!")
