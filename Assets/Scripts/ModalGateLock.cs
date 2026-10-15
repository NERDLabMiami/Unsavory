using UnityEngine;

public class ModalGateLock : MonoBehaviour
{
    public string lockReason = GameplayGate.GateLocks.HomeModal;

    void OnEnable()
    {
        if (GameplayGate.Instance != null)
            GameplayGate.Instance.Lock(lockReason);
    }

    void OnDisable()
    {
        if (GameplayGate.Instance != null)
            GameplayGate.Instance.Unlock(lockReason);
    }
}