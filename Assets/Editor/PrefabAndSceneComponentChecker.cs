#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using RPG.Resources;

public class PrefabAndSceneComponentChecker : EditorWindow
{
    private static readonly System.Type[] CombatTypes = new System.Type[] {
        typeof(RPG.Resources.Health),
        typeof(RPG.Combat.Fighter),
        typeof(RPG.Stats.BaseStats),
        typeof(RPG.Resources.Experience),
        typeof(RPG.Core.ActionScheduler),
        typeof(RPG.Movement.Mover)
    };

    // exclusion tokens to avoid false positives for names like EnemySpawner, EnemyHealthBar, etc.
    private static readonly string[] EnemyNameExclusions = new string[] {
        "spawner", "health", "manager", "dialog", "ui", "bar", "pickup", "controller", "creator", "display", "hud"
    };
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

        // Use the shared CombatTypes constant

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

            // Heuristic: refined enemy detection to reduce false positives
            bool looksLikeEnemy = IsEnemyLike(path, prefab);

            if (looksLikeEnemy)
            {
                // Check for presence of each combat related component somewhere on the prefab hierarchy
                foreach (var t in CombatTypes)
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

        // Write report to project root with error handling
        string reportPath = Path.Combine(Application.dataPath, "../MissingComponentsReport.txt");
        try
        {
            File.WriteAllLines(reportPath, reportLines.ToArray());
            // Print summary to Console
            foreach (var line in reportLines)
            {
                Debug.Log(line);
            }

            Debug.Log($"Scan complete. Report saved to: {reportPath}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to write report to {reportPath}: {ex.Message}\n{ex.StackTrace}");
            EditorUtility.DisplayDialog("Failed to write report", $"Could not write report to {reportPath}: {ex.Message}", "OK");
        }
    }

    public static void AutoFixMissingEnemyComponents()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            if (!IsEnemyLike(path, prefab)) continue;

            // Determine which combat types are missing anywhere in the prefab
            var missingTypes = new List<System.Type>();
            foreach (var t in CombatTypes)
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

            GameObject root = null;
            bool changed = false;
            try
            {
                root = PrefabUtility.LoadPrefabContents(path);

                // Add each missing type to the root (no redundant check)
                foreach (var t in missingTypes)
                {
                    root.AddComponent(t);
                    Debug.Log($"Added component {t.Name} to prefab {path}");
                    changed = true;
                }

                if (changed)
                {
                    var saved = PrefabUtility.SaveAsPrefabAsset(root, path);
                    if (saved == null)
                    {
                        Debug.LogError($"Failed to save prefab at {path} after modification.");
                    }
                    else
                    {
                        fixedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error while auto-fixing prefab {path}: {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                if (root != null)
                {
                    PrefabUtility.UnloadPrefabContents(root);
                }
            }
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