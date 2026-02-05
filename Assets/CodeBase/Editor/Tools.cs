using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor
{
    public class Tools
    {
        [MenuItem("Tools/Clear prefs")]
        public static void ClearPrefs()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("[TOOLS] PlayerPrefs cleared");
        }

        [MenuItem("Tools/Clear save file")]
        public static void ClearSaveFile()
        {
            // ⚠️ постав тут реальну назву файлу з твого SavedLoadService
            string path = Path.Combine(Application.persistentDataPath, "progress.json");

            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"[TOOLS] Save file deleted: {path}");
            }
            else
            {
                Debug.Log($"[TOOLS] Save file not found: {path}");
            }
        }

        [MenuItem("Tools/Print persistentDataPath")]
        public static void PrintPersistentPath()
        {
            Debug.Log($"[TOOLS] persistentDataPath: {Application.persistentDataPath}");
        }
    }
}