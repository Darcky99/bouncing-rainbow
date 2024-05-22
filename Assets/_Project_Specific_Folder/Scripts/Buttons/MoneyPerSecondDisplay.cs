using KobGamesSDKSlim.Collectable;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MoneyPerSecondDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_TextMoney;

    public void SetIncomePerSecond(float i_Value) {
        m_TextMoney.text = $"${i_Value.ToString("0.00")}/s";
    }
}
