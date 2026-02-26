using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CodeBase.Editor
{
    public static partial class FindMissingScripts
    {
        [MenuItem("Tools/Find Missing Scripts (DontDestroyOnLoad)")]
        public static void FindMissingInDontDestroyOnLoad()
        {
            int count = 0;
            var all = Resources.FindObjectsOfTypeAll<GameObject>();

            foreach (var go in all)
            {
                if (!go.scene.IsValid())
                    continue;

                if (go.scene.name != "DontDestroyOnLoad")
                    continue;

                var components = go.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        count++;
                        Debug.LogError($"Missing script in DDoL: {(go)}", go);
                        break;
                    }
                }
            }

            Debug.Log($"Done. Missing scripts in DontDestroyOnLoad: {count}");
        }
    }

    public static partial class FindMissingScripts
    {
        [MenuItem("Tools/Find Missing Scripts In Scenes (Project)")]
        public static void FindMissingInScenes()
        {
            var setup = EditorSceneManager.GetSceneManagerSetup();

            string[] guids = AssetDatabase.FindAssets("t:Scene");
            int count = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                int sceneMissing = 0;
                foreach (var root in scene.GetRootGameObjects())
                    sceneMissing += GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(root);

                if (sceneMissing > 0)
                {
                    count += sceneMissing;
                    Debug.LogError($"Missing script in Scene: {path} (missing: {sceneMissing})");
                }
            }

            EditorSceneManager.RestoreSceneManagerSetup(setup);

            Debug.Log($"Done. Total missing scripts in scenes: {count}");
        }
    }
}

