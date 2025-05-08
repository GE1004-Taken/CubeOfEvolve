// 作成日：   250508
// 更新日：   250508
// 作成者： 安中 健人

// 概要説明(AIにより作成)：

// 使い方説明：

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace uGUI.Ctrl
{
    public class CanvasCtrl : MonoBehaviour
    {
        // ---------------------------- SerializeField
        [Header("ボタン")]
        [SerializeField] private List<Button> _openBtnList = new List<Button> ();
        [SerializeField] private Button _closeBtn;
        // ---------------------------- Field
        private Button _saveBtn; // 一時変数
        private Canvas _canvas;
        // ---------------------------- button
        private void OnOpenCanvas(Button clickedButton) // 引数として押されたボタンを受け取る
        {
            Debug.Log("ボタン処理：in - " + clickedButton.name); // どのボタンが押されたか確認
            _canvas.enabled = true;
            _closeBtn.Select();
            _saveBtn = clickedButton;
        }
        private void OnCloseCanvas()
        {
            Debug.Log("ボタン処理：out");
            _canvas.enabled = false;
            if (_saveBtn != null) // _saveBtn がnullでないか確認
            {
                _saveBtn.Select();
            }
        }

        // ---------------------------- UnityMessage

        private void Start()
        {
            _canvas = GetComponent<Canvas>();
            foreach (var button in _openBtnList)
            {
                // ラムダ式を使って、ボタンがクリックされたときに OnOpenCanvas メソッドに自身を渡す
                button.onClick.AddListener(() => OnOpenCanvas(button));
            }
            _closeBtn.onClick.AddListener(OnCloseCanvas);
        }

        // ---------------------------- PublicMethod

        // ---------------------------- PrivateMethod

    }
}
