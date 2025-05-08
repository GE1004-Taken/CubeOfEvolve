// 作成日：   250508
// 更新日：   250508
// 作成者： 安中 健人

// 概要説明(AIにより作成)：
// このスクリプトは、汎用的な確認ダイアログのUIを制御します。
// 表示するメッセージ、Yes/Noボタンのテキスト、およびYesボタンが押された際の処理は、
// 外部の ScriptableObject (CheckDialogConfig) を通じて設定されます。
// これにより、ゲーム内の様々な確認シーンで共通のUIプレハブを再利用でき、
// 拡張性および保守性が向上します。

// 使い方説明：
// スタート関数でクラス:CheckCanvasCtrl のアクセスを取得し、メソッド:ShowCheckCanvas を、任意のボタンにAddListenerしてね。
// その任意のボタンを引数2に渡すと、閉じるときにカーソルが戻ります。

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CheckCanvasCtrl : MonoBehaviour
{
    // ---------------------------- SerializeField

    [SerializeField] private TextMeshProUGUI _checkText; // 確認メッセージ表示用テキスト
    [SerializeField] private Button _YesButton;       // 「はい」ボタン
    [SerializeField] private Button _NoButton;        // 「いいえ」ボタン

    // ---------------------------- Field

    private Canvas _checkCanvas;             // このGameObjectにアタッチされたCanvasコンポーネント
    private TextMeshProUGUI _YesButtonText; // 「はい」ボタンの子にあるテキストコンポーネント
    private TextMeshProUGUI _NoButtonText;  // 「いいえ」ボタンの子にあるテキストコンポーネント

    private Button _lastSelectedButton;// キャンセル時にフォーカスを戻す先のボタンを一時的に保持
    private ICheckAction _currentAction;  // Yes/Noボタンが押された際に実行する処理を定義したインターフェースのインスタンス

    // ---------------------------- UnityMessage

    private void Start()
    {
        _checkCanvas = GetComponent<Canvas>();
        _checkCanvas.enabled = false; // 初期状態では非表示

        // 各ボタンの子からTextMeshProUGUIコンポーネントを取得
        _YesButtonText = _YesButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        _NoButtonText = _NoButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    // ---------------------------- Public

    /// <summary>
    /// 確認ダイアログを表示します。
    /// </summary>
    /// <param name="config">Assets>01_Games>Scripts>uGUI>SO にあるものを使ってね。</param>
    /// <param name="focusOnCancel">キャンセル時にフォーカスを戻すボタン (任意)</param>
    public void ShowCheckCanvas(CheckDialogConfig config, Button focusOnCancel = null)
    {
        _checkText.text = config.message;             // 確認メッセージを設定
        _YesButtonText.text = config.yesButtonText;   // 「はい」ボタンのテキストを設定
        _NoButtonText.text = config.noButtonText;     // 「いいえ」ボタンのテキストを設定
        _lastSelectedButton = focusOnCancel;        // キャンセル時にフォーカスを戻すボタンを保存

        // CheckDialogConfigに設定された ICheckAction を取得
        _currentAction = config.actionReference?.GetAction();

        // 「はい」ボタンのクリックリスナーを登録（既存のリスナーをクリアしてから追加）
        _YesButton.onClick.RemoveAllListeners();
        _YesButton.onClick.AddListener(() =>
        {
            _currentAction?.OnYes(); // 設定されたYesアクションを実行
            CloseCheckCanvas();      // ダイアログを閉じる
        });

        // 「いいえ」ボタンのクリックリスナーを登録（既存のリスナーをクリアしてから追加）
        _NoButton.onClick.RemoveAllListeners();
        _NoButton.onClick.AddListener(() =>
        {
            _currentAction?.OnNo();  // 設定されたNoアクションを実行
            CloseCheckCanvas();       // ダイアログを閉じる
        });

        _NoButton.Select();           // 初期選択を「いいえ」ボタンにする
        _checkCanvas.enabled = true;  // Canvasを有効にして表示
    }

    // ---------------------------- Private

    /// <summary>
    /// 確認ダイアログを閉じます。
    /// </summary>
    private void CloseCheckCanvas()
    {
        _checkCanvas.enabled = false;           // Canvasを無効にして非表示
        _YesButton.onClick.RemoveAllListeners(); // 「はい」ボタンのリスナーをクリア
        _NoButton.onClick.RemoveAllListeners();  // 「いいえ」ボタンのリスナーをクリア
        if (_lastSelectedButton != null)
        {
            _lastSelectedButton.Select();       // キャンセル前に選択されていたボタンにフォーカスを戻す
            _lastSelectedButton = null;        // 一時変数をクリア
        }
        _currentAction = null;                // 現在のアクションをクリア
    }

}