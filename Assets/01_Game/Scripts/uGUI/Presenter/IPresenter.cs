namespace Assets.IGC2025.Scripts.Presenter
{
    public interface IPresenter
    {
        /// <summary>初期化済みか</summary>
        bool IsInitialized { get; }

        /// <summary>Presenterの初期化。モデルが立ち上がった後に呼ばれる想定。</summary>
        void Initialize();
    }
}