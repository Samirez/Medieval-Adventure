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
    // compiled whole-word regex for enemy or npc to avoid recompiling on each call
    private static readonly Regex EnemyWordRegex = new Regex(@"\b(enemy|npc)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
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

            // Skip model (read-only) prefabs up front
            var assetType = PrefabUtility.GetPrefabAssetType(prefab);
            if (assetType == PrefabAssetType.Model)
            {
                Debug.Log($"Skipping model prefab (read-only): {path}");
                continue;
            }

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

                // NOTE: We add missing components to the prefab root by default.
                // In many projects components are expected on specific child objects; this auto-fix
                // attempts to find a sensible child ('Logic' or 'Body') and add components there first,
                // falling back to the root if none are found.
                foreach (var t in missingTypes)
                {
                    // Try to find a preferred child to attach logic components to
                    Transform targetTransform = root.transform.Find("Logic");
                    if (targetTransform == null) targetTransform = root.transform.Find("Body");

                    GameObject attachTo = targetTransform != null ? targetTransform.gameObject : root;

                    // Ensure attachTo is not part of a nested prefab instance; if it is, fall back to root
                    try
                    {
                        if (PrefabUtility.IsAnyPrefabInstanceRoot(attachTo) || PrefabUtility.IsPartOfPrefabInstance(attachTo))
                        {
                            attachTo = root;
                        }
                    }
                    catch (Exception ex)
                    {
                        // If prefab utilities fail for any reason, log the exception for diagnostics
                        Debug.LogException(ex);
                        // be conservative and use root
                        attachTo = root;
                    }

                    try
                    {
                        attachTo.AddComponent(t);
                        Debug.Log($"Added component {t.Name} to {(attachTo == root ? "root" : attachTo.name)} of prefab {path}");
                        changed = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to add component {t.Name} to {(attachTo == root ? "root" : attachTo.name)} in prefab {path}: {ex.Message}\n{ex.StackTrace}");
                        // continue with next component
                    }
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
                        // Advise manual review because adding components to root/children may be incorrect for some prefabs
                        Debug.LogWarning($"Auto-fix added components to prefab {path}. Please verify the prefab hierarchy and component placement manually.");
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

    // Helper: returns true if the provided name looks like an enemy name
    private static bool IsEnemyLike(string name)
    {
        if (string.IsNullOrEmpty(name)) return false;

        // Whole-word regex for enemy or npc
        if (!EnemyWordRegex.IsMatch(name))
        {
            return false;
        }

        // Exclude common non-enemy tokens to reduce false positives
        string lower = name.ToLower();
        foreach (var ex in EnemyNameExclusions)
        {
            if (lower.Contains(ex)) return false;
        }

        return true;
    }

    // Overload: check by path and prefab name, and also allow folder-based or tag/component heuristics
    private static bool IsEnemyLike(string path, GameObject prefab)
    {
        if (string.IsNullOrEmpty(path) == false)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);
            if (IsEnemyLike(fileName)) return true;

            string lowerPath = path.ToLower();
            // Allow explicit folder placement to classify as enemy
            if (lowerPath.Contains("/enemies/") || lowerPath.Contains("/npcs/")) return true;
        }

        if (prefab != null)
        {
            if (IsEnemyLike(prefab.name)) return true;
            // If prefab has tag 'Enemy', treat it as enemy
            try
            {
                if (!string.IsNullOrEmpty(prefab.tag) && prefab.CompareTag("Enemy")) return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Prefab tag check failed for {prefab?.name}: {ex.Message}", prefab);
            }
        }

        return false;
    }
}
#endif