using UnityEngine;

public class OrderSpawnIn : MonoBehaviour
{
    public float lockoutSeconds = 0.25f;
    float _t;

    void Awake() { _t = lockoutSeconds; }

    public bool IsLockedOut
    {
        get
        {
            if (_t <= 0f) return false;
            _t -= Time.deltaTime;
            return true;
        }
    }
}