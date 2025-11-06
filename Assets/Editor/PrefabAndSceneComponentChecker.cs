#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using RPG.Resources;

public class PrefabAndSceneComponentChecker : EditorWindow
{
    [MenuItem("Tools/Scan Prefabs for Missing Components and Health")]
    public static void ShowWindow()
    {
        GetWindow(typeof(PrefabAndSceneComponentChecker));
    }

    void OnGUI()
    {
        GUILayout.Label("Prefab & Scene Component Scanner", EditorStyles.boldLabel);
        if (GUILayout.Button("Run Scan"))
        {
            RunScan();
        }
        if (GUILayout.Button("Auto-Fix Missing Enemy Components"))
        {
            if (EditorUtility.DisplayDialog("Auto-fix prefabs?",
                "This will add missing combat components (Health, Fighter, BaseStats, Experience, ActionScheduler, Mover) to prefabs that look like enemies. It will modify prefabs on disk. Make a backup before running. Proceed?", "Yes", "No"))
            {
                AutoFixMissingEnemyComponents();
            }
        }
    }

    public static void RunScan()
    {
        var reportLines = new List<string>();
        reportLines.Add("Prefab & Scene Component Scan Report");
        reportLines.Add("=================================");

        // Scan prefabs
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        reportLines.Add($"Found {guids.Length} prefabs to scan.");

        int missingScriptCount = 0;

        // Components we consider relevant for combat/enemy prefabs
        var combatTypes = new System.Type[] {
            typeof(RPG.Resources.Health),
            typeof(RPG.Combat.Fighter),
            typeof(RPG.Stats.BaseStats),
            typeof(RPG.Resources.Experience),
            typeof(RPG.Core.ActionScheduler),
            typeof(RPG.Movement.Mover)
        };

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            var missingForPrefab = new List<string>();

            // First check for missing script references on any child transform
            foreach (Transform t in prefab.GetComponentsInChildren<Transform>(true))
            {
                var components = t.GetComponents<Component>();
                for (int i = 0; i < components.Length; i++)
                {
                    if (components[i] == null)
                    {
                        reportLines.Add($"Missing script on prefab '{path}' at transform '{GetTransformPath(t)}'");
                        missingScriptCount++;
                        break;
                    }
                }
            }

            // Heuristic: if prefab name or path contains 'enemy' or 'npc', check for combat components
            string lowerPath = path.ToLower();
            string lowerName = prefab.name.ToLower();
            bool looksLikeEnemy = lowerPath.Contains("enemy") || lowerName.Contains("enemy") || lowerPath.Contains("npc") || lowerName.Contains("npc");

            if (looksLikeEnemy)
            {
                // Check for presence of each combat related component somewhere on the prefab hierarchy
                foreach (var t in combatTypes)
                {
                    var found = false;
                    foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
                    {
                        var comp = child.GetComponent(t);
                        if (comp != null)
                        {
                            found = true; break;
                        }
                    }
                    if (!found)
                    {
                        missingForPrefab.Add(t.Name);
                    }
                }
            }

            if (missingForPrefab.Count > 0)
            {
                reportLines.Add($"Prefab: {path} looks like an enemy but is missing components: {string.Join(", ", missingForPrefab)}");
            }
        }

        reportLines.Add($"Missing scripts (count): {missingScriptCount}");

        // Write report to project root
        string reportPath = Path.Combine(Application.dataPath, "../MissingComponentsReport.txt");
        File.WriteAllLines(reportPath, reportLines.ToArray());

        // Print summary to Console
        foreach (var line in reportLines)
        {
            Debug.Log(line);
        }

        Debug.Log($"Scan complete. Report saved to: {reportPath}");
    }

    public static void AutoFixMissingEnemyComponents()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int fixedCount = 0;
        var combatTypes = new System.Type[] {
            typeof(RPG.Resources.Health),
            typeof(RPG.Combat.Fighter),
            typeof(RPG.Stats.BaseStats),
            typeof(RPG.Resources.Experience),
            typeof(RPG.Core.ActionScheduler),
            typeof(RPG.Movement.Mover)
        };

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            string lowerPath = path.ToLower();
            string lowerName = prefab.name.ToLower();
            bool looksLikeEnemy = lowerPath.Contains("enemy") || lowerName.Contains("enemy") || lowerPath.Contains("npc") || lowerName.Contains("npc");
            if (!looksLikeEnemy) continue;

            // Determine which combat types are missing anywhere in the prefab
            var missingTypes = new List<System.Type>();
            foreach (var t in combatTypes)
            {
                var found = false;
                foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
                {
                    var comp = child.GetComponent(t);
                    if (comp != null)
                    {
                        found = true; break;
                    }
                }
                if (!found) missingTypes.Add(t);
            }

            if (missingTypes.Count == 0) continue;

            // Load prefab contents, add missing components to root, save
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;
            foreach (var t in missingTypes)
            {
                if (root.GetComponent(t) == null)
                {
                    root.AddComponent(t);
                    Debug.Log($"Added component {t.Name} to prefab {path}");
                    changed = true;
                }
            }

            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                fixedCount++;
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        Debug.Log($"Auto-fix complete. Modified {fixedCount} prefabs.");
        EditorUtility.DisplayDialog("Auto-fix complete", $"Modified {fixedCount} prefabs.", "OK");
    }

    static string GetTransformPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
#endif