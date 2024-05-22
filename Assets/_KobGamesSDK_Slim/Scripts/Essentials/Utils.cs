using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

namespace KobGamesSDKSlim
{
    public static class Utils
    {
        private static AudioSource[] m_AudioSources;

        public static bool RemoveFileOrDirectoryRelativePath(string i_Path)
        {
            try
            {
                bool anyDeleted = false;

                string pathToDelete = Application.dataPath + i_Path;

                if (File.Exists(pathToDelete))
                {
                    File.Delete(pathToDelete);

                    anyDeleted = true;
                    Debug.LogError($"Deleted {pathToDelete}");
                }
                else if (Directory.Exists(pathToDelete))
                {
                    Directory.Delete(pathToDelete, true);

                    anyDeleted = true;
                    Debug.LogError($"Deleted {pathToDelete}");
                }

                return anyDeleted;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}  Stack: {ex.StackTrace}");
                return false;
            }
        }

        public static void MuteAllAudioSources()
        {
            m_AudioSources = UnityEngine.Object.FindObjectsOfType<AudioSource>().Where(audioSource => audioSource.isPlaying && !audioSource.mute).ToArray();

            //Debug.LogError($"Found Audio Sources for Mute: {m_AudioSources.Count()}");

            if (m_AudioSources != null && m_AudioSources.Length > 0)
            {
                foreach (var audioSource in m_AudioSources)
                {
                    audioSource.mute = true;
                }
            }
        }

        public static void UnMuteAllAudioSources()
        {
            //Debug.LogError($"Found Audio Sources for UnMute: {m_AudioSources.Count()}");
            if (m_AudioSources != null && m_AudioSources.Length > 0)
            {
                foreach (var audioSource in m_AudioSources)
                {
                    if (audioSource != null)
                    {
                        audioSource.mute = false;
                    }
                }
            }

            if (m_AudioSources != null && m_AudioSources.Length == 0)
            {
                Debug.Log("Utils-UnMuteAllAudioSources: was called but found 0 AudioSources, please check if this behavior is correct.");
            }
        }

        public static string GetFuncName([CallerMemberName] string i_FuncName = "")
        {
            return i_FuncName;
        }

        public static void SetVertexColor(Color i_Color, Mesh i_Mesh)
        {
            Vector3[] vertices = i_Mesh.vertices;

            Color[] colors = new Color[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
                colors[i] = i_Color;

            i_Mesh.colors = colors;
        }

        public static void ClearConsole()
        {
            var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");

            var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

            clearMethod.Invoke(null, null);
        }

        public static void ToggleBool(ref bool i_Value)
        {
            i_Value = !i_Value;
        }

        #region Lists/Dict
        public static void ClearNullInList<T>(IList<T> i_List)
        {
            for (int index = i_List.Count - 1; index >= 0; index--)
            {
                if (i_List[index] == null)
                {
                    i_List.RemoveAt(index);
                }
            }
        }

        public static void ClearNullInDic<M, T>(IDictionary<M, T> i_SortedList)
        {
            List<M> keysToRemove = new List<M>();

            if (i_SortedList != null && i_SortedList.Count > 0)
            {
                foreach (var key in i_SortedList.Keys)
                {
                    if (i_SortedList[key] == null || i_SortedList[key].Equals(null))
                    {
                        keysToRemove.Add(key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    i_SortedList.Remove(key);
                }
            }
        }

        // This is needed, since we can't create extension methods (because they are being passed by value and not by ref to the extended method
        public static void InitAndClearDic<T>(ref SortedList<int, T> i_SortedList)
        {
            if (i_SortedList == null)
            {
                i_SortedList = new SortedList<int, T>();
            }

            i_SortedList.Clear();
        }
        #endregion

        #region OpenStore - Appstore/GooglePlay
        public static void OpenUrlStore(string i_GooglePlayId, string i_AppStoreId)
        {
#if UNITY_ANDROID
            if (i_GooglePlayId != string.Empty)
            {
                Application.OpenURL(string.Format("market://details?id={0}", i_GooglePlayId));
                Debug.LogFormat("OpenStore: market://details?id={0}", i_GooglePlayId);
                Managers.Instance.Analytics.LogEvent("OpenStoreURL", i_GooglePlayId);
            }
            else
            {
                Debug.LogError($"Utils-{nameof(OpenUrlStore)} GooglePlayId is Empty");
            }
#elif UNITY_IPHONE
            if (i_AppStoreId != string.Empty)
            {
                Application.OpenURL(string.Format("itms-apps://itunes.apple.com/app/id{0}", i_AppStoreId));
                Debug.LogFormat("OpenStore: itms-apps://itunes.apple.com/app/id{0}", i_AppStoreId);
                Managers.Instance.Analytics.LogEvent("OpenStoreURL", i_AppStoreId);
            }
            else
            {
                Debug.LogError($"Utils-{nameof(OpenUrlStore)} AppStoreId is Empty");
            }
#endif
        }
        #endregion

        #region OpenCrossPromoUrlStore - Appstore/GooglePlay

        public static void OpenCrossPromoUrlStore(string i_GooglePlayId, string i_AppStoreId)
        {
#if UNITY_ANDROID
            if (i_GooglePlayId != string.Empty)
            {
                Application.OpenURL(string.Format("market://details?id={0}", i_GooglePlayId));
                Debug.LogFormat("OpenStore: market://details?id={0}", i_GooglePlayId);
                Managers.Instance.Analytics.LogEvent("OpenCrossPromoUrlStore", i_GooglePlayId);
            }
            else
            {
                Debug.LogError($"Utils-{nameof(OpenCrossPromoUrlStore)} GooglePlayId is Empty");
            }
#elif UNITY_IPHONE
            if (i_AppStoreId != string.Empty)
            {
                Application.OpenURL(string.Format("itms-apps://itunes.apple.com/app/id{0}", i_AppStoreId));
                Debug.LogFormat("OpenStore: itms-apps://itunes.apple.com/app/id{0}", i_AppStoreId);
                Managers.Instance.Analytics.LogEvent("OpenCrossPromoUrlStore", i_AppStoreId);
            }
            else
            {
                Debug.LogError($"Utils-{nameof(OpenCrossPromoUrlStore)} AppStoreId is Empty");
            }
#endif
        }
        #endregion

        public static string GetUrlStore(string i_GooglePlayId, string i_AppStoreId)
        {
            string result = string.Empty;

#if UNITY_ANDROID
            if (i_GooglePlayId != string.Empty)
            {
                result = $"market://details?id={i_GooglePlayId}";
            }
            else
            {
                Debug.LogError($"Utils-{nameof(GetUrlStore)} GooglePlayId is Empty");
            }
#elif UNITY_IPHONE
            if (i_AppStoreId != string.Empty)
            {
                result = $"itms-apps://itunes.apple.com/app/id{i_AppStoreId}";
            }
            else
            {
                Debug.LogError($"Utils-{nameof(GetUrlStore)} AppStoreId is Empty");
            }
#endif
            return result;
        }

        public static string FetchAndroidManifestData(string i_NodeName, string i_AttributeName = "android:value")
        {
            //Debug.LogError(string.Concat(Application.dataPath, k_AndroidManifestPath.Replace("Assets", "")));
            string fetchedValue = string.Empty;

            string androidManifestFullPath = string.Concat(Application.dataPath, Constants.k_AndroidManifestPath.Replace("Assets", ""));

            XmlDocument doc = new XmlDocument();
            doc.Load(androidManifestFullPath);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            XmlNode root = doc.DocumentElement;
            XmlNode myNode = root.SelectSingleNode(string.Format("descendant::meta-data[@{0}]", i_NodeName), nsmgr);

            if (myNode != null)
            {
                //Debug.LogError((myNode as XmlElement).GetAttribute(i_AttributeName));

                fetchedValue = (myNode as XmlElement).GetAttribute("android:value");

                if (fetchedValue.IsNullOrEmpty())
                {
                    Debug.LogError(string.Format("AndroidManifest fetched succesfully. Node: {0} Value: {1}", i_NodeName, fetchedValue));
                }
            }

            if (fetchedValue.IsNullOrEmpty())
            {
                Debug.LogError($"AndroidManifest couldn't fetch: {i_NodeName}, got: {fetchedValue}");
            }

            return fetchedValue;
        }

        public static void SaveAndroidManifestData(string i_Value, string i_NodeName, string i_AttributeName = "android:value")
        {
            string androidManifestFullPath = string.Concat(Application.dataPath, Constants.k_AndroidManifestPath.Replace("Assets", ""));

            XmlDocument doc = new XmlDocument();
            doc.Load(androidManifestFullPath);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            XmlNode root = doc.DocumentElement;
            XmlNode myNode = root.SelectSingleNode(string.Format("descendant::meta-data[@{0}]", i_NodeName), nsmgr);

            if (myNode != null)
            {
                if ((myNode as XmlElement).HasAttribute("android:value"))
                {
                    var oldValue = (myNode as XmlElement).GetAttribute("android:value");
                    var newValue = i_Value;

                    if (oldValue != newValue)
                    {
                        (myNode as XmlElement).SetAttribute("android:value", i_Value);

                        doc.Save(androidManifestFullPath);

                        Debug.LogError(string.Format("AndroidManifest saved succesfully, Node: {0} Value: {1}", i_NodeName, i_Value));
                    }
                    //else
                    //{
                    //    Debug.LogError("AndroidManifest No change, same vlues");
                    //}
                }
            }
        }

        public static bool IsAttributionExists(string i_NodeName)
        {
            string androidManifestFullPath = string.Concat(Application.dataPath, Constants.k_AndroidManifestPath.Replace("Assets", ""));

            var xml = XElement.Load(androidManifestFullPath);

            XNamespace ab = "http://schemas.android.com/apk/res/android";

            XAttribute xAttribute = xml.DescendantNodes()
                                    .Select(ele => (ele as XElement))
                                    .SelectMany(attrs => attrs.Attributes())
                                    .Where(attrName => attrName.Value.Contains(i_NodeName))
                                    .FirstOrDefault();

            //Debug.LogError(xAttribute != null);

            return xAttribute != null;
        }

        public static string GetAndroidManifestPackageName()
        {
            string androidManifestFullPath = string.Concat(Application.dataPath, Constants.k_AndroidManifestPath.Replace("Assets", ""));

            XmlDocument doc = new XmlDocument();
            doc.Load(androidManifestFullPath);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            XmlElement root = doc.DocumentElement;
            string myAttribute = root.GetAttribute("package");

            string fetchedValue = myAttribute;// (myNode as XmlElement).GetAttribute("android:value");

            //Debug.LogError(string.Format("AndroidManifest fetched succesfully. Value: {0}", myAttribute));

            return fetchedValue;
        }

        public static void AddManifestNode(string i_AttributeName, string i_AttributeValue)
        {
            if (!IsAttributionExists(i_AttributeName))
            {
                string androidManifestFullPath = string.Concat(Application.dataPath, Constants.k_AndroidManifestPath.Replace("Assets", ""));

                var xml = XElement.Load(androidManifestFullPath);

                XNamespace ab = "http://schemas.android.com/apk/res/android";

                //<meta-data android:name="com.google.android.gms.ads.APPLICATION_ID" android:value="YOUR_ADMOB_APP_ID" />

                XElement elementApplication = xml.Descendants().Where(ele => ele.Name == "application").First();
                XElement element = new XElement("meta-data");
                XAttribute xattr1 = new XAttribute($"{ab + "name"}", i_AttributeName);
                XAttribute xattr2 = new XAttribute($"{ab + "value"}", i_AttributeValue);
                element.Add(xattr1);
                element.Add(xattr2);

                elementApplication.Add(element);

                xml.Save(androidManifestFullPath);

                Debug.LogError($"AndroidManifest: Created New Node, Name: {i_AttributeName} Value: {i_AttributeValue}");
            }
            //string androidManifestFullPath = string.Concat(Application.dataPath, Constants.k_AndroidManifestPath.Replace("Assets", ""));

            //XmlDocument doc = new XmlDocument();
            //doc.Load(androidManifestFullPath);

            //XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            //nsmgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            //XmlNode newNode = doc.CreateNode(XmlNodeType.Element, "meta-data", "abc");
            //doc.AppendChild(newNode);

            //doc.Save(androidManifestFullPath);
        }

        public static void AddManifestAttributeNew(string i_AttributeName, string i_AttributeValue)
        {
            string androidManifestFullPath = string.Concat(Application.dataPath, Constants.k_AndroidManifestPath.Replace("Assets", ""));

            var xml = XElement.Load(androidManifestFullPath);

            XNamespace ab = "http://schemas.android.com/apk/res/android";

            var newAttribute = new XAttribute(ab + i_AttributeName, i_AttributeValue);

            XElement element = xml.Descendants().Where(ele => ele.Name == "application").First();

            bool isAttributeExists = element.Attributes().Where(attr => attr.Name.LocalName.Contains(i_AttributeName)).FirstOrDefault() != null;
            if (!isAttributeExists)
            {
                element.Add(newAttribute);
                xml.Save(androidManifestFullPath);

                Debug.LogError($"AndroidManifest: Added {i_AttributeName}:{i_AttributeValue} to AndroiodManifest.xml");
            }
        }

        public static void RemoveManifestAttributeNew(string i_AttributeName)
        {
            string androidManifestFullPath = string.Concat(Application.dataPath, Constants.k_AndroidManifestPath.Replace("Assets", ""));

            var xml = XElement.Load(androidManifestFullPath);

            bool isAttributeExists = xml.Descendants().Where(x => x.Name == "application").Attributes()
                                 .Where(x => x.Name.LocalName.Contains("networkSecurityConfig")).FirstOrDefault() != null;

            if (isAttributeExists)
            {
                xml.Descendants().Where(x => x.Name == "application").Attributes()
                                     .Where(x => x.Name.LocalName.Contains("networkSecurityConfig"))
                                     .Remove();

                //foreach (var xx in y)
                //{
                //    Debug.LogError(xx.Name.LocalName);
                //}
                //.Remove();

                xml.Save(androidManifestFullPath);

                Debug.LogError($"AndroidManifest: Removed {i_AttributeName} from AndroiodManifest.xml");
            }
        }

        public static void RemoveManifestAttribute(string i_AttributeName)
        {
            string androidManifestFullPath = string.Concat(Application.dataPath, Constants.k_AndroidManifestPath.Replace("Assets", ""));

            XmlDocument doc = new XmlDocument();
            doc.Load(androidManifestFullPath);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            XmlNode root = doc.DocumentElement;
            XmlNode myNode = root.SelectSingleNode($"descendant::activity[@android:name='{i_AttributeName}']", nsmgr);
            XmlNode myNode1 = root.SelectSingleNode($"descendant::meta-data[@android:name='{i_AttributeName}']", nsmgr);

            if (myNode != null) myNode.ParentNode.RemoveChild(myNode);
            if (myNode1 != null) myNode1.ParentNode.RemoveChild(myNode1);

            if (myNode != null || myNode1 != null)
            {
                doc.Save(androidManifestFullPath);

                Debug.LogError($"AndroidManifest saved succesfully, Removed Attribute: {i_AttributeName}");
            }
        }

        public static void UpdateAndroidManifestPackageName(string i_PackageName)
        {
            string androidManifestFullPath = string.Concat(Application.dataPath, Constants.k_AndroidManifestPath.Replace("Assets", ""));

            XmlDocument doc = new XmlDocument();
            doc.Load(androidManifestFullPath);

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

            XmlElement root = doc.DocumentElement;
            string myAttribute = root.GetAttribute("package");

            if (myAttribute != i_PackageName && i_PackageName != string.Empty)
            {
                root.SetAttribute("package", i_PackageName);

                doc.Save(androidManifestFullPath);

                Debug.LogError(string.Format("AndroidManifest saved succesfully, New PackageName: {0}", i_PackageName));
            }
        }

        //public static void RemoveAppsFlyerFrmoManifest()
        //{
        //    string androidManifestFullPath = string.Concat(Application.dataPath, Constants.k_AndroidManifestPath.Replace("Assets", ""));

        //    XmlDocument doc = new XmlDocument();
        //    doc.Load(androidManifestFullPath);

        //    XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
        //    nsmgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

        //    XmlElement root = doc.DocumentElement;
        //    XmlNode myNode = root.SelectSingleNode("//receiver");

        //    if (myNode.HasValue())
        //    {
        //        myNode.ParentNode.RemoveChild(myNode);
        //        doc.Save(androidManifestFullPath);
        //    }
        //}

        public static int GetEnumLength(Type i_Enum)
        {
            return Enum.GetNames(i_Enum).Length;
        }

        //        public static void ShowRateUs(Action i_AndroidRateUsCallback = null)
        //        {
        //#if UNITY_IOS
        //            if (Device.RequestStoreReview())
        //            {
        //                i_AndroidRateUsCallback.InvokeSafe();
        //                //return false;
        //            }
        //#endif

        //#if UNITY_ANDROID
        //            i_AndroidRateUsCallback.InvokeSafe();
        //#endif
        //        }

        public static void QuitApplication()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        #region UI
        public static Vector3 WorldToUISpace(Canvas parentCanvas, Vector3 worldPos)
        {
            //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            Vector2 movePos;

            //Convert the screenpoint to ui rectangle local point
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, parentCanvas.worldCamera, out movePos);
            //Convert the local point to world point
            return parentCanvas.transform.TransformPoint(movePos);
        }

        public static Vector3 ScreenToUISpace(Canvas parentCanvas, Vector3 i_ScreenPos)
        {
            Vector2 movePos;

            //Convert the screenpoint to ui rectangle local point
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, i_ScreenPos, parentCanvas.worldCamera, out movePos);
            //Convert the local point to world point
            return parentCanvas.transform.TransformPoint(movePos);
        }
        #endregion

        public static int CopiedFiles = 0;
        public static int CopiedFolders = 0;
        public static void CopyFolder(string sourceFolder, string destFolder, bool i_Debug = true, bool i_Overwrite = false)
        {
            //Debug.LogError("CopyFolder: " + destFolder);

            if (!Directory.Exists(destFolder))
            {
                Directory.CreateDirectory(destFolder);
                Debug.Log($"Creating Directory... {destFolder}");
                CopiedFolders++;
            }

            string[] files = Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest, i_Overwrite);

                Debug.Log($"Copying... {dest}");
                CopiedFiles++;
            }

            string[] folders = Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, name);
                CopyFolder(folder, dest, i_Debug, i_Overwrite);
            }
        }

        public static void CopyFile(string sourceFile, string destFile, bool i_Overwrite = false)
        {
            if (File.Exists(sourceFile))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));

                File.Copy(sourceFile, destFile, i_Overwrite);
            }
        }

#if UNITY_EDITOR
        public static void EditorDelayRun(UnityEditor.EditorApplication.CallbackFunction i_Callback)
        {
            UnityEditor.EditorApplication.delayCall += i_Callback;
        }

        public static List<T> GetAssets<T>(string i_Filter, params string[] i_FoldersToSearch) where T : UnityEngine.Object
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets(i_Filter, i_FoldersToSearch);
            List<T> a = new List<T>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                a.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path));
            }
            return a;
        }

        public static T GetAsset<T>(string i_Filter, params string[] i_FoldersToSearch) where T : UnityEngine.Object
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets(i_Filter, i_FoldersToSearch);
            List<T> a = new List<T>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i]);
                a.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path));
            }
            return a.Count > 0 ? a[0] : null;
        }
#endif

        public static UnityEngine.Object InstantiateResourcesObject(ResourcesObject i_Object)
        {
#if UNITY_EDITOR
            if (i_Object.Prefab == null)
            {
                Debug.LogError("Trying to Instantiate Null Prefab. Please check reference.");
                return null;
            }

#endif

            return GameObject.Instantiate(Resources.Load(i_Object.Path));
        }


        public static bool TypeExists(params string[] i_Types)
        {
            return GetTypeByString(i_Types) != null;
        }


        public static Type GetTypeByName(string i_TypeName)
        {
            Type type = null;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                type = assembly.GetType(i_TypeName);
                if (type != null)
                {
                    //Debug.LogError(type.Name);
                    break;
                }
            }

            return type;
        }

        public static Type GetTypeByString(params string[] i_Types)
        {
            if (i_Types == null || i_Types.Length == 0)
                return null;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                foreach (string type in i_Types)
                {
                    if (assembly.GetType(type) != null)
                    {
                        return assembly.GetType(type);
                    }
                }
            }

            return null;
        }

        public static object GetFieldValue(string i_TypeName, string i_FieldName)
        {
            Type type = GetTypeByString(i_TypeName);

            if (type != null)
            {
                //Debug.LogError(type);

                // Properties:
                foreach (var x in type
                    .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField | BindingFlags.GetProperty)
                    .Where(y => y.Name == i_FieldName))
                {
                    //Debug.LogError(x.GetValue(type));
                    //Debug.LogError(x.GetRawConstantValue());
                    //Debug.LogError(type);

                    return x.GetValue(type);
                }

                // Fields:
                foreach (var x in type
                    .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField | BindingFlags.GetProperty)
                    .Where(y => y.Name == i_FieldName))
                {
                    //Debug.LogError(x.GetValue(type));
                    //Debug.LogError(x.GetRawConstantValue());
                    //Debug.LogError(type);

                    return x.GetValue(type);
                }

                // Methods:
                foreach (var x in type
                    .GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty)
                    .Where(y => y.Name == i_FieldName))
                {
                    var instance = GetFieldValue(type.ToString(), "afInstance");
                    return x.Invoke(instance, null);
                }
            }
            else
            {
                Debug.LogError($"GetFieldValue(): can't find type: {i_TypeName} field: {i_FieldName}");
            }

            return null;
        }
    }
}
