import subprocess, os, time

project_path = r"C:\Users\ADMIN\UnityProjects\SlimeFoodDrop"
unity_exe = r"C:\Program Files\Unity\Hub\Editor\6000.3.0f1\Editor\Unity.exe"
log_file = os.path.join(project_path, "Logs", "in_engine_playtest.log")

if os.path.exists(log_file):
    try: os.remove(log_file)
    except: pass

cmd = [
    unity_exe,
    "-batchmode",
    "-quit",
    "-projectPath", project_path,
    "-executeMethod", "InEnginePlayTestRunner.RunPlayTest",
    "-logFile", log_file
]

print("Launching Unity to execute InEnginePlayTestRunner.RunPlayTest()...")
proc = subprocess.Popen(cmd)
proc.wait()
print(f"Unity process finished with exit code: {proc.returncode}")

if os.path.exists(log_file):
    with open(log_file, "r", encoding="utf-8", errors="ignore") as f:
        lines = f.readlines()
        print("\n--- IN-ENGINE PLAYTEST LOG OUTPUT ---")
        for line in lines:
            if any(k in line for k in ["[TEST", "[IN-ENGINE", "[PLAYTEST", "FAIL", "Assertion", "Exception"]):
                print(line.strip())
