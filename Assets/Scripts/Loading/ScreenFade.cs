using UnityEngine;
using UnityEngine.UI;

// A full-screen black cover for scene entry: the scene starts under it, a toast reads on top of it
// (HomeToast sorts above), and FadeIn reveals the scene.
public static class ScreenFade
{
	public const int CoverOrder = 32000;
	public const int AboveCoverOrder = CoverOrder + 1;

	static Image _cover;

	// Scene sprites sit on sorting layers above Default (Foreground), and a canvas on Default draws under them,
	// so the cover and the toast on top of it both go on the last layer.
	public static int TopSortingLayerId
	{
		get { var layers = SortingLayer.layers; return layers.Length > 0 ? layers[layers.Length - 1].id : 0; }
	}

	public static bool IsCovered { get { return _cover != null && _cover.transform.parent.gameObject.activeSelf; } }

	// Covers the screen instantly. Pass the canvas the toast lives in so the cover uses the same render mode:
	// a screen-space-camera canvas always draws under an overlay one, whatever the sorting orders.
	public static void Black(Canvas like = null)
	{
		if (_cover == null)
		{
			var canvasGO = new GameObject("ScreenFade", typeof(Canvas));
			var canvas = canvasGO.GetComponent<Canvas>();
			if (like != null && like.renderMode == RenderMode.ScreenSpaceCamera && like.worldCamera != null)
			{
				canvas.renderMode = RenderMode.ScreenSpaceCamera;
				canvas.worldCamera = like.worldCamera;
				canvas.planeDistance = like.planeDistance;
			}
			else canvas.renderMode = RenderMode.ScreenSpaceOverlay;
			canvas.sortingLayerID = TopSortingLayerId;
			canvas.sortingOrder = CoverOrder;

			var bgGO = new GameObject("Black", typeof(CanvasRenderer), typeof(Image));
			bgGO.transform.SetParent(canvasGO.transform, false);
			_cover = bgGO.GetComponent<Image>();
			_cover.raycastTarget = true; // no clicking the scene while it is hidden
			var rt = bgGO.GetComponent<RectTransform>();
			rt.anchorMin = Vector2.zero;
			rt.anchorMax = Vector2.one;
			rt.offsetMin = rt.offsetMax = Vector2.zero;
		}
		_cover.transform.parent.gameObject.SetActive(true);
		_cover.color = Color.black;
	}

	// Fades the cover away; yield on the result to wait. Returns null when nothing is covering the screen.
	public static Tween FadeIn(float duration)
	{
		if (!IsCovered) return null;
		var cover = _cover;
		return Tween.Alpha(cover, 0f, duration, null, 0f, () =>
		{
			if (cover != null && cover.color.a <= 0f) cover.transform.parent.gameObject.SetActive(false);
		});
	}
}
