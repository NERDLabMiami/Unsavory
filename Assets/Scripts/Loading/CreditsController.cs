using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// The end of a run: the credits (Resources/credits.txt) scroll up, then the game returns to the main menu with a fresh run.
// Any click or key press skips ahead after a short grace period. The Credits scene is built by Tools > Create Credits Scene.
public class CreditsController : MonoBehaviour
{
	public TMP_Text text;
	public RectTransform scroller;     // moved up the screen; holds the text
	public CanvasGroup fade;           // whole-scene fade
	public float scrollSpeed = 90f;    // canvas units per second
	public float fadeSeconds = 1f;
	public float skipGraceSeconds = 1.5f;

	bool _leaving;

	void Start()
	{
		var file = Resources.Load<TextAsset>("credits");
		if (text != null && file != null) text.text = file.text;
		if (fade != null) fade.alpha = 0f;
		StartCoroutine(Run());
	}

	IEnumerator Run()
	{
		if (fade != null) yield return Tween.Alpha(fade, 1f, fadeSeconds);

		float startedAt = Time.unscaledTime;
		float endY = EndY();
		while (scroller != null && scroller.anchoredPosition.y < endY && !SkipRequested(startedAt))
		{
			scroller.anchoredPosition += Vector2.up * (scrollSpeed * Time.unscaledDeltaTime);
			yield return null;
		}
		yield return Leave();
	}

	// The scroller is anchored at the bottom edge, just off screen; it is done when its top has passed the top of the screen.
	float EndY()
	{
		if (scroller == null || scroller.parent == null) return 0f;
		float canvasHeight = ((RectTransform)scroller.parent).rect.height;
		return canvasHeight + scroller.rect.height;
	}

	bool SkipRequested(float startedAt)
	{
		if (Time.unscaledTime - startedAt < skipGraceSeconds) return false;
		var keyboard = Keyboard.current;
		var mouse = Mouse.current;
		var touch = Touchscreen.current;
		var pad = Gamepad.current;
		return (keyboard != null && keyboard.anyKey.wasPressedThisFrame)
			|| (mouse != null && mouse.leftButton.wasPressedThisFrame)
			|| (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
			|| (pad != null && pad.buttonSouth.wasPressedThisFrame);
	}

	IEnumerator Leave()
	{
		if (_leaving) yield break;
		_leaving = true;
		if (fade != null) yield return Tween.Alpha(fade, 0f, fadeSeconds);
		CareerFlowManager.Instance.FinishRun();
	}
}
