using DG.Tweening;
using R3;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UI;

namespace Assets.AT
{
    public class CameraDollyMover : MonoBehaviour
    {
        // -----Field
        [Header("Reference")]
        [SerializeField] private CinemachineSplineDolly _cinemachineSplineDolly;
        [SerializeField] private SplineContainer[] _splineContainer;

        [Header("Value")]
        [SerializeField] private float _intervalForChangeSpline = 10f;

        [Header("UI")]
        [SerializeField] private Image _panel;

        private int currentSplineContainerIndex;
        private float _fadeTime;
        private float _intervalTime;

        Coroutine _coroutine;
        private const int FIRST_SPLINE_NUM = 0;

        // -----UnityMessage
        private void Start()
        {
            // null 確認
            if (_cinemachineSplineDolly == null || _splineContainer == null)
            {
                Debug.LogError("CameraDollyMover:CinemachineSplineDolly か SplineContainer 参照されてないよ");
                enabled = false;
                return;
            }

            // 初期化
            currentSplineContainerIndex = FIRST_SPLINE_NUM;
            _cinemachineSplineDolly.Spline = _splineContainer[currentSplineContainerIndex];
            _panel.enabled = true;
            _fadeTime = _intervalForChangeSpline * 0.1f;
            _intervalTime = _intervalForChangeSpline - _fadeTime * 2;
            _coroutine = StartCoroutine(AutoChangeSpline());

            // タイトル終わったらストップ
            GameManager.Instance.CurrentGameState
                .Where(x => x == IGC2025.Scripts.GameManagers.GameState.READY)
                .Take(1)
                .Subscribe(x => { StopCoroutine(_coroutine); _panel.DOComplete(); })
                .AddTo(this);
        }

        private void OnDestroy()
        {
            StopCoroutine(_coroutine);
            _panel.DOComplete();
        }

        // -----Private
        /// <summary>
        /// 時間経過でカメラの移動ルートを切り替える
        /// </summary>
        /// <returns></returns>
        private IEnumerator AutoChangeSpline()
        {
            while (true)
            {
                _panel.DOFade(0, _fadeTime);
                yield return new WaitForSeconds(_intervalTime);
                _panel.DOFade(1, _fadeTime);
                yield return new WaitForSeconds(_fadeTime);
                _cinemachineSplineDolly.CameraPosition = 0;
                SwitchToNextSpline();
                yield return new WaitForSeconds(_fadeTime);
            }
        }

        /// <summary>
        /// 入れ替える中身
        /// </summary>
        private void SwitchToNextSpline()
        {
            currentSplineContainerIndex = (currentSplineContainerIndex + 1) % _splineContainer.Length;
            _cinemachineSplineDolly.Spline = _splineContainer[currentSplineContainerIndex];
            //Debug.Log($"SplineContainer を {_cinemachineSplineDolly.Spline.name} に切り替えました。");
        }

    }
}