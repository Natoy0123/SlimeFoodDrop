import os

runner_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop\Assets\Editor\InEnginePlayTestRunner.cs"
with open(runner_path, 'r', encoding='utf-8') as f:
    text = f.read()

trigger_code = """
[InitializeOnLoad]
public static class InEngineAutoTrigger
{
    private static double lastCheckTime = 0;

    static InEngineAutoTrigger()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate()
    {
        if (EditorApplication.timeSinceStartup - lastCheckTime < 0.5) return;
        lastCheckTime = EditorApplication.timeSinceStartup;

        string triggerPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "run_playtest.trigger");
        if (File.Exists(triggerPath))
        {
            try
            {
                File.Delete(triggerPath);
            }
            catch {}

            Debug.Log("[InEngineAutoTrigger] Detected trigger file! Running InEnginePlayTestRunner.RunPlayTest()...");
            InEnginePlayTestRunner.RunPlayTest();
        }
    }
}
"""

if "InEngineAutoTrigger" not in text:
    text = text.rstrip() + "\n" + trigger_code
    with open(runner_path, 'w', encoding='utf-8') as f:
        f.write(text)
    print("Added InEngineAutoTrigger to InEnginePlayTestRunner.cs")
else:
    print("InEngineAutoTrigger already in InEnginePlayTestRunner.cs")
