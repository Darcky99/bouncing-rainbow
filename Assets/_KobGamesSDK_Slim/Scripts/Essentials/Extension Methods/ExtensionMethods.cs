using UnityEngine;
using System;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KobGamesSDKSlim
{
    public static class ExtensionMethods
    {
#if UNITY_EDITOR
        public static void AddPrefab(this GameObject i_GameObject, GameObject i_PrefabGameObject)
        {
            AddPrefab(i_GameObject, UnityEditor.AssetDatabase.GetAssetPath(i_PrefabGameObject));
        }

        public static void AddPrefab(this GameObject i_GameObject, string i_PrefabPath)
        {
            try
            {               
                var rootPrefab = i_GameObject.GetOutermostPrefabInstanceRoot();
                var rootPrefabPath = rootPrefab.GetPrefabAssetPathOfNearestInstanceRoot();

                i_GameObject.UnpackPrefabInstanceNearest();

                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(i_PrefabPath);
                PrefabUtility.InstantiatePrefab(obj, i_GameObject.transform);

                rootPrefab.ApplyToPrefab(rootPrefabPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Ex: {ex.Message} Stack: {ex.StackTrace}");
            }
        }

        public static void RemovePrefab(this GameObject i_GameObject, GameObject i_DestroyObject)
        {
            try
            {
                var rootPrefab = i_GameObject.GetOutermostPrefabInstanceRoot();
                var rootPrefabPath = rootPrefab.GetPrefabAssetPathOfNearestInstanceRoot();

                i_GameObject.UnpackPrefabInstanceNearest();

                UnityEngine.Object.DestroyImmediate(i_DestroyObject);

                PrefabUtility.SaveAsPrefabAssetAndConnect(rootPrefab, rootPrefabPath, InteractionMode.UserAction);
                //rootPrefab.ApplyToPrefab(rootPrefabPath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"RemovePrefab() Ex: {ex.Message} Stack: {ex.StackTrace}");
            }
        }

        public static void ApplyToPrefab(this GameObject i_GameObject, string i_PrefabPath = "")
        {
            if (!EditorApplication.isPlaying)
            {
                string prefabType = PrefabHelper.GetPrefabType(i_GameObject);

                var m_PrefabPath = i_PrefabPath == "" ? PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(i_GameObject) : i_PrefabPath;
                var m_PrefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(i_GameObject);

                Debug.LogWarning($"ApplyToPrefab Prefab {prefabType} of {i_GameObject.name} '{m_PrefabRoot.name}' Path: {m_PrefabPath}");

                if (m_PrefabRoot != null && m_PrefabPath != string.Empty)
                {
                    FindAndRemoveMissingScriptsInSelected(m_PrefabRoot);
                    PrefabUtility.SaveAsPrefabAssetAndConnect(m_PrefabRoot, m_PrefabPath, UnityEditor.InteractionMode.UserAction);
                }
            }
            else
            {
                Debug.LogWarning("Won't allow to Apply Prefab on play mode - dangerous");
            }
        }

        public static bool IsPrefab(this GameObject i_GameObject)
        {
            return PrefabUtility.IsAnyPrefabInstanceRoot(i_GameObject);
        }

        public static bool IsPrefabOutermostRoot(this GameObject i_GameObject)
        {
            return PrefabUtility.IsOutermostPrefabInstanceRoot(i_GameObject);
        }

        public static GameObject GetOutermostPrefabInstanceRoot(this GameObject i_GameObject)
        {
            return PrefabUtility.GetOutermostPrefabInstanceRoot(i_GameObject);
        }

        public static void UnpackPrefabInstanceOutermost(this GameObject i_GameObject)
        {
            PrefabUtility.UnpackPrefabInstance(i_GameObject.GetOutermostPrefabInstanceRoot(),
                PrefabUnpackMode.OutermostRoot,
                InteractionMode.UserAction);
        }

        public static GameObject GetNearestPrefabInstanceRoot(this GameObject i_GameObject)
        {
            return PrefabUtility.GetNearestPrefabInstanceRoot(i_GameObject);
        }

        public static void UnpackPrefabInstanceNearest(this GameObject i_GameObject)
        {
            if (PrefabUtility.IsAnyPrefabInstanceRoot(i_GameObject.GetNearestPrefabInstanceRoot()))
            {
                PrefabUtility.UnpackPrefabInstance(i_GameObject.GetNearestPrefabInstanceRoot(),
                   PrefabUnpackMode.OutermostRoot,
                   InteractionMode.UserAction);
            }
        }

        public static string UnpackPrefabInstanceNearestAndReturnPath(this GameObject i_GameObject)
        {
            string result = string.Empty;

            if (PrefabUtility.IsAnyPrefabInstanceRoot(i_GameObject.GetNearestPrefabInstanceRoot()))
            {
                result = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(i_GameObject.GetNearestPrefabInstanceRoot());

                PrefabUtility.UnpackPrefabInstance(i_GameObject.GetNearestPrefabInstanceRoot(),
                   PrefabUnpackMode.OutermostRoot,
                   InteractionMode.UserAction);
            }
            else
            {
                Debug.LogError($"{Utils.GetFuncName()}(): GO: {i_GameObject}, Couldn't find prefab root");
            }

            return result;
        }

        public static void ApplyPrefabInstance(this GameObject i_PrefabRoot)
        {
            PrefabUtility.ApplyPrefabInstance(i_PrefabRoot, InteractionMode.UserAction);
        }

        public static string GetPrefabAssetPathOfNearestInstanceRoot(this GameObject i_PrefabRoot)
        {
            return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(i_PrefabRoot);
        }

        public static void FindAndRemoveMissingScriptsInSelected(params GameObject[] i_GameObjects)
        {
            var prefabs = new HashSet<UnityEngine.Object>();
            int compCount = 0;
            int goCount = 0;
            foreach (var go in i_GameObjects)
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                if (count > 0)
                {
                    if (PrefabUtility.IsPartOfAnyPrefab(go))
                    {
                        RecursivePrefabSource(go, prefabs, ref compCount, ref goCount);
                        count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                        // if count == 0 the missing scripts has been removed from prefabs
                        if (count == 0)
                            continue;
                        // if not the missing scripts must be prefab overrides on this instance
                    }

                    Undo.RegisterCompleteObjectUndo(go, "Remove missing scripts");
                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                    compCount += count;
                    goCount++;
                }
            }

            if (compCount > 0 || goCount > 0)
                Debug.LogError($"Found and removed {compCount} missing scripts from {goCount} GameObjects");
        }

        // Prefabs can both be nested or variants, so best way to clean all is to go through them all
        // rather than jumping straight to the original prefab source.
        public static void RecursivePrefabSource(GameObject instance, HashSet<UnityEngine.Object> prefabs, ref int compCount, ref int goCount)
        {
            var source = PrefabUtility.GetCorrespondingObjectFromSource(instance);
            // Only visit if source is valid, and hasn't been visited before
            if (source == null || !prefabs.Add(source))
                return;

            // go deep before removing, to differantiate local overrides from missing in source
            RecursivePrefabSource(source, prefabs, ref compCount, ref goCount);

            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(source);
            if (count > 0)
            {
                Undo.RegisterCompleteObjectUndo(source, "Remove missing scripts");
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(source);
                compCount += count;
                goCount++;
            }
        }
#endif

        public static void ShowDebugErrorIfNull<T>(this T i_Target, string i_ClassName, string i_FuncName, string i_VariableName)
        {
            if (i_Target == null)
            {
                Debug.LogErrorFormat("{0}: {1} @ {2} is null", i_ClassName, i_FuncName, i_VariableName);
            }
        }

        public static bool Contains<T>(this T[] i_Array, T i_Item)
        {
            bool result = false;

            if (i_Array != null && i_Item != null)
            {
                foreach (var x in i_Array)
                {
                    if (x != null && x.Equals(i_Item))
                    {
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        public static void ForEach2<T>(this T[] i_Array, Action<T> i_Action)
        {
            foreach (var x in i_Array)
            {
                i_Action.InvokeSafe(x);
            }
        }

        public static bool IsNull(this UnityEngine.Object obj)
        {
            return obj == null || !obj;
        }

        public static T DeQueue<T>(this IList<T> i_List)
        {
            T item = default(T);

            if (i_List.Count > 0)
            {
                item = i_List[0];
                i_List.RemoveAt(0);
            }

            return item;
        }

        public static T GetRandomItem<T>(this IList<T> i_List)
        {
            T result = default(T);

            result = i_List[UnityEngine.Random.Range(0, i_List.Count)];

            return result;
        }

        public static void GetRandomItem(this Type i_List)
        {
        }

        public static void EnQueue<T>(this IList<T> i_List, T i_Item)
        {
            i_List.Add(i_Item);
        }

        public static void Shuffle<T>(this IList<T> i_List)
        {
            for (int i = 0; i < i_List.Count; i++)
            {
                i_List.Swap(i, UnityEngine.Random.Range(i, i_List.Count));
            }
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static Sprite ConvertToSprite(this Texture2D i_Texture)
        {
            return Sprite.Create(i_Texture, new Rect(0, 0, i_Texture.width, i_Texture.height), new Vector2(0.5f, 0.5f), 100);
        }

        public static void Process(this Exception i_Exception, string i_Class, string i_FunctionName)
        {
            string errorMsg = string.Format("{0}-{1} - Error: {2}  Stack: {3}", i_Class, i_FunctionName, i_Exception.Message, i_Exception.StackTrace);

            Debug.LogError(errorMsg);

            //GoogleAnalyticsV4.instance.LogEvent("Exception", string.Format("{0}-{1}", i_Class, i_FunctionName), errorMsg, 0);
        }

        public static string SetColor(this string i_String, Color i_Color)
        {
            return string.Format("<color=#{0}>{1}</color>", ColorUtility.ToHtmlStringRGB(i_Color), i_String);
        }

        //    public static string Reverse(this string s)
        //    {
        //        char[] charArray = s.ToCharArray();
        //        Array.Reverse(charArray);
        //        return new string(charArray);
        //    }

        public static Boolean ToBoolean(this int i_Int)
        {
            return i_Int == 1 ? true : false;
        }

        public static int ToInt(this bool i_Bool)
        {
            return i_Bool == true ? 1 : 0;
        }

        /// Use this method to get all loaded objects of some type, including inactive objects. 
        /// This is an alternative to Resources.FindObjectsOfTypeAll (returns project assets, including prefabs), and GameObject.FindObjectsOfTypeAll (deprecated).
        public static List<T> FindObjectsOfTypeAll<T>()
        {
            List<T> results = new List<T>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.isLoaded)
                {
                    var allGameObjects = s.GetRootGameObjects();
                    for (int j = 0; j < allGameObjects.Length; j++)
                    {
                        var go = allGameObjects[j];
                        results.AddRange(go.GetComponentsInChildren<T>(true));
                    }
                }
            }
            return results;
        }

        public static void AddListener(this Button i_Button, UnityAction i_Action)
        {
            if (i_Button != null && i_Action != null)
            {
                i_Button.onClick.AddListener(i_Action);
            }
        }

        public static void RemoveListener(this Button i_Button, UnityAction i_Action)
        {
            if (i_Button != null && i_Action != null)
            {
                i_Button.onClick.RemoveListener(i_Action);
            }
        }

        public static void Reset(this Transform i_Transform)
        {
            i_Transform.localPosition = Vector3.zero;
            i_Transform.localRotation = Quaternion.identity;
            i_Transform.localScale = new Vector3(1, 1, 1);
        }

        public static void ReferenceCheck<T>(this T i_Item, string i_ClassName = "", string i_MemberName = "")
        {
            // i_Item.Equals(null) this is an important check since Unity "fake-null-objects" arent really null, this will check both cases
            if (i_Item == null || i_Item.Equals(null))
            {
                Debug.LogError(string.Format("{0}-{1}: Error, check your references!", i_ClassName, i_MemberName));
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                return;
#endif
            }
        }

        public static void ReferenceCheck<T>(this T[] i_Array, string i_ClassName = "", string i_MemberName = "")
        {
            foreach (var item in i_Array)
            {
                // item.Equals(null) this is an important check since Unity "fake-null-objects" arent really null, this will check both cases
                if (item == null || item.Equals(null))
                {
                    Debug.LogError(string.Format("{0}-{1}: Error, check your references!", i_ClassName, i_MemberName));
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                    return;
#else
                break;
#endif
                }
            }
        }

        public static Vector3 Change(this Vector3 org, float? x = null, float? y = null, float? z = null)
        {
            return new Vector3((x == null ? org.x : (float)x), (y == null ? org.y : (float)y), (z == null ? org.z : (float)z));
        }

        public static Quaternion ToQuaternion(this Vector3 i_Vector3)
        {
            return Quaternion.Euler(i_Vector3);
        }

        // WARNING: won't work, the list is being passed by value
        //public static void InitAndClearList<T>(this SortedList<int, T> i_List)
        //{
        //    if (i_List == null)
        //    {
        //        i_List = new SortedList<int, T>();
        //    }

        //    i_List.Clear();
        //}


        // WARNING: won't work, the list is being passed by value
        //public static void InitAndClearList<T>(this List<T> i_List)
        //{
        //    if (i_List == null)
        //    {

        //        i_List = new List<T>();
        //    }

        //    i_List.Clear();
        //}


        public static void RemoveIfNotNull<T>(this SortedList<int, T> i_List, T i_Value)
        {
            if (i_List != null && i_Value != null && i_List.ContainsValue(i_Value))
            {
                i_List.RemoveAt(i_List.IndexOfValue(i_Value));
            }
        }

        public static void AddIfNotNull<T>(this SortedList<int, T> i_List, T i_Value, Type i_Exclude, int i_KeyOrder = 0)
        {
            //Debug.LogError(i_Exclude.IsAssignableFrom(i_Value.GetType()) + " " + i_Value + " " + (i_Value is GameManagerBase));

            if (i_Value != null && (i_Exclude == null || !i_Exclude.IsAssignableFrom(i_Value.GetType())))
            {
                if (!i_List.ContainsValue(i_Value)) // Prevents duplication on the sortedlist
                {
                    // Incase the key already exists, we try to find the closest availble key
                    if (i_List.ContainsKey(i_KeyOrder))
                    {
                        for (int i = i_KeyOrder; i < 10000; i++)
                        {
                            i_KeyOrder = i;

                            if (!i_List.ContainsKey(i_KeyOrder))
                            {
                                break;
                            }
                        }
                    }

                    i_List.Add(i_KeyOrder, i_Value);
                }
            }
        }

        public static void AddIfNotNull<T>(this ICollection<T> i_List, T i_Value, Type i_Exclude)
        {
            //Debug.LogError(i_Exclude.IsAssignableFrom(i_Value.GetType()) + " " + i_Value + " " + (i_Value is GameManagerBase));

            if (i_Value != null && (i_Exclude == null || !i_Exclude.IsAssignableFrom(i_Value.GetType())))
            {
                i_List.AddIfNotNull(i_Value);
            }
        }

        public static void AddIfNotExists<T>(this ICollection<T> i_List, T i_Value)
        {
            if (i_Value != null && !i_List.Contains(i_Value))
            {
                i_List.AddIfNotNull(i_Value);
            }
        }

        public static void AddIfNotNull<T>(this ICollection<T> i_List, T i_Value)
        {
            if (i_Value != null)
            {
                i_List.Add(i_Value);
            }
        }

        public static void RemoveIfNotNull<T>(this ICollection<T> i_List, T i_Value)
        {
            if (i_List.Contains(i_Value) && i_Value != null)
            {
                i_List.Remove(i_Value);
            }
        }

        public static bool IsBetween(this float val, float low, float high)
        {
            return val >= low && val <= high;
        }

        public static bool IsBetween(this int val, int low, int high)
        {
            return val >= low && val <= high;
        }

        public static bool IsBetween(this int val, float low, float high)
        {
            return val >= low && val <= high;
        }

        public static void AddX(this Transform i_Transform, float i_X)
        {
            i_Transform.position = new Vector3(i_Transform.position.x + i_X, i_Transform.position.y, i_Transform.position.z);
        }

        public static void AddY(this Transform i_Transform, float i_Y)
        {
            i_Transform.position = new Vector3(i_Transform.position.x, i_Transform.position.y + i_Y, i_Transform.position.z);
        }

        public static void AddLocalY(this Transform i_Transform, float i_Y)
        {
            i_Transform.localPosition = new Vector3(i_Transform.localPosition.x, i_Transform.localPosition.y + i_Y, i_Transform.localPosition.z);
        }

        public static void SetX(this Transform i_Transform, float i_X)
        {
            i_Transform.position = new Vector3(i_X, i_Transform.position.y, i_Transform.position.z);
        }

        public static void SetY(this Transform i_Transform, float i_Y)
        {
            i_Transform.position = new Vector3(i_Transform.position.x, i_Y, i_Transform.position.z);
        }

        public static void SetZ(this Transform i_Transform, float i_Z)
        {
            i_Transform.position = new Vector3(i_Transform.position.x, i_Transform.position.y, i_Z);
        }

        public static void SetLocalX(this Transform i_Transform, float i_X)
        {
            i_Transform.localPosition = new Vector3(i_X, i_Transform.localPosition.y, i_Transform.localPosition.z);
        }

        public static void SetLocalY(this Transform i_Transform, float i_Y)
        {
            i_Transform.localPosition = new Vector3(i_Transform.localPosition.x, i_Y, i_Transform.localPosition.z);
        }

        public static void SetLocalZ(this Transform i_Transform, float i_Z)
        {
            i_Transform.localPosition = new Vector3(i_Transform.localPosition.x, i_Transform.localPosition.y, i_Z);
        }

        public static void SetLocalScaleX(this Transform i_Transform, float i_X)
        {
            i_Transform.localScale = new Vector3(i_X, i_Transform.localScale.y, i_Transform.localScale.z);
        }

        public static void SetLocalScaleY(this Transform i_Transform, float i_Y)
        {
            i_Transform.localScale = new Vector3(i_Transform.localScale.x, i_Y, i_Transform.localScale.z);
        }

        public static void SetLocalScaleZ(this Transform i_Transform, float i_Z)
        {
            i_Transform.localScale = new Vector3(i_Transform.localScale.x, i_Transform.localScale.y, i_Z);
        }

        public static void SetAlpha(this Material i_Material, float i_A)
        {
            i_Material.color = new Color(i_Material.color.r, i_Material.color.g, i_Material.color.b, i_A);
        }

        public static Color SetAlpha(this Color i_Color, float i_Alpha)
        {
            return new Color(i_Color.r, i_Color.g, i_Color.b, i_Alpha);
        }



        //public static void SetX(this Vector3 i_Vector3, float i_X)
        //{
        //    i_Vector3 = new Vector3(i_X, i_Vector3.y, i_Vector3.z);
        //}

        //public static void SetY(this Vector3 i_Vector3, float i_Y)
        //{
        //    i_Vector3 = new Vector3(i_Vector3.x, i_Y, i_Vector3.z);
        //}

        //public static void SetZ(this Vector3 i_Vector3, float i_Z)
        //{
        //    i_Vector3 = new Vector3(i_Vector3.x, i_Vector3.y, i_Z);
        //}

        public static Vector3 Clone(this Vector3 i_Vector3)
        {
            return new Vector3(i_Vector3.x, i_Vector3.y, i_Vector3.z);
        }

        public static Vector3 Round(this Vector3 i_Vector3)
        {
            float x = Mathf.Round(i_Vector3.x * 100f) / 100f;
            float y = Mathf.Round(i_Vector3.y * 100f) / 100f;
            float z = Mathf.Round(i_Vector3.z * 100f) / 100f;
            return new Vector3(x, y, z);
        }

        //public static void SetLayerRecursively(this Transform i_Transform, Layers i_Layer)
        //{
        //    SetLayerRecursively(i_Transform, i_Layer.ToString());
        //}

        public static void SetLayerRecursively(this Transform i_Transform, string i_LayerName)
        {
            int layerNumber = LayerMask.NameToLayer(i_LayerName);

            if (layerNumber >= 0)
            {
                i_Transform.SetLayerRecursively(layerNumber);
            }
        }

        public static void SetLayerRecursively(this Transform i_Transform, int i_LayerNumber)
        {
            if (i_LayerNumber >= 0)
            {
                i_Transform.gameObject.layer = i_LayerNumber;

                for (int i = 0; i < i_Transform.childCount; i++)
                {
                    i_Transform.GetChild(i).SetLayerRecursively(i_LayerNumber);
                }
            }
        }

        public static void SetActiveRecursivelyF(this GameObject i_GameObject, bool i_State, bool i_IncludeSelf = true)
        {
            if (i_IncludeSelf)
            {
                i_GameObject.SetActive(i_State);
            }

            for (int i = 0; i < i_GameObject.transform.childCount; i++)
            {
                i_GameObject.transform.GetChild(i).gameObject.SetActive(i_State);
            }
        }

        public static Transform Search(this Transform i_Target, string i_Name)
        {
            if (i_Target.name == i_Name) return i_Target;

            for (int i = 0; i < i_Target.childCount; ++i)
            {
                var result = Search(i_Target.GetChild(i), i_Name);

                if (result != null) return result;
            }

            return null;
        }

        public static T GetSafeComponent<T>(this GameObject obj) where T : MonoBehaviour
        {
            T component = obj.GetComponent<T>();

            if (component == null)
            {
                Debug.LogError(string.Format("Expected to find component of type {0} but found none {1}", typeof(T), obj));
            }

            return component;
        }


        /// <summary>
        /// Calls the delegate if it is not null
        /// </summary>
        /// <param name="i_Action">The action to be called</param>
        public static void InvokeSafe(this Action i_Action)
        {
            if (i_Action != null)
            {
                i_Action.Invoke();
            }
        }

        public static void InvokeSafe(this UnityEvent i_UnityEvent)
        {
            if (i_UnityEvent != null)
            {
                i_UnityEvent.Invoke();
            }
        }

        /// <summary>
        /// Calls the delegate if it is not null
        /// </summary>
        /// <param name="i_Action">The action to be called</param>
        /// <param name="i_Object">The object passed</param>
        public static void InvokeSafe<T>(this Action<T> i_Action, T i_Value)
        {
            if (i_Action != null)
            {
                i_Action.Invoke(i_Value);
            }
        }

        /// <summary>
        /// Calls the delegate if it is not null
        /// </summary>
        /// <param name="i_Action">The action to be called</param>
        /// <param name="i_Object">The object passed</param>
        public static void InvokeSafe<T, L>(this Action<T, L> i_Action, T i_Value1, L i_Value2)
        {
            if (i_Action != null)
            {
                i_Action.Invoke(i_Value1, i_Value2);
            }
        }

        public static void InvokeSafe(this UnityAction i_Action)
        {
            if (i_Action != null)
            {
                i_Action.Invoke();
            }
        }

        public static void InvokeSafe<T>(this UnityAction<T> i_Action, T i_Value1)
        {
            if (i_Action != null)
            {
                i_Action.Invoke(i_Value1);
            }
        }

        public static void InvokeSafe<T, L>(this UnityAction<T, L> i_Action, T i_Value1, L i_Value2)
        {
            if (i_Action != null)
            {
                i_Action.Invoke(i_Value1, i_Value2);
            }
        }

        public static void ActivateChildren(this GameObject i_GameObject, bool i_Activate)
        {
            i_GameObject.SetActive(i_Activate);

            foreach (Transform child in i_GameObject.transform)
            {
                ActivateChildren(child.gameObject, i_Activate);
            }
        }

        public static T GetComponentInChildrenNoRoot<T>(this Transform i_Transform) where T : Component
        {
            T result = null;
            T[] results = i_Transform.GetComponentsInChildren<T>();

            if (results.Length > 1)
                result = results[1];

            return result;
        }

        public static T GetComponenetByTagSafe<T>(this MonoBehaviour i_GameObject, string i_Tag)
        {
            T result = default(T);
            GameObject tmp = GameObject.FindGameObjectWithTag(i_Tag);

            if (tmp != null)
            {
                result = tmp.GetComponent<T>();
            }

            return result;
        }

        public static T[] GetComponenetsByTagSafe<T>(this MonoBehaviour i_GameObject, string i_Tag)
        {
            List<T> result = new List<T>();

            GameObject[] tmp = GameObject.FindGameObjectsWithTag(i_Tag);

            foreach (var item in tmp)
            {
                if (item.GetComponent<T>() != null)
                {
                    result.Add(item.GetComponent<T>());
                }
            }

            return result.ToArray();
        }

        public static T[] GetComponenetsInChildrenByTagSafe<T>(this MonoBehaviour i_GameObject, string i_Tag)
        {
            List<T> result = new List<T>();

            foreach (var go in i_GameObject.GetComponentsInChildren<Transform>(true))
            {
                if (go.tag == i_Tag)
                {
                    result.Add(go.GetComponent<T>());
                }
            }

            return result.ToArray();
        }

        public static T GetItem<T>(this List<T> i_List, int i_Index) where T : class
        {
            T result = default(T);

            if (i_List.Count > i_Index && i_Index >= 0)
            {
                result = i_List[i_Index];
            }

            return result;
        }

        /// <summary>
        /// Find GameObject in Children and afterwards 
        /// </summary>
        /// <param name="i_Parent"></param>
        /// <param name="i_Name"></param>
        /// <param name="i_IncludeInactive"></param>
        /// <returns></returns>
        public static T FindObject<T>(this GameObject i_Parent, string i_Name, bool i_IncludeInactive = true, bool i_FindGlobal = false)
        {
            var result = default(T);

            Transform[] trs = i_FindGlobal == false ? i_Parent.transform.GetComponentsInChildren<Transform>(i_IncludeInactive) : i_Parent.transform.root.GetComponentsInChildren<Transform>(i_IncludeInactive);
            foreach (Transform t in trs)
            {
                if (t.name.Equals(i_Name))
                {
                    result = (typeof(T) == typeof(GameObject)) ? (T)Convert.ChangeType(t.gameObject, typeof(T)) : t.GetComponent<T>();
                }
            }
            return result;
        }

        public static T FindObjectTag<T>(this GameObject i_Parent, string i_TagName, bool i_IncludeInactive = true, bool i_FindGlobal = false)
        {
            Transform[] trs;

            if (i_FindGlobal)
            {
                foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    trs = root.GetComponentsInChildren<Transform>(true);

                    foreach (Transform t in trs)
                    {
                        if (t.tag.Equals(i_TagName))
                        {
                            return t.gameObject.GetComponent<T>();
                        }
                    }
                }
            }
            else
            {
                trs = i_Parent.transform.GetComponentsInChildren<Transform>(i_IncludeInactive);

                foreach (Transform t in trs)
                {
                    if (t.tag.Equals(i_TagName))
                    {
                        return t.gameObject.GetComponent<T>();
                    }
                }
            }

            return default(T);
        }

        //public static UnityEngine.Object FindObject2(this UnityEngine.Object i_Parent, string i_Name, bool i_IncludeInactive = true)
        //{        
        //    //Debug.LogWarning(i_Parent.transform.root.root.name);
        //    //Transform[] trs = i_Parent.transform.root.root.GetComponentsInChildren<Transform>(i_IncludeInactive);
        //    //foreach (Transform t in trs)
        //    //{
        //    //    if (t.name == i_Name)
        //    //    {
        //    //        return t.gameObject;
        //    //    }
        //    //}
        //    return null;
        //}

        public static Transform FindParentThatHasChild(this Transform i_Parent, string i_Name, int i_Deep = 15)
        {
            int counter = 1;
            while (i_Parent != null && i_Parent.FindDeepChild(i_Name) == null && counter < i_Deep && i_Parent.parent != null)
            {
                i_Parent = i_Parent.parent;

                counter++;
            }

            return i_Parent;
        }



        //Breadth-first search
        public static Transform FindDeepChild(this Transform aParent, string aName)
        {
            if (aParent != null)
            {
                var result = aParent.Find(aName);
                if (result != null)
                    return result;

                foreach (Transform child in aParent)
                {
                    result = child.FindDeepChild(aName);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        public static T FindDeepChild<T>(this Transform aParent, string aName)
        {
            T result = default(T);

            var transform = aParent.FindDeepChild(aName);

            if (transform != null)
            {
                result = (typeof(T) == typeof(GameObject)) ? (T)Convert.ChangeType(transform.gameObject, typeof(T)) : transform.GetComponent<T>();
            }

            if (result == null)
            {
                Debug.LogError($"FindDeepChild didn't find: '{aName}' on GameObject: '{aParent.name}'");
            }

            return result;
        }

        //public static Tweener DOFlicker(this Text i_Target, Color i_TargetColor, Color i_ColorOrginal, float i_Duration)
        //{
        //    Tweener tween = i_Target.DOColor(i_TargetColor, i_Duration).OnComplete(() => i_Target.DOColor(i_ColorOrginal, i_Duration)).SetTarget(i_Target);

        //    return tween;
        //}

        // Only set parent if the parent and the input is different
        public static void SetParent(this MonoBehaviour i_Target, Transform i_Parent)
        {
            if (i_Target.transform.parent != i_Parent)
            {
                i_Target.transform.SetParent(i_Parent);
            }
        }

        public static Bounds ComputeTotalBounds(this Transform i_Target)
        {
            Debug.LogError("COMPUTE");
            var bounds = new Bounds();
            bounds.SetMinMax(
                new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
                new Vector3(float.MinValue, float.MinValue, float.MinValue));

            var renderersList = new List<Renderer>(i_Target.GetComponentsInChildren<Renderer>());
            renderersList.AddIfNotNull(i_Target.GetComponent<Renderer>());

            var count = 0;
            for (int i = 0; i < renderersList.Count; i++)
            {
                bounds.Encapsulate(renderersList[i].bounds);
                count++;
            }

            //#if UNITY_5_5_OR_NEWER
            //        var terrainsList = new List<Terrain>(i_Parent.GetComponentsInChildren<Terrain>());
            //        terrainsList.AddIfNotNull(i_Parent.GetComponent<Terrain>());
            //        foreach (var child in terrainsList)
            //        {
            //            bounds.Encapsulate(child.transform.TransformPoint(child.terrainData.bounds.min));
            //            bounds.Encapsulate(child.transform.TransformPoint(child.terrainData.bounds.max));
            //            count++;
            //        }
            //#endif
            if (count == 0)
                bounds.Encapsulate(i_Target.position);

            return bounds;
        }

        /// <summary>
        /// The callback will be invoked only once
        /// </summary>
        /// <param name="i_Action"></param>
        /// <param name="i_Callback"></param>
        public static void RegisterOnce(this UnityEvent i_Action, UnityAction i_Callback)
        {
            UnityAction tempAction = null;
            tempAction = () =>
            {
                i_Callback.InvokeSafe();
                i_Action.RemoveListener(tempAction);
            };

            i_Action.AddListener(tempAction);
        }

        //public delegate void MultiDelegate();    
        //// Return an Action since, Action is immutable reference type
        //public static Action RegisterOnceUnique(this Action i_Action, Action i_Callback)
        //{        
        //    //Debug.LogError("Exists: " + exists);

        //    Action tempAction = null;
        //    tempAction = () =>
        //    {
        //        if (i_Callback != null)
        //        {
        //            i_Callback.Invoke();
        //        }
        //        i_Action -= tempAction;
        //    };

        //    if (i_Action != null)
        //    {
        //        Delegate[] x = i_Action.GetInvocationList();

        //        bool exists = false;
        //        if (x.Length > 0)
        //        {
        //            Debug.LogError("Invocations Length: " + x.Length);
        //            exists = Array.Exists(x, y => y.Equals((Delegate)tempAction));
        //        }

        //        if (exists == false)
        //        {
        //            Debug.LogError("REGISTER");
        //            i_Action += tempAction;
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("REGISTER INIT");
        //        i_Action += tempAction;
        //    }

        //    return i_Action;
        //}

        //public static void OnInteractionEnterOnce(this Brick i_Brick, Action i_Callback)
        //{
        //    if (i_Brick != null)
        //    {
        //        i_Brick.ActionsManager.OnInteractionEnterOnce(i_Callback);
        //    }
        //}

        //public static void OnInteractionEnterOnce(this ActionsManager i_ActionManager, Action i_Callback)
        //{
        //    Action tempAction = null;
        //    tempAction = () =>
        //    {
        //        i_Callback.InvokeSafe();
        //        i_ActionManager.OnInteractionEnter -= tempAction;
        //    };

        //    i_ActionManager.OnInteractionEnter += tempAction;
        //}

        //public static void OnInteractionExitOnce(this ActionsManager i_ActionManager, Action i_Callback)
        //{
        //    Action tempAction = null;
        //    tempAction = () =>
        //    {
        //        i_Callback.InvokeSafe();
        //        i_ActionManager.OnInteractionExit -= tempAction;
        //    };

        //    i_ActionManager.OnInteractionExit += tempAction;
        //}

        //public static void PlayBackwards(this Tween i_Tween, TweenCallback i_Callback)
        //{
        //    if (i_Tween != null)
        //    {
        //        // Fix to make sure OnRewind will back called, even if there is no Playing Backwards to do
        //        //i_Tween.fullPosition = i_Tween.fullPosition <= 0 ? 0.01f : i_Tween.fullPosition;

        //        Debug.LogError(string.Format("Object: {0} \n IsBackwards: {1} IsPlaying: {2} fullPosition: {3}", i_Tween.target, i_Tween.IsBackwards(), i_Tween.IsPlaying(), i_Tween.fullPosition));

        //        i_Tween.onRewind += i_Callback;
        //        i_Tween.PlayBackwards();
        //    }
        //    else
        //    {
        //        Debug.LogError("Error in PlayBackwards() - No Tweener (ExtensionMethods.cs)");
        //    }
        //}

        //public static bool IsPlayForwardAvailable(this Tween i_Tween)
        //{
        //    bool result = false;

        //    if (i_Tween != null)
        //    {
        //        if (i_Tween.fullPosition > 0)
        //        {
        //            result = true;
        //        }
        //    }

        //    return result;
        //}

        public static void SetAlpha(this Text i_Text, float i_Alpha)
        {
            if (i_Text != null)
            {
                i_Text.color = new Color(i_Text.color.r, i_Text.color.g, i_Text.color.b, i_Alpha);
            }
        }

        private static readonly System.Random random = new System.Random();
        private static readonly object syncLock = new object();
        public static string GetUniqueHash()
        {
            lock (syncLock)
            { // synchronize
                int randomN = random.Next(Int32.MinValue, Int32.MaxValue);
                int currentDateHash = DateTime.Now.GetHashCode();
                return (randomN * currentDateHash).ToString();
            }
        }

#if UNITY_EDITOR
        public static void ObjectFieldLine(this UnityEditor.EditorGUILayout i_EditorGUILayout, UnityEngine.Object i_Object, Type i_ObjectType)
        {

        }
#endif
    }

    public static class PlayerPrefsBool
    {
        public static bool GetBool(string key, bool i_Default)
        {
            return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) == 1 : i_Default;
        }

        public static void SetBool(string key, bool state)
        {
            PlayerPrefs.SetInt(key, state ? 1 : 0);
        }
    }
}