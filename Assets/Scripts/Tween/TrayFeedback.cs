using UnityEngine;

// When an ingredient is taken from its tray, the tray snaps to its "-less" art (a smaller pile), then the full
// pile fades back in as the less art fades out. The less sprite lives on a child overlay.
public static class TrayFeedback {
	const float Hold = 0.25f;
	const float Fade = 0.2f;
	const string OverlayName = "LessOverlay";

	// Remembers the tray's authored alpha, which is not always 1 and can't be read back mid-dip.
	class DipState : MonoBehaviour { public float restAlpha; }

	public static void Dip(SpriteRenderer tray, Sprite less, float scale = 1f) {
		if (tray == null || less == null) return;
		AnimUtil.DisableAnimator(tray.gameObject); // the old Tray controller writes m_Color every frame and would undo the fade
		var overlay = GetOverlay(tray, out float rest);
		overlay.sprite = less;
		overlay.transform.localScale = new Vector3(scale, scale, 1f);
		overlay.color = WithAlpha(tray.color, rest);
		tray.color = WithAlpha(tray.color, 0f);

		Tween.Alpha(tray, rest, Fade, Tween.Ease.Linear, Hold);
		Tween.Alpha(overlay, 0f, Fade, Tween.Ease.Linear, Hold);
	}

	static Color WithAlpha(Color c, float a) { return new Color(c.r, c.g, c.b, a); }

	static SpriteRenderer GetOverlay(SpriteRenderer tray, out float restAlpha) {
		var child = tray.transform.Find(OverlayName);
		SpriteRenderer sr;
		DipState state;
		if (child == null) {
			var go = new GameObject(OverlayName);
			go.transform.SetParent(tray.transform, false);
			sr = go.AddComponent<SpriteRenderer>();
			state = go.AddComponent<DipState>();
			state.restAlpha = tray.color.a;
		} else {
			sr = child.GetComponent<SpriteRenderer>();
			state = child.GetComponent<DipState>();
		}
		sr.sharedMaterial = tray.sharedMaterial;
		sr.sortingLayerID = tray.sortingLayerID;
		sr.sortingOrder = tray.sortingOrder + 1;
		restAlpha = state.restAlpha;
		return sr;
	}
}
