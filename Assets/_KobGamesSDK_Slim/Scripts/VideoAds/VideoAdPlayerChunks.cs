﻿using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Video;

public class VideoAdPlayerChunks : MonoBehaviour
{
    public VideoPlayer VideoPlayer;
    #region base64video
    #endregion

    private void Start()
    {
        DownloadBase64Immediate();
    }

    private void DownloadBase64Immediate()
    {
        File.WriteAllBytes(Application.persistentDataPath + "/videoad.mp4", Convert.FromBase64String(base64video));
        //this.m_Status = VideoStatus.prepairing;
        this.VideoPlayer.url = "file://" + Application.persistentDataPath + "/videoad.mp4";
        this.VideoPlayer.Prepare();
    }

    private void Update()
    {
        if (this.VideoPlayer.isPrepared && !this.VideoPlayer.isPlaying)
        {
            this.VideoPlayer.Play();
        }
    }

    [Button]
    public void OpenPersistentPath()
    {
        Application.OpenURL("file://" + Application.persistentDataPath);
    }

    [Button]
    public void CreateBase64Chunks()
    {
        ConvertToStringChunks(base64video, 600000);
    }

    public void ConvertToStringChunks(string i_Base64Video, int i_ChunkSize = 600000)
    {
        Debug.Log($"String Length: {i_Base64Video.Length}");
        Debug.Log($"Bytes Length: {Convert.FromBase64String(base64video).Length}");


        //string byteStringCompressed = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64video), 0, Convert.FromBase64String(base64video).Length);
        //string byteStringCompressed = Convert.FromBase64String(base64video, 0, );


        //Debug.Log($"Compressed String Length: {byteStringCompressed.Length}");
        //Debug.Log($"String from Compress Length: {byteString.Length}");

        string stringToWrite = "";
        int chunkSize = 0;
        int i = 0;
        int maxParts = 5;
        while (i_ChunkSize <= i_Base64Video.Length && i_Base64Video.Length > 0 && i < maxParts)
        {
            chunkSize = Mathf.Min(i_Base64Video.Length, i_ChunkSize);
            stringToWrite = i_Base64Video.Substring(0, chunkSize);
            i_Base64Video = i_Base64Video.Substring(chunkSize);

            Debug.Log($"{Application.persistentDataPath}/part_{i}.txt Length={stringToWrite.Length} ChunkSize={chunkSize} Left={i_Base64Video.Length}");
            File.WriteAllText($"{Application.persistentDataPath}/part_{i}.txt", stringToWrite);

            i++;

            if (i_Base64Video.Length > 0 && i_Base64Video.Length < chunkSize)
            {
                i_ChunkSize = i_Base64Video.Length;
                //i = 0;
            }
        }
    }
}