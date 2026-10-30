import os

path = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\SlimeController.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

target = '''    public void TriggerEatEffect()
    {
        currentAnimState = AnimState.Eat;
        currentFrameIndex = 0;
        specialStateTimer = 0.28f;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        currentDeformScale = new Vector3(baseScale.x * 1.28f, baseScale.y * 0.74f, 1f);
        transform.localScale = currentDeformScale;

        if (mouthRoutine != null) StopCoroutine(mouthRoutine);
        mouthRoutine = StartCoroutine(AnimateEatMouthRoutine());
    }'''

repl = '''    public void TriggerEatEffect()
    {
        currentAnimState = AnimState.Eat;
        currentFrameIndex = 0;
        specialStateTimer = 0.28f;
        if (spriteRenderer != null) spriteRenderer.color = Color.white;

        currentDeformScale = new Vector3(baseScale.x * 1.28f, baseScale.y * 0.74f, 1f);
        transform.localScale = currentDeformScale;

        if (mouthRenderer != null)
        {
            mouthRenderer.gameObject.SetActive(true);
            if (mouthOpenSprite != null) mouthRenderer.sprite = mouthOpenSprite;
        }

        if (Application.isPlaying)
        {
            if (mouthRoutine != null) StopCoroutine(mouthRoutine);
            mouthRoutine = StartCoroutine(AnimateEatMouthRoutine());
        }
    }'''

content_nl = content.replace('\r\n', '\n')
if target in content_nl:
    content_nl = content_nl.replace(target, repl, 1)
    content_out = content_nl.replace('\n', '\r\n')
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content_out)
    print('SlimeController.cs TriggerEatEffect updated!')
else:
    print('Target not found in SlimeController.cs')
