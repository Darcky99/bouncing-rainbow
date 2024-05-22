
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;
using DG.Tweening;
using Sirenix.OdinInspector;

namespace KobGamesSDKSlim
{
    public class CrossPromoVid : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Button m_PlayBtn;
        [SerializeField] private Button m_CloseBtn;
        [SerializeField] private RectTransform m_Parent;
        [SerializeField] private TextMeshProUGUI m_Promotext;
        [SerializeField] private VideoPlayer m_VideoPlayer;
        [SerializeField] private string[] m_PromoTexts;
        [SerializeField] private AdConfiguration[] m_AdConfigs;
        [ReadOnly, ShowInInspector] private int m_GameOverCounter;
        [ReadOnly, ShowInInspector] private int m_GameWonCounter;
#pragma warning restore 0649

        private AdConfiguration m_CurrentAd;
        private GameManager m_GameManager;
        private bool m_StartCounters;

        public int ShowGameOverCountdown = 3;
        public int ShowGameWonCountdown = 3;
        public bool ShowOnFirstLaunch;


        private void Awake()
        {
            m_GameManager = FindObjectOfType<GameManager>();
            m_CloseBtn.onClick.AddListener(closeWindow);

            if (m_GameManager)
            {
                GameManager.OnLevelStarted += onLevelStarted;
                GameManager.OnLevelLoaded += onLevelLoaded;
                GameManager.OnLevelFailedNoContinue += onLevelFailed;
                GameManager.OnLevelCompleted += onLevelCompleted;
            }
            else
            {
                Debug.LogError($"Not Found [{nameof(AnalyticsManager)}] in the scene. Please add before adding this prefab.");
            }
        }

        private void Start()
        {
            if (!ShowOnFirstLaunch)
            {
                if (Managers.Instance.Storage.GameLaunchCount == 1)
                    gameObject.SetActive(false);
                else if (Managers.Instance.Storage.GameLaunchCount > 1)
                    gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        private void onLevelCompleted()
        {
            m_GameWonCounter += 1;
        }

        private void onLevelFailed()
        {
            m_GameOverCounter += 1;
        }

        private void onLevelLoaded()
        {
            if (m_StartCounters || Managers.Instance.Storage.GameLaunchCount == 1)
            {
                if (m_GameOverCounter >= ShowGameOverCountdown)
                {
                    show();
                }

                else if (m_GameWonCounter >= ShowGameWonCountdown)
                {
                    show();
                }
            }
            else
            {
                show();
            }
        }

        private void show()
        {
            m_GameWonCounter = 0;
            m_GameOverCounter = 0;

            m_StartCounters = false;
            gameObject.SetActive(true);
            if (DOTween.IsTweening(m_Parent.GetInstanceID())) return;
            m_Parent.DOAnchorPosX(-m_Parent.anchoredPosition.x, .25f).From().SetUpdate(true).SetId(m_Parent.GetInstanceID());
        }

        private void onLevelStarted()
        {
            gameObject.SetActive(false);
        }

        private void closeWindow()
        {
            m_GameWonCounter = 0;
            m_GameOverCounter = 0;

            m_StartCounters = true;
            gameObject.SetActive(false);

        }

        private void OnEnable()
        {
            m_CurrentAd = m_AdConfigs[Random.Range(0, m_AdConfigs.Length)];

            m_Promotext.text = m_PromoTexts[Random.Range(0, m_PromoTexts.Length)];
            m_VideoPlayer.clip = m_CurrentAd.VideoClip;

            m_PlayBtn.onClick.RemoveAllListeners();
            m_PlayBtn.onClick.AddListener(onPlay);

            m_VideoPlayer.GetComponent<Button>().onClick.RemoveAllListeners();
            m_VideoPlayer.GetComponent<Button>().onClick.AddListener(onPlay);

        }

        private void onPlay()
        {
            Utils.OpenCrossPromoUrlStore(m_CurrentAd.AndroidAppID, m_CurrentAd.IOSAppID);
        }
    }
}