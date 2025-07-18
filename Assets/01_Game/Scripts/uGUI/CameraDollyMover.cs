using Assets.IGC2025.Scripts.GameManagers;
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

        private Coroutine _coroutine;
        private const int FIRST_SPLINE_NUM = 0;

        // -----UnityMessage
        private void Start()
        {
            // null 確認
            if (_cinemachineSplineDolly == null || _splineContainer == null)
            {
                Debug.LogError("CameraDollyMover:CinemachineSplineDolly か SplineContainer が設定されていません");
                enabled = false;
                return;
            }

            // 初期化
            currentSplineContainerIndex = FIRST_SPLINE_NUM;
            _cinemachineSplineDolly.Spline = _splineContainer[currentSplineContainerIndex];
            _panel.enabled = true;

            _fadeTime = _intervalForChangeSpline * 0.1f;
            _intervalTime = _intervalForChangeSpline - _fadeTime * 2;

            // GameStateがTITLEのときのみ Coroutine を開始、それ以外で停止
            GameManager.Instance.CurrentGameState
                .Subscribe(state =>
                {
                    if (state == GameState.TITLE)
                    {
                        // 既に動いていれば止める
                        if (_coroutine != null)
                        {
                            StopCoroutine(_coroutine);
                            _coroutine = null;
                        }

                        // Coroutine を開始
                        _coroutine = StartCoroutine(AutoChangeSpline());
                    }
                    else
                    {
                        // Coroutine を停止、安全処理
                        if (_coroutine != null)
                        {
                            StopCoroutine(_coroutine);
                            _coroutine = null;
                        }

                        // DOTween演出も止める
                        _panel?.DOComplete();
                        _panel?.DOKill();
                    }
                })
                .AddTo(this);
        }

        private void OnDestroy()
        {
            // Coroutine 停止と演出停止（破棄時）
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }
            _panel?.DOComplete();
            _panel?.DOKill();
        }

        // -----Private
        /// <summary>
        /// 時間経過でカメラの移動ルートを切り替える（Coroutine）
        /// </summary>
        private IEnumerator AutoChangeSpline()
        {
            while (true)
            {
                // フェードアウト
                _panel?.DOFade(0, _fadeTime);
                yield return new WaitForSeconds(_intervalTime);

                // フェードイン
                _panel?.DOFade(1, _fadeTime);
                yield return new WaitForSeconds(_fadeTime);

                // カメラ位置をリセットしてスプライン切り替え
                if (_cinemachineSplineDolly != null)
                {
                    _cinemachineSplineDolly.CameraPosition = 0;
                    SwitchToNextSpline();
                }

                yield return new WaitForSeconds(_fadeTime);
            }
        }

        /// <summary>
        /// スプラインを順番に切り替える
        /// </summary>
        private void SwitchToNextSpline()
        {
            currentSplineContainerIndex = (currentSplineContainerIndex + 1) % _splineContainer.Length;
            _cinemachineSplineDolly.Spline = _splineContainer[currentSplineContainerIndex];
        }
    }
}
