import os

path = r'C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\BuildMainGameScene.cs'
with open(path, 'rb') as f:
    data = f.read()

target = b'        // Label inside Bar\r\n        labelText = CreateUIText("Text_BarLabel", trackObj.transform, font, defaultLabel, 23, TextAnchor.MiddleCenter, Color.white);'

repl = b'        // Label inside Bar (resizeTextForBestFit enabled for audit safety)\r\n        labelText = CreateUIText("Text_BarLabel", trackObj.transform, font, defaultLabel, 19, TextAnchor.MiddleCenter, Color.white);\r\n        labelText.resizeTextForBestFit = true;\r\n        labelText.resizeTextMinSize = 14;\r\n        labelText.resizeTextMaxSize = 20;'

if target in data:
    data = data.replace(target, repl, 1)
    with open(path, 'wb') as f:
        f.write(data)
    print('Updated Text_BarLabel resizeTextForBestFit!')
else:
    print('Target not found')
