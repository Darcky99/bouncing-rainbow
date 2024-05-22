using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using KobGamesSDKSlim;

public class SaveSquadsLevel
{
    #region Saving data
    public SquadData[] Squads;
    public int SquadCount;
    public int Coins;
    public int IncomeLevel;

    public SaveSquadsLevel() { }
    public SaveSquadsLevel(SquadsLevel i_SquadLevel)
    {
        this.Squads = new SquadData[i_SquadLevel.Squads.Length];
        this.SquadCount = i_SquadLevel.SquadCount;
        this.Coins = StorageManager.Instance.CoinsAmount;
        this.IncomeLevel = i_SquadLevel.IncomeLevel;

        for (int i = 0; i < this.Squads.Length; i++)
        {
            this.Squads[i] = new SquadData(i_SquadLevel.Squads[i]);
        }
    }
    public class SquadData {
        public Dictionary<int, int> LinesByLevel;

        public SquadData() { }
        public SquadData(Squad i_Squad) {
            LinesByLevel = i_Squad.LinesByLevel;
        }
    }
    #endregion

    #region Methods
    private const string k_KEY = "SALAMI";

    public static void Save(SquadsLevel i_Data)
    {
        SaveSquadsLevel i_dataToSave = new SaveSquadsLevel(i_Data);

        string i_rainbowSquadsData = JsonConvert.SerializeObject(i_dataToSave);
        PlayerPrefs.SetString(k_KEY, i_rainbowSquadsData);
    }
    public static bool TryLoad(out SaveSquadsLevel i_GameData)
    {
        i_GameData = null;
        if (PlayerPrefs.HasKey(k_KEY))
        {
            string i_rainbowSquadsData = PlayerPrefs.GetString(k_KEY);
            i_GameData = JsonConvert.DeserializeObject<SaveSquadsLevel>(i_rainbowSquadsData);
            return true;
        }
        return false;
    }
    public static bool Exists()
    {
        return PlayerPrefs.HasKey(k_KEY);
    }
    public static void DeleteSaveFile()
    {
        if (PlayerPrefs.HasKey(k_KEY))
        {
            PlayerPrefs.DeleteKey(k_KEY);
        }
    }
    #endregion
}