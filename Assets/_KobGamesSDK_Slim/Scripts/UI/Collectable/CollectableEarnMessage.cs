using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KobGamesSDKSlim.Collectable
{
    public class CollectableEarnMessage : MonoBehaviour
    {
        [Title("Refs")]
        [SerializeField, ReadOnly] private CanvasGroup m_CanvasGroup;
        [SerializeField, ReadOnly] private Image m_Panel;
        [SerializeField, ReadOnly] private TextMeshProUGUI m_Text;
        [SerializeField, ReadOnly] private Image m_Icon;
        [SerializeField, ReadOnly] private ParticleSystem m_EarnParticles;
        [SerializeField, HideInInspector] private ParticleSystemRenderer m_EarnParticlesRenderer;

        [Title("Parameters")]
        [SerializeField] private bool m_IsUseTextFormat = true;
        [SerializeField] private bool m_IsHideBigNumbers = true;

        [Title("Colors")]
        [SerializeField] private Material m_BlackText;
        [SerializeField] private Material m_WhiteText;

        private AnimationData m_CollectableData => GameConfig.Instance.HUD.AnimationData;

        private AnimationData.EarnMessageAnimDataDictionary m_AnimDataDictionary => m_CollectableData.EarnMessageAnim;

        private AnimationData.EarnMessageAnimData m_AnimData => m_AnimDataDictionary.ContainsKey(m_AnimType)
            ? m_AnimDataDictionary[m_AnimType]
            : m_AnimDataDictionary[eCollectableEarnMessageAnimType.Default];

        private CollectableData m_CollectableUniqueData => GameConfig.Instance.HUD.CollectableData[CollectableType];

        protected virtual eCollectableType CollectableType => m_CollectableType;

        private float m_EarnAmount;
        private Vector3 m_SpawnPosition;
        private eCollectableEarnMessageAnimType m_AnimType;
        private eCollectableType m_CollectableType;

        private string m_TextFormat;

        private Sequence m_AnimSequence;

        #region Editor

        private void OnValidate()
        {
            SetRefs();
        }

        [Button]
        private void SetRefs()
        {
            m_CanvasGroup = GetComponentInChildren<CanvasGroup>(true);
            m_Panel = transform.FindDeepChild<Image>("Background");
            m_Text = transform.FindDeepChild<TextMeshProUGUI>("Text");
            m_Icon = transform.FindDeepChild<Image>("Icon");
            m_EarnParticles = transform.FindDeepChild<ParticleSystem>("Earn Particles");

            m_EarnParticlesRenderer = m_EarnParticles.GetComponent<ParticleSystemRenderer>();
        }
        #endregion

        public void ShowMessage(eCollectableType i_Type, int i_Amount, eCollectableEarnMessageAnimType i_AnimType, Vector3 i_Position, bool i_isWhite)
        {
            m_AnimType = i_AnimType;

            m_CanvasGroup.transform.localScale = m_AnimData.SizeMultiplier * Vector3.one;
            transform.localScale = Vector3.zero;

            m_EarnAmount = i_Amount;
            m_CollectableType = i_Type;
            m_SpawnPosition = i_Position;

            transform.position = i_Position;
            
            m_Icon.sprite = m_CollectableUniqueData.CollectableSprite;

            switch (m_AnimData.AnimMode)
            {
                case AnimationData.EarnMessageAnimData.eAnimMode.SimpleText:
                    showSimpleText(m_AnimData.SimpleTextAnimMode);
                    break;
                default:
                    break;
            }

            if (m_IsUseTextFormat)
            {
                if (m_TextFormat == null)
                {
                    m_TextFormat = m_Text.text;
                }

                m_Text.SetText(string.Format(m_TextFormat,
                    m_IsHideBigNumbers ? (object)hideBigNumber(i_Amount) : i_Amount));
            }
            else
            {
                m_Text.SetText($"+{i_Amount}");
            }

            switch (i_isWhite)
            {
                case true :
                    m_Text.fontMaterial = m_WhiteText;
                    break;
                case false:
                    m_Text.fontMaterial = m_BlackText;
                    break;
            }
        }

        private string hideBigNumber(int i_Num)
        {
            if (i_Num >= 100000000)
            {
                return (i_Num / 1000000D).ToString("0.#M");
            }
            if (i_Num >= 1000000)
            {
                return (i_Num / 1000000D).ToString("0.##M");
            }
            if (i_Num >= 100000)
            {
                return (i_Num / 1000D).ToString("0.#k");
            }
            if (i_Num >= 10000)
            {
                return (i_Num / 1000D).ToString("0.##k");
            }

            return i_Num.ToString();
        }

        private void showSimpleText(AnimationData.EarnMessageAnimData.SimpleTextAnimData i_AnimData)
        {
            if(i_AnimData.IsShowPanel)
            {
                m_Panel.enabled = true;

                m_Panel.color = i_AnimData.PanelColor;
            }
            else
            {
                m_Panel.enabled = false;

                m_Text.enabled = i_AnimData.IsShowText;
                m_Icon.enabled = i_AnimData.IsShowText;
            }

            m_AnimSequence?.Kill();

            m_AnimSequence = DOTween.Sequence();

            m_AnimSequence
                .OnStart(() => //Start
                {
                    m_CanvasGroup.alpha = i_AnimData.StartFade;
                    transform.localScale = Vector3.one * i_AnimData.StartScale;
                    transform.position += Vector3.up * i_AnimData.StartOffset;
                })
                .AppendCallback(() =>
                {
                    if(m_AnimData.IsSpawnParticles)
                    {
                        DOVirtual.DelayedCall(m_AnimData.Particles.SpawnDelay, spawnParticles);
                    }
                })
                .AppendCallback(()=> //Appear
                {
                    transform.DOScale(i_AnimData.AppearScale, i_AnimData.AppearDuration).SetEase(i_AnimData.AppearEase);
                    m_CanvasGroup.DOFade(i_AnimData.AppearFade, i_AnimData.AppearDuration).SetEase(i_AnimData.AppearEase);
                })
                .Append(transform.DOMove(Vector3.up * i_AnimData.AppearOffset, i_AnimData.AppearDuration)).SetEase(i_AnimData.AppearEase).SetRelative()
                .AppendCallback(() => //Move
                {
                    transform.DOScale(i_AnimData.MoveScale, i_AnimData.MoveDuration).SetEase(i_AnimData.MoveEase);
                    m_CanvasGroup.DOFade(i_AnimData.MoveFade, i_AnimData.MoveDuration).SetEase(i_AnimData.MoveEase);
                })
                .Append(transform.DOMove(Vector3.up * i_AnimData.MoveOffset, i_AnimData.MoveDuration)).SetEase(i_AnimData.MoveEase).SetRelative()
                .AppendCallback(() => //Disappear
                {
                    transform.DOScale(i_AnimData.DisappearScale, i_AnimData.DisappearDuration).SetEase(i_AnimData.DisappearEase);
                    m_CanvasGroup.DOFade(i_AnimData.DisappearFade, i_AnimData.DisappearDuration).SetEase(i_AnimData.DisappearEase);
                })
                .Append(transform.DOMove(Vector3.up * i_AnimData.DisappearOffset, i_AnimData.DisappearDuration)).SetEase(i_AnimData.DisappearEase).SetRelative()
                ;
                

            m_AnimSequence
                .OnComplete(onShowMessageComplete);
        }

        private void spawnParticles()
        {
            m_EarnParticlesRenderer.material = m_CollectableUniqueData.ParticlesMaterial;

            var particleSize = m_CollectableUniqueData.ParticlesSize * m_AnimData.Particles.ParticlesSize;

            var main = m_EarnParticles.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(m_AnimData.Particles.ParticlesLifetime.x, m_AnimData.Particles.ParticlesLifetime.y);

            var randomScale = Random.Range(m_AnimData.Particles.ParticlesSize.x, m_AnimData.Particles.ParticlesSize.y);
            main.startSizeX = new ParticleSystem.MinMaxCurve(m_CollectableUniqueData.ParticlesSize.x * randomScale);
            main.startSizeY= new ParticleSystem.MinMaxCurve(m_CollectableUniqueData.ParticlesSize.y * randomScale);
            main.startSizeZ = new ParticleSystem.MinMaxCurve(m_CollectableUniqueData.ParticlesSize.z * randomScale);

            main.startSpeed = new ParticleSystem.MinMaxCurve(m_AnimData.Particles.Speed.x, m_AnimData.Particles.Speed.y);

            var shape = m_EarnParticles.shape;
            shape.position = m_AnimData.Particles.SpawnParticlesOffset;
            shape.angle = m_AnimData.Particles.SpawnConeAngle;
            shape.radius = m_AnimData.Particles.SpawnConeRadius;

            var burst = m_EarnParticles.emission.GetBurst(0);
            burst.count = new ParticleSystem.MinMaxCurve((int)(Mathf.Clamp(m_EarnAmount / m_AnimData.Particles.ItemCost, 1, m_AnimData.Particles.MaxAmount)));
            m_EarnParticles.emission.SetBurst(0, burst);

            m_EarnParticles.transform.up = Vector3.up;
            m_EarnParticles.Play();
        }

        protected virtual void onShowMessageComplete()
        {
            PoolManager.Instance.Queue(ePoolType.CollectableEarnMessage, gameObject);
        }
    }

}
