using UnityEngine;
using UnityEngine.UI;

namespace GreatestOfTheTrader.Safety
{
    /// <summary>
    /// Simple UI binder that toggles HUD layers based on current safety state.
    /// Assign UI references in the scene to match the spec (idle HUD, caution banner, danger curtain).
    /// </summary>
    public class SafetyStateUIBinder : MonoBehaviour
    {
        [SerializeField] GameObject idleLayer;
        [SerializeField] GameObject cautionBanner;
        [SerializeField] GameObject dangerCurtain;
        [SerializeField] GameObject overrideLock;
        [SerializeField] Text statusLabel;

        [Header("Audio")]
        [SerializeField] AudioSource cautionAudio;
        [SerializeField] AudioSource dangerAudio;

        SafetyStateController _controller;

        void Awake()
        {
            _controller = GetComponent<SafetyStateController>();
            if (_controller == null)
            {
                Debug.LogError("SafetyStateUIBinder requires SafetyStateController on the same GameObject.");
                enabled = false;
                return;
            }
            _controller.onStateChanged.AddListener(HandleStateChanged);
        }

        void OnDestroy()
        {
            if (_controller != null)
            {
                _controller.onStateChanged.RemoveListener(HandleStateChanged);
            }
        }

        void HandleStateChanged(SafetyStateController.SafetyState state)
        {
            if (idleLayer != null)
            {
                idleLayer.SetActive(state == SafetyStateController.SafetyState.Idle);
            }

            if (cautionBanner != null)
            {
                cautionBanner.SetActive(state == SafetyStateController.SafetyState.Caution);
            }

            if (dangerCurtain != null)
            {
                dangerCurtain.SetActive(state == SafetyStateController.SafetyState.Danger);
            }

            if (overrideLock != null)
            {
                overrideLock.SetActive(state == SafetyStateController.SafetyState.Override);
            }

            UpdateStatusLabel(state);
            HandleAudio(state);
        }

        void UpdateStatusLabel(SafetyStateController.SafetyState state)
        {
            if (statusLabel == null)
            {
                return;
            }

            switch (state)
            {
                case SafetyStateController.SafetyState.Idle:
                    statusLabel.text = "안전 상태";
                    break;
                case SafetyStateController.SafetyState.Caution:
                    statusLabel.text = "주의! 주변을 확인하세요.";
                    break;
                case SafetyStateController.SafetyState.Danger:
                    statusLabel.text = "위험! 기기를 내려 주변을 확인하세요.";
                    break;
                case SafetyStateController.SafetyState.Override:
                    statusLabel.text = "잠금 해제 전 안전을 확인하세요.";
                    break;
            }
        }

        void HandleAudio(SafetyStateController.SafetyState state)
        {
            if (cautionAudio != null)
            {
                if (state == SafetyStateController.SafetyState.Caution && !cautionAudio.isPlaying)
                {
                    cautionAudio.Play();
                }
                else if (state != SafetyStateController.SafetyState.Caution && cautionAudio.isPlaying)
                {
                    cautionAudio.Stop();
                }
            }

            if (dangerAudio != null)
            {
                if (state == SafetyStateController.SafetyState.Danger || state == SafetyStateController.SafetyState.Override)
                {
                    if (!dangerAudio.isPlaying)
                    {
                        dangerAudio.loop = state == SafetyStateController.SafetyState.Danger;
                        dangerAudio.Play();
                    }
                }
                else if (dangerAudio.isPlaying)
                {
                    dangerAudio.Stop();
                }
            }
        }
    }
}
