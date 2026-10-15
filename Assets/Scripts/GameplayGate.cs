using System.Collections.Generic;
using UnityEngine;

public class GameplayGate : MonoBehaviour
{
    public static class GateLocks
    {
        public const string HomeModal = "HOME_MODAL";
        public const string BillsModal = "BILLS_MODAL";
        public const string PillsModal = "PILLS_MODAL";
        public const string BossFlow = "BOSS_FLOW";
    }
    public static GameplayGate Instance { get; private set; }

    private readonly HashSet<string> _locks = new HashSet<string>();
    public bool InteractionEnabled => _locks.Count == 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Lock(string reason)
    {
        _locks.Add(reason);
        Debug.Log($"[GameplayGate] LOCK: {reason} (count={_locks.Count})");
    }

    public void Unlock(string reason)
    {
        if (_locks.Remove(reason))
            Debug.Log($"[GameplayGate] UNLOCK: {reason} (count={_locks.Count})");
    }
}