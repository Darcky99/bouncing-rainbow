using UnityEngine;
using System;
using Unity.Mathematics;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim
{
    [CreateAssetMenu(fileName = "GameConfig")]
    public class GameConfig : GameConfigBase
    {
        public DebugVariablesEditor    Debug    = new DebugVariablesEditor();
        public InputVariablesEditor    Input    = new InputVariablesEditor();
        public GamePlayVariablesEditor GamePlay = new GamePlayVariablesEditor();
        public LevelsVariablesEditor   Levels   = new LevelsVariablesEditor();
        public HUDVariablesEditor      HUD      = new HUDVariablesEditor();
        public MenuVariablesEditor     Menus    = new MenuVariablesEditor();
        public PlayerVariablesEditor   Player   = new PlayerVariablesEditor();
        public CameraVariablesEditor   Camera   = new CameraVariablesEditor();
        public HapticsVariablesEditor  Haptics  = new HapticsVariablesEditor();
    }

    [Serializable]
    public class DebugVariablesEditor : DebugVariablesEditorBase
    {
        //IMPORTANT - FOLLOW THIS TEMPLATE FOR DEBUG VARS SO THEY ARE NOT CALLED ON FINAL BUILDS
        //[SerializeField, ShowIf(nameof(DebugMode))] private bool m_VarName = false;
        //public bool VarName { get { return DebugMode && m_VarName; } }
    }

    [Serializable]
    public class InputVariablesEditor : InputVariablesEditorBase
    {
    }

    [Serializable]
    public class LevelsVariablesEditor : LevelsVariablesEditorBase
    {
        [Sirenix.OdinInspector.ValueDropdown(nameof(levelNames))] public string[] Levels;
        private Sirenix.OdinInspector.ValueDropdownList<string> levelNames()
        {
            return LevelManager.Instance.LevelNames;
        }
    }

    [Serializable]
    public class GamePlayVariablesEditor : GamePlayVariablesEditorBase
    {
        [TitleGroup("Lines")]
        [Header("Store")]
        [SerializeField] private int m_AddLinePrice = 2;
        [SerializeField] private int m_AddSquadPrice = 100;
        [SerializeField] private int m_IncomeLevelPrice = 200;
        [SerializeField] private int m_SquadNextLevelPrice;
        [SerializeField] private int m_AmountToMerge = 2;
        [Header("Earnings")]
        [SerializeField] private int m_CoinsPerLine = 5;
        [Header("Configuration")]
        [SerializeField] private float m_LineTimeOffset = 0.2f;
        [SerializeField] private float m_TapMultiplier = 1.2f;
        [SerializeField] private float m_SpeedLevelFactor = 1.1f;

        private SquadsLevel m_SquadsLevel => (SquadsLevel)LevelManager.Instance.CurrentLevel;

        public int AmountToMerge { get { return m_AmountToMerge; } }
        public int CoinsPerLine { get { return m_CoinsPerLine; } }
        public float LineTimeOffset { get { return m_LineTimeOffset; } }
        public float TapMultiplier { get { return m_TapMultiplier; } }
        public float SpeedLevelFactor { get { return m_SpeedLevelFactor; } }

        public int CurrentLineOrQuadPrice 
        { 
            get 
            {
                if (m_SquadsLevel.CanAddLine)
                {
                    int i_lineCount = m_SquadsLevel.GetTotalLineCount();
                    return Mathf.FloorToInt((1 + i_lineCount * i_lineCount * 0.8f) * m_AddLinePrice);
                }
                else if (m_SquadsLevel.CanAddSquad)
                {
                    int i_squadCount = m_SquadsLevel.SquadCount;
                    return i_squadCount * i_squadCount * m_AddSquadPrice;
                }
                else
                    return 500000; //maybe should log an error
            } 
        }
        public int CurrentIncomePrice
        {
            get
            {
                int i_levelSquared = (m_SquadsLevel.IncomeLevel * 2) + 1;
                return i_levelSquared * m_IncomeLevelPrice;
            }
        }
        public int SquadsNextLevelPrice
        {
            get
            {
                return m_SquadNextLevelPrice;
            }
        }

        [TitleGroup("Cirlces")]
        [Header("Economy")]
        [SerializeField] private int m_CirclePrice = 15;
        [SerializeField] private int m_GatePrice = 10;
        [SerializeField] private int m_CirclesNextLevelPrice = 35000;

        private CircleLevel m_CircleLevel => (CircleLevel)LevelManager.Instance.CurrentLevel;
        public int CircleEarning
        {
            get { return m_CircleEarning; }
        }
        public int CurrentCirclePrice
        {
            get
            {
                return m_CircleLevel.CircleCount < 1
                ? m_CirclePrice : m_CirclePrice * (m_CircleLevel.CircleCount + 1) * (m_CircleLevel.CircleCount + 1);
            }
        }
        public int CurrentGatePrice { get { return m_CircleLevel.EnabledGatesCount < 1
                    ? m_GatePrice : m_GatePrice * (m_CircleLevel.EnabledGatesCount + 1) * (m_CircleLevel.EnabledGatesCount + 1);
        } }
        public int CirclesNextLevelPrice
        {
            get { return m_CirclesNextLevelPrice; }
        }
        
        [Header("Earnings")]
        [SerializeField] private int m_CircleEarning = 1;

        [TitleGroup("Squares")]
        [Header("Squares")]
        [SerializeField] private float2 m_XCoordinateRange;
        [SerializeField] private float2 m_YCoordinateRange;

        public float2 XCoordinateRange { get { return m_XCoordinateRange; } }
        public float2 YCoordinateRange { get { return m_YCoordinateRange; } }
    }

    [Serializable]
    public class HUDVariablesEditor : HUDVariablesEditorBase
    {
    }

    [Serializable]
    public class MenuVariablesEditor : MenuVariablesEditorBase
    {
        public float ScreenFadeInDuration = .25f;
        public float ScreenFadeOutDuration = .15f;
    }

    [Serializable]
    public class PlayerVariablesEditor : PlayerVariablesEditorBase
    {
    }

    [Serializable]
    public class CameraVariablesEditor : CameraVariablesEditorBase
    {
    }
    
    [Serializable]
    public class HapticsVariablesEditor : HapticsVariablesEditorBase
    {
    }
}
