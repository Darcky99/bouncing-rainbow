#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class PrefabHelper
{
    public static GameObject SelectedGO => Selection.activeGameObject;

    [MenuItem("GameObject/Kob Prefab/UnPack Prefab", false, -100)]
    public static void KobUnpackPrefab()
    {
        Debug.LogError($"UnPacking {GetPrefabType(SelectedGO)} Prefab '{SelectedGO.name}'");

        UnpackPrefab(SelectedGO);
    }

    [MenuItem("GameObject/Kob Prefab/UnPack Prefab Variant Only", false, -100)]
    public static void KobUnpackPrefabVariant()
    {
        Debug.LogError($"UnPacking {GetPrefabType(SelectedGO)} Prefab '{SelectedGO.name}'");

        UnpackPrefab(SelectedGO, true);
    }

    [MenuItem("GameObject/Kob Prefab/RePack Prefab From Stack", false, -100)]
    public static void KobPackPrefab()
    {
        if (PrefabDataStack.Count > 0)
        {
            Debug.LogError($"RePacking From Stack Prefab {PrefabDataStack.Peek().Root.name}");

            RePackPrefabFromStack();
        }
        else
        {
            Debug.LogError("Nothing to Re Pack, Stack is empty");
        }
    }

    [MenuItem("GameObject/Kob Prefab/---------------------", false, -100)]
    public static void KobSpacer1() { }

    [MenuItem("GameObject/Kob Prefab/Remove Prefab Child", false, -100)]
    public static void KobRemovePrefabChild()
    {
        Debug.LogError($"Removed Prefab Child - TBD");
    }

    [MenuItem("GameObject/Kob Prefab/--------------------", false, -100)]
    public static void KobSpacer2() { }

    [MenuItem("GameObject/Kob Prefab/Prefab Nearest Root?", false, -100)]
    public static void PrefabNearestInstanceRoot()
    {
        Debug.LogError($"Prefab '{SelectedGO.name}' nearest root is {PrefabUtility.GetNearestPrefabInstanceRoot(SelectedGO).name}");
    }

    [MenuItem("GameObject/Kob Prefab/Prefab Outermost Root?", false, -100)]
    public static void PrefabOutermostInstanceRoot()
    {
        Debug.LogError($"Prefab '{SelectedGO.name}' outermost root is {PrefabUtility.GetOutermostPrefabInstanceRoot(SelectedGO).name}");
    }

    [MenuItem("GameObject/Kob Prefab/Prefab Type?", false, -100)]
    public static void PrefabType()
    {
        Debug.LogError($"Prefab '{SelectedGO.name}' is {GetPrefabType(SelectedGO)}");
    }

    [MenuItem("GameObject/Kob Prefab/Print Prefab Data Stack", false, -100)]
    public static void PrefabStackPrint()
    {
        PrintPrefabDataStack();
    }

    // ------------------------------------------------------------ //

    public static void ApplyPrefab(GameObject i_GameObject)
    {
        PrefabUtility.ApplyPrefabInstance(i_GameObject, InteractionMode.AutomatedAction);
    }

    public static void UnpackPrefab(GameObject i_GameObject, bool i_UnPackVariantOnly = false)
    {
        GameObject outermostPrefabInstance;
        string outermostPrefabInstancePath;

        
        while (PrefabUtility.IsAnyPrefabInstanceRoot(i_GameObject) && (i_UnPackVariantOnly && IsPrefabVariant(i_GameObject) || !i_UnPackVariantOnly))
        {
            outermostPrefabInstance = PrefabUtility.GetOutermostPrefabInstanceRoot(i_GameObject);
            outermostPrefabInstancePath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(outermostPrefabInstance);

            PrefabDataStack.Push(new PrefabData(outermostPrefabInstancePath, outermostPrefabInstance));

            Debug.LogError($"+ UnPacked Root {GetPrefabType(SelectedGO)} Prefab '{outermostPrefabInstance.name}' to unpack Prefab '{i_GameObject.name}'");

            PrefabUtility.UnpackPrefabInstance(outermostPrefabInstance, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);            
        }
    }

    public static Stack<PrefabData> PrefabDataStack = new Stack<PrefabData>();

    public static void RePackPrefabFromStack()
    {
        int count = PrefabDataStack.Count;
        for (int i = 0; i < count; i++)
        {
            PrefabData item = PrefabDataStack.Pop();

            PrefabUtility.SaveAsPrefabAssetAndConnect(item.Root, item.Path, InteractionMode.AutomatedAction);

            if (i == 0) Debug.LogError("----------------------------------");

            Debug.LogError($"{i}. Packing Prefab {GetPrefabType(item.Root)} '{item.Root.name}' Path: {item.Path}");

            if (i + 1 == count) Debug.LogError("Finished.");
        }
    }

    public static void PrintPrefabDataStack()
    {
        int count = PrefabDataStack.Count;
        for (int i = 0; i < count; i++)
        {
            PrefabData item = PrefabDataStack.Pop();

            Debug.LogError($"{i}. Prefab {GetPrefabType(item.Root)} '{item.Root.name}' Path: {item.Path}");
        }
    }

    public static bool IsPrefabVariant(Object i_Object)
    {
        return PrefabUtility.IsPartOfVariantPrefab(i_Object);
    }

    public static bool IsPrefabRegular(Object i_Object)
    {
        return PrefabUtility.IsPartOfRegularPrefab(i_Object);
    }

    public static string GetPrefabType(Object i_Object)
    {
        string result = "";

        if (IsPrefabVariant(i_Object) == true)  result = "Variant";
        if (IsPrefabRegular(i_Object) == true)  result = "Regular";

        return result;
    }

    public class PrefabData
    {
        public string Path;
        public GameObject Root;

        public PrefabData(string i_Path, GameObject i_Root)
        {
            this.Path = i_Path;
            this.Root = i_Root;
        }
    }
}
#endif