using Assets.IGC2025.Scripts.Presenter;
using UnityEngine;

namespace App.GameSystem.Handler
{
    public class SceneClassReferenceHandler : MonoBehaviour
    {
        // -----Public
        public PresenterDropCanvas PresenterDropCanvas { get; private set; }

        // -----UnityMessage
        private void OnEnable()
        {
            PresenterDropCanvas = FindAnyObjectByType<PresenterDropCanvas>();
        }
    }
}