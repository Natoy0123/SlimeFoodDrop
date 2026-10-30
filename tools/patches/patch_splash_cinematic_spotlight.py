import os

path = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Scripts\PixarSplashCinematic.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

target1 = '''        if (spotlightImage != null)
        {
            spotlightImage.color = new Color(1f, 1f, 0.85f, 0f);
            spotlightImage.gameObject.SetActive(true);
        }'''

repl1 = '''        if (spotlightImage != null)
        {
            spotlightImage.color = new Color(1f, 1f, 1f, 0f);
            spotlightImage.gameObject.SetActive(true);
        }'''

target2 = '''            if (spotlightImage != null) spotlightImage.color = new Color(1f, 0.98f, 0.82f, a * 0.45f);'''
repl2 = '''            if (spotlightImage != null) spotlightImage.color = new Color(1f, 1f, 1f, a * 0.90f);'''

target3 = '''            for (int fl = 0; fl < flickerOpacities.Length; fl++)
            {
                spotlightImage.color = new Color(origColor.r, origColor.g, origColor.b, flickerOpacities[fl]);
                yield return new WaitForSecondsRealtime(0.06f);
            }
            spotlightImage.color = new Color(origColor.r, origColor.g, origColor.b, 0.0f);'''

repl3 = '''            for (int fl = 0; fl < flickerOpacities.Length; fl++)
            {
                spotlightImage.color = new Color(1f, 1f, 1f, flickerOpacities[fl] * 0.90f);
                yield return new WaitForSecondsRealtime(0.06f);
            }
            spotlightImage.color = new Color(1f, 1f, 1f, 0.0f);'''

target4 = '''                spotlightImage.color = new Color(1f, 0.94f, 0.80f, rAlpha);'''
repl4 = '''                spotlightImage.color = new Color(1f, 1f, 1f, Mathf.Clamp01(rAlpha * 2.2f));'''

content_nl = content.replace('\r\n', '\n')
for i, (t, r) in enumerate([(target1, repl1), (target2, repl2), (target3, repl3), (target4, repl4)]):
    if t not in content_nl:
        print(f'FAILED TO FIND TARGET {i+1}:', t[:40])
    else:
        content_nl = content_nl.replace(t, r, 1)

content_out = content_nl.replace('\n', '\r\n')
with open(path, 'w', encoding='utf-8') as f:
    f.write(content_out)

print('PixarSplashCinematic.cs successfully patched!')
