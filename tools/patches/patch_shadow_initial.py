path = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs'
with open(path, 'r', encoding='utf-8') as f:
    text = f.read()

target = '''        Image foodShadow = CreateUISpriteIcon("Shadow_Food", stageObj.transform, contactShadowSprite, new Vector2(-460f, -115f), new Vector2(140f, 44f));
        foodShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.85f);
        cinematic.foodShadow = foodShadow;'''

repl = '''        Image foodShadow = CreateUISpriteIcon("Shadow_Food", stageObj.transform, contactShadowSprite, new Vector2(-460f, -115f), new Vector2(140f, 44f));
        foodShadow.color = new Color(0.08f, 0.05f, 0.12f, 0.85f);
        foodShadow.gameObject.SetActive(false);
        cinematic.foodShadow = foodShadow;'''

text_nl = text.replace('\r\n', '\n')
target_nl = target.replace('\r\n', '\n')
repl_nl = repl.replace('\r\n', '\n')

if target_nl in text_nl:
    text_nl = text_nl.replace(target_nl, repl_nl, 1)
    with open(path, 'wb') as f:
        f.write(text_nl.replace('\n', '\r\n').encode('utf-8'))
    print('Successfully patched Shadow_Food initial inactive state!')
else:
    print('Failed to find target')
