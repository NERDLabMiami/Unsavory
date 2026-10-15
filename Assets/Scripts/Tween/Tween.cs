using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Minimal code-driven tween system. Always runs on unscaled time so it keeps
// working while the tutorial holds Time.timeScale = 0.
//
// A tween is keyed by (target object, channel). Starting a new tween on the same
// key cancels the old one without calling its onComplete, which replaces the
// "re-fired Animator trigger restarts the clip" behaviour.
public sealed class Tween : CustomYieldInstruction {
	public enum Ease { Linear, OutQuad, InQuad, InOutCubic, OutBack, OutCubic }

	public static Ease DefaultEase = Ease.OutQuad;

	// ---- Instance state ----
	public bool IsDone { get; private set; }
	public bool WasCancelled { get; private set; }
	public override bool keepWaiting { get { return !IsDone; } }

	object _target;
	string _channel;
	float _duration;
	float _delay;
	float _elapsed;
	int _loops = 1; // 1 = once, <= 0 = infinite
	bool _pingPong;
	Ease _ease;
	Action<float> _apply;
	Action _onComplete;

	// ---- Registry ----
	struct Key : IEquatable<Key> {
		public object target; public string channel;
		public bool Equals(Key o) { return ReferenceEquals(target, o.target) && channel == o.channel; }
		public override bool Equals(object o) { return o is Key && Equals((Key)o); }
		public override int GetHashCode() { return (target != null ? target.GetHashCode() : 0) * 31 + (channel != null ? channel.GetHashCode() : 0); }
	}

	static readonly Dictionary<Key, Tween> _byKey = new Dictionary<Key, Tween>();
	static readonly List<Tween> _active = new List<Tween>();
	static TweenRunner _runner;

	// ---- Core ----

	// Generic 0..1 tween. `apply` receives the eased value. `target` may be null for untargeted tweens.
	public static Tween Run(UnityEngine.Object target, string channel, float duration, Action<float> apply,
		Ease? ease = null, float delay = 0f, Action onComplete = null) {
		EnsureRunner();
		var t = new Tween {
			_target = target, _channel = channel, _duration = Mathf.Max(0f, duration), _delay = Mathf.Max(0f, delay),
			_ease = ease ?? DefaultEase, _apply = apply, _onComplete = onComplete
		};
		if (target != null) {
			var k = new Key { target = target, channel = channel };
			Tween old;
			if (_byKey.TryGetValue(k, out old)) old.Cancel();
			_byKey[k] = t;
		}
		_active.Add(t);
		return t;
	}

	public Tween Loop(int count = 0, bool pingPong = false) { _loops = count; _pingPong = pingPong; return this; }

	public Tween OnComplete(Action a) { _onComplete = a; return this; }

	public void Cancel() {
		if (IsDone) return;
		WasCancelled = true;
		Finish(false);
	}

	// Jump to the end value and complete (used to snap on skip).
	public void Complete() {
		if (IsDone) return;
		Apply(_pingPong && _loops != 1 ? 0f : 1f);
		Finish(true);
	}

	public static void Cancel(UnityEngine.Object target, string channel) {
		Tween t;
		if (_byKey.TryGetValue(new Key { target = target, channel = channel }, out t)) t.Cancel();
	}

	public static void CancelAll(UnityEngine.Object target) {
		for (int i = _active.Count - 1; i >= 0; i--)
			if (ReferenceEquals(_active[i]._target, target)) _active[i].Cancel();
	}

	// Waits for real (unscaled) seconds; the replacement for hand-tuned WaitForSecondsRealtime guesses.
	public static Tween Delay(float seconds, Action onComplete = null) {
		return Run(null, null, 0f, null, Ease.Linear, seconds, onComplete);
	}

	// ---- Typed helpers ----

	public static Tween Move(Transform t, Vector3 to, float duration, Ease? ease = null, float delay = 0f, Action done = null) {
		Vector3 from = default(Vector3); bool init = false;
		return Run(t, "pos", duration, v => { if (t == null) return; if (!init) { from = t.position; init = true; } t.position = Vector3.LerpUnclamped(from, to, v); }, ease, delay, done);
	}

	public static Tween MoveLocal(Transform t, Vector3 to, float duration, Ease? ease = null, float delay = 0f, Action done = null) {
		Vector3 from = default(Vector3); bool init = false;
		return Run(t, "pos", duration, v => { if (t == null) return; if (!init) { from = t.localPosition; init = true; } t.localPosition = Vector3.LerpUnclamped(from, to, v); }, ease, delay, done);
	}

	public static Tween Anchored(RectTransform t, Vector2 to, float duration, Ease? ease = null, float delay = 0f, Action done = null) {
		Vector2 from = default(Vector2); bool init = false;
		return Run(t, "pos", duration, v => { if (t == null) return; if (!init) { from = t.anchoredPosition; init = true; } t.anchoredPosition = Vector2.LerpUnclamped(from, to, v); }, ease, delay, done);
	}

	public static Tween Scale(Transform t, Vector3 to, float duration, Ease? ease = null, float delay = 0f, Action done = null) {
		Vector3 from = default(Vector3); bool init = false;
		return Run(t, "scale", duration, v => { if (t == null) return; if (!init) { from = t.localScale; init = true; } t.localScale = Vector3.LerpUnclamped(from, to, v); }, ease, delay, done);
	}

	public static Tween Rotate(Transform t, Vector3 toEuler, float duration, Ease? ease = null, float delay = 0f, Action done = null) {
		Quaternion from = Quaternion.identity; bool init = false;
		Quaternion to = Quaternion.Euler(toEuler);
		return Run(t, "rot", duration, v => { if (t == null) return; if (!init) { from = t.localRotation; init = true; } t.localRotation = Quaternion.SlerpUnclamped(from, to, v); }, ease, delay, done);
	}

	public static Tween Alpha(CanvasGroup g, float to, float duration, Ease? ease = null, float delay = 0f, Action done = null) {
		float from = 0f; bool init = false;
		return Run(g, "alpha", duration, v => { if (g == null) return; if (!init) { from = g.alpha; init = true; } g.alpha = Mathf.LerpUnclamped(from, to, v); }, ease, delay, done);
	}

	// Works for Image, Text and TMP (all Graphics).
	public static Tween Alpha(Graphic g, float to, float duration, Ease? ease = null, float delay = 0f, Action done = null) {
		float from = 0f; bool init = false;
		return Run(g, "alpha", duration, v => {
			if (g == null) return; if (!init) { from = g.color.a; init = true; }
			var c = g.color; c.a = Mathf.LerpUnclamped(from, to, v); g.color = c;
		}, ease, delay, done);
	}

	public static Tween Alpha(SpriteRenderer s, float to, float duration, Ease? ease = null, float delay = 0f, Action done = null) {
		float from = 0f; bool init = false;
		return Run(s, "alpha", duration, v => {
			if (s == null) return; if (!init) { from = s.color.a; init = true; }
			var c = s.color; c.a = Mathf.LerpUnclamped(from, to, v); s.color = c;
		}, ease, delay, done);
	}

	public static Tween Tint(Graphic g, Color to, float duration, Ease? ease = null, float delay = 0f, Action done = null) {
		Color from = default(Color); bool init = false;
		return Run(g, "color", duration, v => { if (g == null) return; if (!init) { from = g.color; init = true; } g.color = Color.LerpUnclamped(from, to, v); }, ease, delay, done);
	}

	public static Tween Tint(SpriteRenderer s, Color to, float duration, Ease? ease = null, float delay = 0f, Action done = null) {
		Color from = default(Color); bool init = false;
		return Run(s, "color", duration, v => { if (s == null) return; if (!init) { from = s.color; init = true; } s.color = Color.LerpUnclamped(from, to, v); }, ease, delay, done);
	}

	public static Tween Volume(AudioSource a, float to, float duration, Ease? ease = null, float delay = 0f, Action done = null) {
		float from = 0f; bool init = false;
		return Run(a, "volume", duration, v => { if (a == null) return; if (!init) { from = a.volume; init = true; } a.volume = Mathf.LerpUnclamped(from, to, v); }, ease, delay, done);
	}

	public static Tween Intensity(Light l, float to, float duration, Ease? ease = null, float delay = 0f, Action done = null) {
		float from = 0f; bool init = false;
		return Run(l, "intensity", duration, v => { if (l == null) return; if (!init) { from = l.intensity; init = true; } l.intensity = Mathf.LerpUnclamped(from, to, v); }, ease, delay, done);
	}

	// ---- Easing ----

	public static float Evaluate(Ease e, float x) {
		switch (e) {
			case Ease.InQuad: return x * x;
			case Ease.OutQuad: return 1f - (1f - x) * (1f - x);
			case Ease.OutCubic: { float y = 1f - x; return 1f - y * y * y; }
			case Ease.InOutCubic: return x < 0.5f ? 4f * x * x * x : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
			case Ease.OutBack: { const float c1 = 1.70158f, c3 = c1 + 1f; float y = x - 1f; return 1f + c3 * y * y * y + c1 * y * y; }
			default: return x;
		}
	}

	// ---- Internals ----

	void Apply(float normalized) {
		if (_apply != null) _apply(Evaluate(_ease, normalized));
	}

	// Returns true when finished.
	bool Step(float dt) {
		if (IsDone) return true;
		if (_target is UnityEngine.Object && (UnityEngine.Object)_target == null) { WasCancelled = true; Finish(false); return true; }

		if (_delay > 0f) {
			_delay -= dt;
			if (_delay > 0f) return false;
			dt = -_delay; _delay = 0f;
		}

		_elapsed += dt;
		if (_duration <= 0f) { Apply(1f); Finish(true); return true; }

		int cycle = (int)(_elapsed / _duration);
		if (_loops > 0 && cycle >= _loops) {
			Apply(_pingPong && _loops % 2 == 0 ? 0f : 1f);
			Finish(true);
			return true;
		}

		float n = (_elapsed % _duration) / _duration;
		if (_pingPong && cycle % 2 == 1) n = 1f - n;
		Apply(n);
		return false;
	}

	void Finish(bool invokeComplete) {
		IsDone = true;
		if (_target != null) {
			var k = new Key { target = _target, channel = _channel };
			Tween cur;
			if (_byKey.TryGetValue(k, out cur) && cur == this) _byKey.Remove(k);
		}
		if (invokeComplete && _onComplete != null) {
			var cb = _onComplete; _onComplete = null;
			cb();
		}
	}

	static void EnsureRunner() {
		if (_runner != null) return;
		var go = new GameObject("[Tween]");
		go.hideFlags = HideFlags.HideAndDontSave;
		UnityEngine.Object.DontDestroyOnLoad(go);
		_runner = go.AddComponent<TweenRunner>();
	}

	// Scene unload leaves destroyed targets behind; Step() culls them. Also reset on domain reload with fast play mode.
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void ResetStatics() {
		_byKey.Clear(); _active.Clear(); _runner = null;
	}

	sealed class TweenRunner : MonoBehaviour {
		void Update() {
			float dt = Time.unscaledDeltaTime;
			// Iterate by index: callbacks may start new tweens (appended) or cancel others.
			for (int i = 0; i < _active.Count; i++) {
				var t = _active[i];
				if (!t.IsDone) t.Step(dt);
			}
			_active.RemoveAll(t => t.IsDone);
		}
	}
}
