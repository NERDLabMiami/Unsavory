using UnityEngine;

// Feel settings for the pop-up bubbles (boss speech, the player's reply, tutorial hints).
// Edit Assets/Resources/PopTuning.asset; changes apply the next time a bubble pops, even in Play mode.
[CreateAssetMenu(fileName = "PopTuning", menuName = "Unsavory/Pop Tuning")]
public class PopTuning : ScriptableObject {
	[Header("Jiggle after a pop-in")]
	[Tooltip("Peak wobble angle in degrees. 0 = no wobble at all.")]
	[Range(0f, 15f)] public float jiggleDegrees = 2f;
	[Tooltip("How many back-and-forth swings the wobble makes.")]
	[Range(0f, 8f)] public float jiggleCycles = 3f;
	[Tooltip("How long the wobble lasts, in seconds. It fades out over this time.")]
	[Range(0.1f, 2f)] public float jiggleSeconds = 0.6f;

	static PopTuning _current;

	/// <summary>The tuning asset in Resources, or built-in defaults if it has been deleted.</summary>
	public static PopTuning Current {
		get {
			if (_current == null) _current = Resources.Load<PopTuning>("PopTuning");
			if (_current == null) _current = CreateInstance<PopTuning>();
			return _current;
		}
	}
}
