using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim
{
    [CreateAssetMenu(fileName = "Shortcuts Group - ", menuName = "KobGames/Shortcuts Group"), InlineEditor]
    public class ShortcutsGroup : ScriptableObject
    {
        public string GroupName;
        public ShortcutsKeysCombination[] Shortcuts;
    }
}
