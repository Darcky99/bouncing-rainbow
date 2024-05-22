using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KobGamesSDKSlim
{
    public class InteractionManager : MonoBehaviour
    {
        public RateUsPopUp RateUsPopup;

        [ValueDropdown(nameof(getSchemas)), OnValueChanged(nameof(onSelectedSchema))]
        public RateUsPopupSchema SelectedSchema;

        [Button]
        public void ShowRateUsPopup()
        {
            RateUsPopup.ShowRateUs();
        }

        [Button]
        public void CloseRateUsPopup()
        {
            RateUsPopup.CloseRateUs();
        }

        private RateUsPopupSchema[] getSchemas()
        {            
            return RateUsPopup.Schemas;
        }

        private void onSelectedSchema()
        {
            RateUsPopup.SelectedSchema = SelectedSchema;
            RateUsPopup.OnSelectedSchema();
        }
    }
}