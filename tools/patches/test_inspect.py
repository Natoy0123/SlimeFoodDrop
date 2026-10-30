import os

file_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\PixarSplashCinematic.cs"

with open(file_path, "r", encoding="utf-8") as f:
    text = f.read()

# Let's write the complete, updated PixarSplashCinematic.cs with clean methods
# We'll read the existing file, ensure UpdateBloopShadow and UpdateFoodShadow are present,
# and wire them into every single animation state.
lines = text.splitlines()

# Let's inspect where UpdateLoadingBar is
idx_load = -1
for i, l in enumerate(lines):
    if "private void UpdateLoadingBar" in l:
        idx_load = i
        break

shadow_methods = [
    "    public void UpdateBloopShadow(float x, float y, Vector3 localScale)",
    "    {",
    "        if (bloopShadow == null) return;",
    "        float floorY = -180f;",
    "        float heightAboveFloor = Mathf.Max(0f, y - (-80f));",
    "        float heightRatio = Mathf.Clamp01(heightAboveFloor / 140f);",
    "",
    "        bloopShadow.rectTransform.anchoredPosition = new Vector2(x, floorY);",
    "",
    "        // Dynamic Squash & Stretch: Shadow directly mirrors Bloop's horizontal squish/stretch",
    "        float shadowScaleX = localScale.x * (1f - heightRatio * 0.35f);",
    "        float shadowScaleY = (localScale.x > 0.1f ? (1f / Mathf.Sqrt(localScale.x)) : 1f) * Mathf.Lerp(1f, 0.50f, heightRatio);",
    "        bloopShadow.rectTransform.localScale = new Vector3(shadowScaleX, shadowScaleY, 1f);",
    "",
    "        // Dynamic shadow opacity: dark and sharp when grounded, softer when in the air",
    "        float shadowAlpha = Mathf.Lerp(0.88f, 0.32f, heightRatio);",
    "        bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, shadowAlpha);",
    "    }",
    "",
    "    public void UpdateFoodShadow(float x, float y, Vector3 localScale)",
    "    {",
    "        if (foodShadow == null) return;",
    "        float floorY = -180f;",
    "        float heightAboveFloor = Mathf.Max(0f, y - (-95f));",
    "        float heightRatio = Mathf.Clamp01(heightAboveFloor / 30f);",
    "",
    "        foodShadow.rectTransform.anchoredPosition = new Vector2(x, floorY);",
    "",
    "        float shadowScaleX = localScale.x * (1f - heightRatio * 0.30f);",
    "        float shadowScaleY = localScale.y * (1f - heightRatio * 0.30f);",
    "        foodShadow.rectTransform.localScale = new Vector3(shadowScaleX, shadowScaleY, 1f);",
    "",
    "        float shadowAlpha = Mathf.Lerp(0.85f, 0.30f, heightRatio);",
    "        foodShadow.color = new Color(0.08f, 0.05f, 0.12f, shadowAlpha);",
    "    }",
    ""
]

# We will inject shadow_methods above CinematicSequenceRoutine or replace CinematicSequenceRoutine with the fully wired version.
print(f"UpdateLoadingBar found at line {idx_load}")
