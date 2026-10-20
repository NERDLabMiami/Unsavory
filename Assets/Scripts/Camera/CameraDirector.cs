using System;
using System.Collections.Generic;
using UnityEngine;

// Named camera shots (position + zoom). Boss dialogue uses a shot that frames part of the kitchen with no food trays,
// plate, order queue or nose in view; gameplay uses the wide shot. Moves run on unscaled time.
//
// Shots can be tuned on the Main Camera in the Inspector: place the camera in the Scene view and use the component's
// "Capture current view as shot" menu item. Built-in defaults are added for any of the standard shots that are missing.
[RequireComponent(typeof(Camera))]
public class CameraDirector : MonoBehaviour {
	public const string Gameplay = "Gameplay";
	public const string Dialogue = "Dialogue";
	public const string Kitchen = "Kitchen";
	const float KitchenOrtho = 4.955149f;
	static readonly Vector2 KitchenCenter = new Vector2(9.57f, 0f);

	[Serializable]
	public class Shot {
		public string name;
		public Vector2 position;
		public float orthoSize = 4.955149f;
	}

	public List<Shot> shots = new List<Shot>();
	public float defaultDuration = 1f;

	// The prep table sits lower in the frame for the gameplay shot than the Kitchen shot's framing leaves it, so during
	// dialogue it is shifted by this much (world units) and eases back for gameplay. Tune it in the Inspector.
	public Vector2 dialogueTableOffset = new Vector2(0f, -1.53f);

	// Boss dialogue is framed by this shot (the whole kitchen, zoomed out) whatever the Dialogue entry in the list says;
	// clear the name to use the Dialogue entry itself.
	public string dialogueShotName = Kitchen;

	static CameraDirector _instance;
	Camera _cam;

	public string CurrentShot { get; private set; }

	public static CameraDirector Instance {
		get {
			if (_instance == null && Camera.main != null) _instance = AnimUtil.Get<CameraDirector>(Camera.main.gameObject);
			return _instance;
		}
	}

	// Null-safe entry points for game code.
	public static Tween Go(string shot, float? duration = null) {
		var d = Instance;
		return d != null ? d.GoTo(shot, duration) : null;
	}

	public static void Cut(string shot) {
		var d = Instance;
		if (d != null) d.CutTo(shot);
	}

	/// <summary>The fade that revealed the gameplay area on the last shot change (null if none); yield on it to wait.</summary>
	public Tween LastRevealTween { get; private set; }

	/// <summary>Jumps to a shot with no camera move and returns the reveal fade, if the shot shows the gameplay area.</summary>
	public static Tween CutAndReveal(string shot) {
		var d = Instance;
		if (d == null) return null;
		d.CutTo(shot);
		return d.LastRevealTween;
	}

	void Awake() {
		_instance = this;
		_cam = GetComponent<Camera>();
		EnsureDefaults();
		// The old Game Camera Animator would keep resetting the position to the scene's saved pose, undoing every shot.
		AnimUtil.DisableAnimator(gameObject);
		// The level opens with the boss dialogue, so start on that framing whatever pose the scene camera was saved in.
		CutTo(Dialogue);
	}

	void Reset() { _cam = GetComponent<Camera>(); EnsureDefaults(); }

	void OnDestroy() { if (_instance == this) _instance = null; }

	// Gameplay = how the scene camera is framed now. Kitchen = the whole artboard, used for boss dialogue.
	void EnsureDefaults() {
		if (Find(Gameplay) == null) {
			var cam = _cam != null ? _cam : GetComponent<Camera>();
			shots.Add(new Shot { name = Gameplay, position = new Vector2(transform.position.x, transform.position.y), orthoSize = cam != null ? cam.orthographicSize : 4.955149f });
		}
		if (Find(Kitchen) == null)
			shots.Add(new Shot { name = Kitchen, position = KitchenCenter, orthoSize = KitchenOrtho });
	}

	public Shot Find(string name) {
		if (name == Dialogue && !string.IsNullOrEmpty(dialogueShotName)) {
			var alias = FindExact(dialogueShotName);
			if (alias != null) return alias;
		}
		return FindExact(name);
	}

	Shot FindExact(string name) {
		for (int i = 0; i < shots.Count; i++)
			if (shots[i].name == name) return shots[i];
		return null;
	}

	public Tween GoTo(string shot, float? duration = null) {
		var s = Find(shot);
		if (s == null) { Debug.LogWarning("[CameraDirector] No shot named '" + shot + "'"); return null; }
		if (_cam == null) _cam = GetComponent<Camera>();

		CurrentShot = shot;
		float d = duration ?? defaultDuration;
		ShowForShot(shot, d);
		Tween.MoveLocal(transform, new Vector3(s.position.x, s.position.y, transform.position.z), d, Tween.Ease.InOutCubic);
		return Tween.OrthoSize(_cam, s.orthoSize, d, Tween.Ease.InOutCubic);
	}

	// Dialogue plays over the empty kitchen; every other shot is gameplay.
	void ShowForShot(string shot, float tableDuration) {
		bool dialogue = shot == Dialogue || (!string.IsNullOrEmpty(dialogueShotName) && shot == dialogueShotName);
		KitchenStage.SetTableOffset(dialogue ? dialogueTableOffset : Vector2.zero, tableDuration);
		LastRevealTween = KitchenStage.SetGameplayVisible(!dialogue);
	}

	public void CutTo(string shot) {
		var s = Find(shot);
		if (s == null) { Debug.LogWarning("[CameraDirector] No shot named '" + shot + "'"); return; }
		if (_cam == null) _cam = GetComponent<Camera>();

		CurrentShot = shot;
		ShowForShot(shot, 0f);
		AnimUtil.DisableAnimator(gameObject);
		Tween.Cancel(transform, "pos");
		Tween.Cancel(_cam, "ortho");
		transform.position = new Vector3(s.position.x, s.position.y, transform.position.z);
		_cam.orthographicSize = s.orthoSize;
	}

#if UNITY_EDITOR
	[ContextMenu("Capture current view as shot")]
	void CaptureCurrentView() {
		var cam = GetComponent<Camera>();
		var s = new Shot { name = "Shot " + (shots.Count + 1), position = transform.position, orthoSize = cam.orthographicSize };
		UnityEditor.Undo.RecordObject(this, "Capture camera shot");
		shots.Add(s);
		UnityEditor.EditorUtility.SetDirty(this);
	}

	void OnDrawGizmosSelected() {
		var cam = GetComponent<Camera>();
		float aspect = cam != null ? cam.aspect : 16f / 9f;
		for (int i = 0; i < shots.Count; i++) {
			var s = shots[i];
			Gizmos.color = s.name == Gameplay ? Color.green : s.name == Dialogue ? Color.yellow : Color.cyan;
			Gizmos.DrawWireCube(new Vector3(s.position.x, s.position.y, 0f), new Vector3(2f * s.orthoSize * aspect, 2f * s.orthoSize, 0f));
			UnityEditor.Handles.Label(new Vector3(s.position.x - s.orthoSize * aspect, s.position.y + s.orthoSize, 0f), s.name);
		}
	}
#endif
}
