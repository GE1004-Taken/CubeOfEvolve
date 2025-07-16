using UnityEngine;

namespace Assets.AT.CameraCtrl
{
    public class CameraRotationLinkage : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera; // 同期される側のカメラ
        [SerializeField] private Camera referenceCamera; // 基準となるカメラ

        private void Start()
        {
            if (targetCamera == null || referenceCamera == null) enabled = false;
        }

        private void LateUpdate()
        {
            // 基準カメラのZ軸の回転角度を取得
            Vector3 referenceRotation = referenceCamera.transform.eulerAngles;
            targetCamera.transform.rotation = Quaternion.Euler(referenceRotation);
        }
    }
}