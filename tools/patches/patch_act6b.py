import os

file_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\PixarSplashCinematic.cs"

with open(file_path, "r", encoding="utf-8") as f:
    text = f.read()

# Replace multiple empty lines if doubled (\r\n\r\n\r\n -> \r\n)
# But let's check exact lines with "ACT 6B"
lines = [l.strip("\r\n") for l in text.splitlines()]

# Find ACT 6B
start_idx = -1
end_idx = -1
for i, l in enumerate(lines):
    if "ACT 6B: Spotlight loses power" in l:
        start_idx = i
    if start_idx != -1 and "ACT 7: Bloop collapses" in l:
        end_idx = i
        break

print(f"Found ACT 6B at line {start_idx} to {end_idx}")

if start_idx != -1 and end_idx != -1:
    replacement_block = [
        "        // --- ACT 6B: Spotlight loses power and flickers off in a dramatic theater short-circuit! ---",
        "        if (SoundManager.Instance != null) SoundManager.Instance.PlayPoisonSound();",
        "",
        "        // 1. Violent electrical malfunction short-circuit flicker",
        "        float[] shortCircuitFlicker = new float[] { 0.12f, 0.95f, 0.05f, 0.80f, 0.0f, 0.65f, 0.08f, 0.40f, 0.0f };",
        "        for (int fl = 0; fl < shortCircuitFlicker.Length; fl++)",
        "        {",
        "            float op = shortCircuitFlicker[fl];",
        "            if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, op * 0.90f);",
        "            if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, op * 0.88f);",
        "            yield return new WaitForSecondsRealtime(0.06f);",
        "        }",
        "",
        "        // 2. Complete power failure — Total Blackout!",
        "        if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, 0f);",
        "        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0f);",
        "",
        "        // Bloop comical double-take in the pitch dark",
        "        ShowSpeechBubble(\"...Wait, who turned off the lights?!\", new Vector2(0f, 75f));",
        "",
        "        // Dramatic suspense blackout duration (1.4 seconds of dark)",
        "        yield return new WaitForSecondsRealtime(1.40f);",
        "",
        "        HideSpeechBubble();",
        "",
        "        // 3. Emergency backup generator / ignition starter sparks",
        "        if (SoundManager.Instance != null) SoundManager.Instance.PlayCountdownTick(false);",
        "        // Spark 1",
        "        if (spotlightImage != null) spotlightImage.color = new Color(1f, 0.95f, 0.85f, 0.35f);",
        "        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.35f);",
        "        yield return new WaitForSecondsRealtime(0.08f);",
        "",
        "        // Quick drop to dark",
        "        if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, 0f);",
        "        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0f);",
        "        yield return new WaitForSecondsRealtime(0.18f);",
        "",
        "        // Spark 2 (stronger)",
        "        if (SoundManager.Instance != null) SoundManager.Instance.PlayCountdownTick(false);",
        "        if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, 0.70f);",
        "        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.70f);",
        "        yield return new WaitForSecondsRealtime(0.07f);",
        "",
        "        // 4. Emergency power stabilizes — warm light hums to full stage illumination",
        "        if (SoundManager.Instance != null) SoundManager.Instance.PlayCountdownTick(true);",
        "        float restoreDur = 0.55f;",
        "        float rElapsed = 0f;",
        "        while (rElapsed < restoreDur)",
        "        {",
        "            rElapsed += Time.unscaledDeltaTime;",
        "            float rProgress = Mathf.Clamp01(rElapsed / restoreDur);",
        "            float rLight = Mathf.Lerp(0.35f, 0.90f, rProgress);",
        "            float rShadow = Mathf.Lerp(0.35f, 0.88f, rProgress);",
        "            if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, rLight);",
        "            if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, rShadow);",
        "            yield return null;",
        "        }",
        "        if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, 0.90f);",
        "        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.88f);",
        ""
    ]

    new_lines = lines[:start_idx] + replacement_block + lines[end_idx:]
    with open(file_path, "wb") as f:
        f.write("\r\n".join(new_lines).encode("utf-8"))
    print("Successfully replaced ACT 6B in PixarSplashCinematic.cs!")
