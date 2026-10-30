import os

# 1. Patch PixarSplashCinematic.cs
splash_cs = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\PixarSplashCinematic.cs"
with open(splash_cs, "rb") as f:
    splash_text = f.read().decode("utf-8")

target_splash = """        // --- ACT 6B: Spotlight loses power and flickers off as power shuts down! ---
        if (spotlightImage != null)
        {
            Color origColor = spotlightImage.color;
            // Rapid power failure flicker
            float[] flickerOpacities = new float[] { 0.05f, 0.35f, 0.0f, 0.28f, 0.04f, 0.18f, 0.0f };
            for (int fl = 0; fl < flickerOpacities.Length; fl++)
            {
                spotlightImage.color = new Color(1f, 1f, 1f, flickerOpacities[fl] * 0.90f);
                yield return new WaitForSecondsRealtime(0.06f);
            }
            spotlightImage.color = new Color(1f, 1f, 1f, 0.0f);
        }

        yield return new WaitForSecondsRealtime(0.25f);

        // Emergency power restores spotlight with a warm gentle glow for the loading phase
        if (spotlightImage != null)
        {
            float restoreDur = 0.50f;
            float rElapsed = 0f;
            while (rElapsed < restoreDur)
            {
                rElapsed += Time.unscaledDeltaTime;
                float rAlpha = Mathf.Lerp(0f, 0.28f, rElapsed / restoreDur);
                spotlightImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01(rAlpha * 2.2f));
                yield return null;
            }
        }"""

replacement_splash = """        // --- ACT 6B: Spotlight loses power and flickers off in a dramatic theater short-circuit! ---
        if (SoundManager.Instance != null) SoundManager.Instance.PlayPoisonSound();

        // 1. Violent electrical malfunction short-circuit flicker
        float[] shortCircuitFlicker = new float[] { 0.12f, 0.95f, 0.05f, 0.80f, 0.0f, 0.65f, 0.08f, 0.40f, 0.0f };
        for (int fl = 0; fl < shortCircuitFlicker.Length; fl++)
        {
            float op = shortCircuitFlicker[fl];
            if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, op * 0.90f);
            if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, op * 0.88f);
            yield return new WaitForSecondsRealtime(0.06f);
        }

        // 2. Complete power failure — Total Blackout!
        if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, 0f);
        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0f);

        // Bloop comical double-take in the pitch dark
        ShowSpeechBubble("...Wait, who turned off the lights?!", new Vector2(0f, 75f));

        // Dramatic suspense blackout duration (1.4 seconds of dark)
        yield return new WaitForSecondsRealtime(1.40f);

        HideSpeechBubble();

        // 3. Emergency backup generator / ignition starter sparks
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCountdownTick(false);
        // Spark 1
        if (spotlightImage != null) spotlightImage.color = new Color(1f, 0.95f, 0.85f, 0.35f);
        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.35f);
        yield return new WaitForSecondsRealtime(0.08f);

        // Quick drop to dark
        if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, 0f);
        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0f);
        yield return new WaitForSecondsRealtime(0.18f);

        // Spark 2 (stronger)
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCountdownTick(false);
        if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, 0.70f);
        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.70f);
        yield return new WaitForSecondsRealtime(0.07f);

        // 4. Emergency power stabilizes — warm light hums to full stage illumination
        if (SoundManager.Instance != null) SoundManager.Instance.PlayCountdownTick(true);
        float restoreDur = 0.55f;
        float rElapsed = 0f;
        while (rElapsed < restoreDur)
        {
            rElapsed += Time.unscaledDeltaTime;
            float rProgress = Mathf.Clamp01(rElapsed / restoreDur);
            float rLight = Mathf.Lerp(0.35f, 0.90f, rProgress);
            float rShadow = Mathf.Lerp(0.35f, 0.88f, rProgress);
            if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, rLight);
            if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, rShadow);
            yield return null;
        }
        if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, 0.90f);
        if (bloopShadow != null) bloopShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.88f);"""

splash_text_nl = splash_text.replace("\r\n", "\n")
target_splash_nl = target_splash.replace("\r\n", "\n")
replacement_splash_nl = replacement_splash.replace("\r\n", "\n")

if target_splash_nl in splash_text_nl:
    splash_text_nl = splash_text_nl.replace(target_splash_nl, replacement_splash_nl, 1)
    with open(splash_cs, "wb") as f:
        f.write(splash_text_nl.replace("\n", "\r\n").encode("utf-8"))
    print("PixarSplashCinematic.cs patched successfully!")
else:
    print("FAILED to find target in PixarSplashCinematic.cs")

# 2. Patch BuildMainGameScene.cs for foodShadow visibility
build_cs = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs"
with open(build_cs, "rb") as f:
    build_text = f.read().decode("utf-8")

build_text_nl = build_text.replace("\r\n", "\n")

target_preview = """                ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeTurnLookUser;
                ui.pixarSplashCinematic.foodImage.gameObject.SetActive(false);
                ui.pixarSplashCinematic.ShowSpeechBubble("...Did you see what I just ate?!", new Vector2(0f, 75f));"""

repl_preview = """                ui.pixarSplashCinematic.bloopImage.sprite = ui.pixarSplashCinematic.slimeTurnLookUser;
                ui.pixarSplashCinematic.foodImage.gameObject.SetActive(false);
                if (ui.pixarSplashCinematic.foodShadow != null) ui.pixarSplashCinematic.foodShadow.gameObject.SetActive(false);
                ui.pixarSplashCinematic.ShowSpeechBubble("...Did you see what I just ate?!", new Vector2(0f, 75f));"""

target_catch = """            ui.pixarSplashCinematic.foodImage.gameObject.SetActive(true);
            ui.pixarSplashCinematic.foodImage.sprite = ui.pixarSplashCinematic.foodPoisonReveal;
            ui.pixarSplashCinematic.foodRect.anchoredPosition = new Vector2(40f, -95f);
            ui.pixarSplashCinematic.foodRect.localScale = new Vector3(0.5f, 0.5f, 1f);"""

repl_catch = """            ui.pixarSplashCinematic.foodImage.gameObject.SetActive(true);
            ui.pixarSplashCinematic.foodImage.sprite = ui.pixarSplashCinematic.foodPoisonReveal;
            ui.pixarSplashCinematic.foodRect.anchoredPosition = new Vector2(40f, -95f);
            ui.pixarSplashCinematic.foodRect.localScale = new Vector3(0.5f, 0.5f, 1f);
            if (ui.pixarSplashCinematic.foodShadow != null)
            {
                ui.pixarSplashCinematic.foodShadow.gameObject.SetActive(true);
                ui.pixarSplashCinematic.foodShadow.rectTransform.anchoredPosition = new Vector2(40f, -115f);
            }"""

target_cleanup = """        if (ui.pixarSplashCinematic != null)
        {
            ui.pixarSplashCinematic.foodImage.gameObject.SetActive(false);
            ui.pixarSplashCinematic.projectileImage.gameObject.SetActive(false);
            ui.pixarSplashCinematic.speechBubbleGroup.SetActive(false);
            ui.pixarSplashCinematic.bloopRect.localScale = Vector3.one;
        }"""

repl_cleanup = """        if (ui.pixarSplashCinematic != null)
        {
            ui.pixarSplashCinematic.foodImage.gameObject.SetActive(false);
            if (ui.pixarSplashCinematic.foodShadow != null) ui.pixarSplashCinematic.foodShadow.gameObject.SetActive(false);
            ui.pixarSplashCinematic.projectileImage.gameObject.SetActive(false);
            ui.pixarSplashCinematic.speechBubbleGroup.SetActive(false);
            ui.pixarSplashCinematic.bloopRect.localScale = Vector3.one;
        }"""

for t, r in [(target_preview, repl_preview), (target_catch, repl_catch), (target_cleanup, repl_cleanup)]:
    t_nl = t.replace("\r\n", "\n")
    r_nl = r.replace("\r\n", "\n")
    if t_nl in build_text_nl:
        build_text_nl = build_text_nl.replace(t_nl, r_nl, 1)
        print("Patched a block in BuildMainGameScene.cs")
    else:
        print("FAILED to find block:", t_nl[:40])

with open(build_cs, "wb") as f:
    f.write(build_text_nl.replace("\n", "\r\n").encode("utf-8"))
