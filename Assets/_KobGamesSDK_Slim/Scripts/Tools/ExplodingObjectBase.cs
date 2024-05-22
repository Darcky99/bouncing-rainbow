using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

namespace KobGamesSDKSlim
{
    public class ExplodingObjectBase : MonoBehaviour
    {
        [Header("Models")]
        [ReadOnly] public GameObject Model;
        [ReadOnly] public GameObject ExplodedModel;
        [SerializeField, ReadOnly] protected List<Rigidbody> ExplodedModelPieces = new List<Rigidbody>();
        [SerializeField, ReadOnly] protected List<Vector3> ExplodedModelPiecesPos = new List<Vector3>();

        public Vector2 ExplosionForceRange;
        public Vector2 ExplosionUpModifierRange;
        public float HorizontalForce;
        public float Torque;
        public float ExplosionRadius;

        public AudioClip ExplosionSound;


        private Rigidbody m_DummyPiece;

        public virtual void Start()
        {
            // Ruben: should be called from derived
            //GameManager.Instance.OnReset += OnCoreLoop_Reset;
        }

        [Button]
        public virtual void SetRefs()
        {
            Model = transform.FindDeepChild<GameObject>("Model");
            ExplodedModel = transform.FindDeepChild<GameObject>("Exploded Model");

            ExplodedModelPieces.Clear();
            ExplodedModelPiecesPos.Clear();
            for (int i = 0; i < ExplodedModel.transform.childCount; i++)
            {
                m_DummyPiece = ExplodedModel.transform.GetChild(i).gameObject.GetComponent<Rigidbody>();
                if (m_DummyPiece == null)
                    m_DummyPiece = ExplodedModel.transform.GetChild(i).gameObject.AddComponent<Rigidbody>();
                ExplodedModelPieces.Add(m_DummyPiece);
                ExplodedModelPieces[i].isKinematic = true;
                ExplodedModelPiecesPos.Add(ExplodedModelPieces[i].transform.localPosition);
            }
        }

        protected virtual void Awake()
        {
            if (Model == null || ExplodedModel == null)
                return;

            Model.SetActive(true);
            ExplodedModel.SetActive(false);

            if (ExplodedModelPieces.Count == 0)
            {
                ExplodedModelPieces.Clear();
                ExplodedModelPiecesPos.Clear();
                for (int i = 0; i < ExplodedModel.transform.childCount; i++)
                {
                    m_DummyPiece = ExplodedModel.transform.GetChild(i).gameObject.GetComponent<Rigidbody>();
                    if (m_DummyPiece == null)
                        m_DummyPiece = ExplodedModel.transform.GetChild(i).gameObject.AddComponent<Rigidbody>();
                    ExplodedModelPieces.Add(ExplodedModel.transform.GetChild(i).gameObject.AddComponent<Rigidbody>());
                    ExplodedModelPieces[i].isKinematic = true;
                    ExplodedModelPiecesPos.Add(ExplodedModelPieces[i].transform.localPosition);
                }

                Debug.LogError(gameObject.name + "You should prebake the explosion lists");
            }
        }
        protected virtual void OnEnable()
        {

        }
        protected virtual void OnDisable()
        {
            ResetObject();
        }

        [Button]
        public virtual void Explode(Vector3 ExplosionCenter)
        {
            if (ExplodedModel != null)
            {
                Model.SetActive(false);
                ExplodedModel.SetActive(true);

#if SoundManagerSDK
                SoundManager.Instance.PlaySFX(ExplosionSound);
#endif

                for (int i = 0; i < ExplodedModelPieces.Count; i++)
                {
                    m_DummyPiece = ExplodedModelPieces[i];

                    m_DummyPiece.transform.localPosition = ExplodedModelPiecesPos[i];
                    m_DummyPiece.transform.localRotation = Quaternion.identity;
                    m_DummyPiece.velocity = Vector3.zero;
                    m_DummyPiece.angularVelocity = Vector3.zero;

                    m_DummyPiece.isKinematic = false;

                    m_DummyPiece.AddExplosionForce(Random.Range(ExplosionForceRange.x, ExplosionForceRange.y), ExplosionCenter, ExplosionRadius, Random.Range(ExplosionUpModifierRange.x, ExplosionUpModifierRange.y), ForceMode.Impulse);

                    m_DummyPiece.AddForce(Vector3.right * Random.Range(-HorizontalForce, HorizontalForce), ForceMode.Impulse);
                    m_DummyPiece.AddTorque(Vector3.right * Random.Range(-Torque, Torque) + Vector3.up * Random.Range(-Torque, Torque) + Vector3.forward * Random.Range(-Torque, Torque), ForceMode.Impulse);
                }
            }
        }

        public virtual void ResetObject()
        {
            //Debug.Log("Reset " + gameObject.name);

            Model.SetActive(true);
            ExplodedModel.SetActive(false);

            //this is probably not necessary.Let's use it only when exploding
            //for (int i = 0; i < ExplodedModelPieces.Count; i++)
            //{
            //    ExplodedModelPieces[i].transform.localPosition = ExplodedModelPiecesPos[i];
            //    ExplodedModelPieces[i].transform.localRotation = Quaternion.identity;
            //    ExplodedModelPieces[i].isKinematic = true;
            //}
        }

#region ICoreLoopEvents call
        public virtual void OnCoreLoop_Reset()
        {
            ResetObject();
        }
#endregion
    }
}
