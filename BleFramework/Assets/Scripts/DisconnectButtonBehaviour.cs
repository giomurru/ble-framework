using UnityEngine;

public class DisconnectButtonBehaviour : MonoBehaviour
{
    public delegate void DisconnectButtonEventDelegate();
    public static event DisconnectButtonEventDelegate DisconnectButtonEvent;
    // Use this for initialization
    public void ExecuteDisconnectAction()
    {
        DisconnectButtonEvent?.Invoke();
    }
}
