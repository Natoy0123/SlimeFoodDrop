import os

path = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\SlimeController.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

# 1. Add fields
target1 = '    public Sprite[] deadFrames;\n'
repl1 = '''    public Sprite[] deadFrames;

    [Header("Mouth Animation (Pixar Eat and Chomp)")]
    public SpriteRenderer mouthRenderer;
    public Sprite mouthOpenSprite;
    public Sprite mouthChewSprite;
    public Sprite mouthSmileSprite;
    private Coroutine mouthRoutine;
    private readonly Vector3 mouthBaseOffset = new Vector3(0.04f, -0.07f, 0f);
'''

# 2. Add mouth in EnsurePowerUpAuraVisuals
target2 = '        healBurstRenderer = GetOrCreateChildRenderer("HealSparkle_Burst", new Vector3(0f, 0.12f, 0f), baseOrder + 4, GetOrCreateHealBurstSprite());\n    }'
repl2 = '''        healBurstRenderer = GetOrCreateChildRenderer("HealSparkle_Burst", new Vector3(0f, 0.12f, 0f), baseOrder + 4, GetOrCreateHealBurstSprite());

        // 5. Attached Animated Mouth on Slime Face
        mouthRenderer = GetOrCreateChildRenderer("Slime_Mouth", mouthBaseOffset, baseOrder + 2, mouthOpenSprite != null ? mouthOpenSprite : GetOrCreateDefaultMouthSprite());
        if (mouthRenderer != null)
        {
            mouthRenderer.transform.localScale = Vector3.one * 0.45f;
            mouthRenderer.gameObject.SetActive(false);
        }
    }'''

# 3. Add mouth orientation in Update
target3 = '''        if (spriteRenderer != null && Mathf.Abs(currentVelocityX) > 0.4f)
        {
            spriteRenderer.flipX = currentVelocityX > 0f;
        }
    }'''
repl3 = '''        if (spriteRenderer != null && Mathf.Abs(currentVelocityX) > 0.4f)
        {
            spriteRenderer.flipX = currentVelocityX > 0f;
        }

        if (mouthRenderer != null && mouthRenderer.gameObject.activeSelf)
        {
            mouthRenderer.flipX = spriteRenderer != null ? spriteRenderer.flipX : false;
            float mx = (mouthRenderer.flipX) ? -mouthBaseOffset.x : mouthBaseOffset.x;
            mouthRenderer.transform.localPosition = new Vector3(mx, mouthBaseOffset.y, 0f);
        }
    }'''

# 4. Update TriggerEatEffect
target4 = '''    public void TriggerEatEffect()
    {
        currentAnimState = AnimState.Eat;
        currentFrameIndex = 0;
        specialStateTimer = 0.28f;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        currentDeformScale = new Vector3(baseScale.x * 1.28f, baseScale.y * 0.74f, 1f);
        transform.localScale = currentDeformScale;
    }'''
repl4 = '''    public void TriggerEatEffect()
    {
        currentAnimState = AnimState.Eat;
        currentFrameIndex = 0;
        specialStateTimer = 0.28f;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        currentDeformScale = new Vector3(baseScale.x * 1.28f, baseScale.y * 0.74f, 1f);
        transform.localScale = currentDeformScale;

        if (mouthRoutine != null) StopCoroutine(mouthRoutine);
        mouthRoutine = StartCoroutine(AnimateEatMouthRoutine());
    }

    private IEnumerator AnimateEatMouthRoutine()
    {
        if (mouthRenderer == null) yield break;
        mouthRenderer.gameObject.SetActive(true);

        if (mouthOpenSprite != null) mouthRenderer.sprite = mouthOpenSprite;
        yield return new WaitForSeconds(0.12f);

        if (mouthChewSprite != null) mouthRenderer.sprite = mouthChewSprite;
        yield return new WaitForSeconds(0.10f);

        if (mouthSmileSprite != null) mouthRenderer.sprite = mouthSmileSprite;
        yield return new WaitForSeconds(0.12f);

        mouthRenderer.gameObject.SetActive(false);
        mouthRoutine = null;
    }'''

# 5. Reset mouth in TriggerHurtEffect
target5 = '''    public void TriggerHurtEffect()
    {'''
repl5 = '''    public void TriggerHurtEffect()
    {
        if (mouthRoutine != null) { StopCoroutine(mouthRoutine); mouthRoutine = null; }
        if (mouthRenderer != null) mouthRenderer.gameObject.SetActive(false);'''

# 6. Reset mouth in TriggerDeadEffect
target6 = '''    public void TriggerDeadEffect()
    {'''
repl6 = '''    public void TriggerDeadEffect()
    {
        if (mouthRoutine != null) { StopCoroutine(mouthRoutine); mouthRoutine = null; }
        if (mouthRenderer != null) mouthRenderer.gameObject.SetActive(false);'''

# 7. Reset mouth in ResetToAliveState
target7 = '''        healBurstTimer = 0f;
        shieldFlashTimer = 0f;'''
repl7 = '''        healBurstTimer = 0f;
        shieldFlashTimer = 0f;
        if (mouthRoutine != null) { StopCoroutine(mouthRoutine); mouthRoutine = null; }
        if (mouthRenderer != null) mouthRenderer.gameObject.SetActive(false);'''

# 8. Add GetOrCreateDefaultMouthSprite helper
target8 = '''    private static Sprite cachedHealBurstSprite;'''
repl8 = '''    private static Sprite cachedDefaultMouthSprite;
    private static Sprite GetOrCreateDefaultMouthSprite()
    {
        if (cachedDefaultMouthSprite != null) return cachedDefaultMouthSprite;
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color[] cols = new Color[size * size];
        for (int i = 0; i < cols.Length; i++) cols[i] = Color.clear;
        for (int y = 14; y <= 50; y++)
        {
            for (int x = 8; x <= 56; x++)
            {
                float dx = (x - 32f) / 22f;
                float dy = (y - 32f) / 16f;
                if (dx * dx + dy * dy <= 1f)
                {
                    cols[y * size + x] = (y < 26) ? new Color(0.92f, 0.42f, 0.55f, 1f) : new Color(0.12f, 0.08f, 0.14f, 1f);
                }
            }
        }
        tex.SetPixels(cols);
        tex.Apply();
        cachedDefaultMouthSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return cachedDefaultMouthSprite;
    }

    private static Sprite cachedHealBurstSprite;'''

# Normalize newlines for matching
content_nl = content.replace('\r\n', '\n')
for i, (t, r) in enumerate([(target1, repl1), (target2, repl2), (target3, repl3), (target4, repl4), (target5, repl5), (target6, repl6), (target7, repl7), (target8, repl8)]):
    if t not in content_nl:
        print(f'FAILED TO FIND TARGET {i+1}:', t[:40])
    else:
        content_nl = content_nl.replace(t, r, 1)

# Restore CRLF
content_out = content_nl.replace('\n', '\r\n')
with open(path, 'w', encoding='utf-8') as f:
    f.write(content_out)

print('SlimeController.cs successfully patched!')
