using Assets.AT;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildCameraManualUpdater : MonoBehaviour
{
    [SerializeField]
    private CinemachineCamera m_camera = null;

    public void OnLook(InputAction.CallbackContext context)
    {
        if (CameraCtrlManager.Instance.GetCamera(CameraCtrlManager.Instance.GetCurrentActiveCameraKey()) == m_camera)
        {
            if (context.started == true)
            {
                CameraCtrlManager.Instance.SetBrainUpdateMode(CinemachineBrain.UpdateMethods.ManualUpdate);
            }

            if (context.performed == true)
            {
                CameraCtrlManager.Instance.BrainManualUpdate();
            }

            if (context.canceled == true)
            {
                CameraCtrlManager.Instance.SetBrainUpdateMode(CinemachineBrain.UpdateMethods.SmartUpdate);
            }
        }
    }
}
