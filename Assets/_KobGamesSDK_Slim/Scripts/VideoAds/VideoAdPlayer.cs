using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

namespace KobGamesSDKSlim
{
    public class VideoAdPlayer : MonoBehaviour
    {
        public VideoPlayer VideoPlayer;
        public bool PlayWhenReady = true;
        public string VideoAdUrlBase64;
        public string VideoAdUrlMP4;

        private const string k_AdName = "VideoAd.mp4";

        private void Start()
        {
            this.StartCoroutine(this.DownloadBase64());
        }

        private IEnumerator DownloadBase64()
        {
            using (UnityWebRequest req = UnityWebRequest.Get(VideoAdUrlBase64))
            {
                yield return req.SendWebRequest();

                if (req.isNetworkError || req.isHttpError)
                {
                    Debug.LogError("Failed to download base64 string: " + req.error);
                }
                else
                {
                    File.WriteAllBytes($"{Application.persistentDataPath}/{k_AdName}", Convert.FromBase64String(req.downloadHandler.text));
                    this.VideoPlayer.url = $"file://{Application.persistentDataPath}/{k_AdName}";
                    this.VideoPlayer.Prepare();
                }
            }
        }
        
        private void Update()
        {
            if (this.PlayWhenReady && this.VideoPlayer != null && this.VideoPlayer.isPrepared && !this.VideoPlayer.isPlaying)
            {
                this.transform.GetChild(0).gameObject.SetActive(true);
                this.VideoPlayer.Play();
            }
        }

        private void Reset()
        {
            if (VideoPlayer == null)
            {
                VideoPlayer = this.GetComponentInChildren<VideoPlayer>();
            }
        }
    }
}