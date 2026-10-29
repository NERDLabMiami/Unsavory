using UnityEngine;
using UnityEngine.UI;

// Drives the scene's "Taco HUD" (Container > Taco x N): one icon per trust taco, full while the player has it, lost otherwise.
// The art comes from the scene: the first icon's sprite is "full", the last icon's is "lost".
public class TrustTacoHud : MonoBehaviour
{
    public Sprite full;
    public Sprite lost;

    Image[] icons;

    /// <summary>The full taco art from the scene's HUD, for other UI that wants to show a trust taco inline.</summary>
    public static Sprite FullSprite { get; private set; }

    // Level calls this on arrival; nothing needs to be wired in the scene.
    public static void Attach()
    {
        foreach (var t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (t.name != "Taco HUD") continue;
            if (t.GetComponent<TrustTacoHud>() == null) t.gameObject.AddComponent<TrustTacoHud>();
            return;
        }
        Debug.LogWarning("TrustTacoHud: no 'Taco HUD' in this scene.");
    }

    void Awake()
    {
        var container = transform.Find("Container");
        icons = container != null ? container.GetComponentsInChildren<Image>(true) : new Image[0];
        if (icons.Length == 0) { Debug.LogWarning("TrustTacoHud: 'Taco HUD' has no icons under Container."); return; }
        if (full == null) full = icons[0].sprite;
        if (lost == null) lost = icons[icons.Length - 1].sprite;
        FullSprite = full;
    }

    void OnEnable()
    {
        PlayerSignals.TrustChanged += Refresh;
        Refresh(TacoLedger.Trust);
    }

    void OnDisable() { PlayerSignals.TrustChanged -= Refresh; }

    void Refresh(int trust)
    {
        if (icons == null) return;
        for (int i = 0; i < icons.Length; i++) icons[i].sprite = i < trust ? full : lost;
    }
}
