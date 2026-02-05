using UnityEditor;
using UnityEngine;

namespace CodeBase.Editor
{
    public static class FindMissingScripts
    {
        [MenuItem("Tools/Find Missing Scripts (All Objects)")]
        public static void FindMissingAllObjects()
        {
            var objects = Resources.FindObjectsOfTypeAll<GameObject>();
            int count = 0;

            foreach (var go in objects)
            {
                // відсіємо “ассети” (префаби в Project) — це метод про сцену/плеймод
                if (EditorUtility.IsPersistent(go))
                    continue;

                var components = go.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        count++;
                        Debug.LogError($"Missing script on GameObject: {GetHierarchyPath(go)}", go);
                    }
                }
            }

            Debug.Log($"Done. Missing scripts found: {count}");
        }

        private static string GetHierarchyPath(GameObject go)
        {
            string path = go.name;
            Transform t = go.transform;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }
    }
}
public static partial class FindMissingScripts
{
    [MenuItem("Tools/Find Missing Scripts In Prefabs (Project)")]
    public static void FindMissingInPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var components = prefab.GetComponentsInChildren<Component>(true);
            foreach (var c in components)
            {
                if (c == null)
                {
                    count++;
                    Debug.LogError($"Missing script in Prefab: {path}", prefab);
                    break; // достатньо 1 разу підсвітити префаб
                }
            }
        }

        Debug.Log($"Done. Prefabs with missing scripts: {count}");
    }
}