using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;

namespace KobGamesSDKSlim
{
    public class WebToolsManager : MonoBehaviour
    {
#if ENABLE_WEBTOOLS
        public static event Action OnReady = () => { };

        public int PlayerId;
        public int SessionId;

        public bool IsReady()
        {
            return this.PlayerId > 0 && this.SessionId > 0;
        }

        private static string k_WebApiRoot = "https://webtools.kobgames.com/extorio/apis";
        public bool LogWebRequestResponses = false;

        private void OnEnable()
        {
            RemoteSettingsManager.OnFirebaseRemotConfigUpdated += OnFirebaseRemotConfigUpdated;
        }

        private void OnDisable()
        {
            RemoteSettingsManager.OnFirebaseRemotConfigUpdated -= OnFirebaseRemotConfigUpdated;
        }

        private void OnFirebaseRemotConfigUpdated(bool i_IsUpdated)
        {
            this.UpdatePlayerSessionVersion();
        }

        private void Start()
        {
            this.GetPlayerSession();
        }

        private string m_ABTestVersionSet = "unset";
        public void GetPlayerSession(Action i_OnReady = null)
        {
            if (this.IsReady())
            {
                i_OnReady?.Invoke();
            }
            else
            {
                this.m_ABTestVersionSet = Managers.Instance.RemoteSettings.FirebaseRemoteConfig.ABTestVersion.Value;
                this.WebReq("/game-players/create_session", new Dictionary<string, string>()
                {
                    //{ "UDID", SystemInfo.deviceUniqueIdentifier },
                    { "gameProfileId", GameSettings.Instance.General.AppStoreId },
                    { "version", Application.version + this.m_ABTestVersionSet },
                    { "deviceModel", SystemInfo.deviceModel },
                    { "operatingSystem", SystemInfo.operatingSystem },
                    { "processorName", SystemInfo.processorType },
                    { "processorCount", SystemInfo.processorCount.ToString() },
                    { "systemMemorySize", SystemInfo.systemMemorySize.ToString() },
                    { "graphicMemorySize", SystemInfo.graphicsMemorySize.ToString() }
                },
                data =>
                {
                    this.PlayerId = int.Parse(data["playerId"].ToString());
                    this.SessionId = int.Parse(data["sessionId"].ToString());
                    i_OnReady?.Invoke();
                    OnReady.Invoke();
                },
                s => { });
            }
        }

        public void UpdatePlayerSessionVersion()
        {
            if (this.IsReady() && this.m_ABTestVersionSet != Managers.Instance.RemoteSettings.FirebaseRemoteConfig.ABTestVersion.Value)
            {
                this.m_ABTestVersionSet = Managers.Instance.RemoteSettings.FirebaseRemoteConfig.ABTestVersion.Value;
                this.WebReq("/game-players/update_session_version", new Dictionary<string, string>()
                {
                    {"sessionId", this.SessionId.ToString()},
                    {"version", Application.version + this.m_ABTestVersionSet}
                },
                data => { },
                s => { });
            }
        }

        public void WebReq(string i_Endpoint, Dictionary<string, string> i_Data, Action<JsonData> i_OnSuccess, Action<string> i_OnFail)
        {
            this.StartCoroutine(this.HandleReq(UnityWebRequest.Post(k_WebApiRoot + i_Endpoint, i_Data), i_OnSuccess, i_OnFail));
        }

        private IEnumerator HandleReq(UnityWebRequest i_Req, Action<JsonData> i_OnSuccess, Action<string> i_OnFail)
        {
            yield return i_Req.SendWebRequest();

            if (i_Req.isNetworkError)
            {
                Debug.LogError("WebTools web request error: " + i_Req.error);
                i_OnFail.Invoke(i_Req.error);
            }
            else
            {
                try
                {
					JsonData data = JsonMapper.ToObject(i_Req.downloadHandler.text);
                    if (!data.Keys.Contains("error")) 
                    {
                        Debug.LogError("WebTools JsonMapper failed to decode json. Response from server: " + i_Req.downloadHandler.text);
                    } else if ((bool)data["error"])
                    {
                        Debug.LogError("WebTools extorio error: " + data["error_message"]);
                    }
                    else
                    {
                        if (this.LogWebRequestResponses)
                        {
                            Debug.Log("WebTools web request response: \n\n" + i_Req.downloadHandler.text);
                        }
                        i_OnSuccess.Invoke(data);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("WebTools handler error: " + ex.Message + "\n" + ex.StackTrace + "\n\nWEB RESPONSE:\n" + i_Req.downloadHandler.text);
                    i_OnFail.Invoke(ex.Message);
                }
            }
        }
#endif
    }
}
