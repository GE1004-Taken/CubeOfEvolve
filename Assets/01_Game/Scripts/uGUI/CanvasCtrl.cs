// 作成日： 250508
// 更新日： 250508 機能完成
//              250820 機能拡張 閉じるボタンを複数設定可能に。
// 作成者： 安中 健人

// 概要説明(AIにより作成)：

// 使い方説明：

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AT.uGUI
{
    public class CanvasCtrl : MonoBehaviour
    {
        // ---------------------------- SerializeField
        [Header("ボタン")]
        [SerializeField] private List<Button> _openBtnList = new List<Button>();
        [SerializeField] private List<Button> _closeBtnList = new List<Button>();
        [Header("イベント")]
        [SerializeField] private UnityEvent _openEvent = new();
        [SerializeField] private UnityEvent _closeEvent = new();
        // ---------------------------- Field
        private Button _saveBtn; // 一時変数
        private Canvas _canvas;
        // ---------------------------- button
        public void OnOpenCanvas(Button clickedButton = null) // 引数として押されたボタンを受け取る
        {
            if (clickedButton == null) Debug.LogWarning("nullなのでする―");
            _canvas.enabled = true;
            if (_closeBtnList.Count != 0) _closeBtnList[0].Select();
            if (clickedButton != null) _saveBtn = clickedButton;
            _openEvent?.Invoke();
        }
        public void OnCloseCanvas()
        {
            _canvas.enabled = false;
            if (_saveBtn != null) _saveBtn.Select();
            _closeEvent?.Invoke();
        }

        // ---------------------------- UnityMessage

        private void Start()
        {
            _canvas = GetComponent<Canvas>();

            if (_openBtnList == null)
                return;
            else
                foreach (var button in _openBtnList)
                {
                    button.onClick.AddListener(() => OnOpenCanvas(button));
                }

            if (_closeBtnList == null)
                return;
            else
                foreach (var button in _closeBtnList)
                {
                    button.onClick.AddListener(() => OnCloseCanvas());
                }
        }

        // ---------------------------- PublicMethod

        // ---------------------------- PrivateMethod

    }
}
