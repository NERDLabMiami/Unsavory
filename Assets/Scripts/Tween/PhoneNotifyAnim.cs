using UnityEngine;

// Table phone notification: rocks in short buzz bursts while there is unread mail or a bill due today,
// until the player opens the phone. Added to the Cellphone by HomeInteractable; Home reloads each day, so Start re-checks.
public class PhoneNotifyAnim : MonoBehaviour {
	const float Period = 1.2f, BuzzFraction = 0.55f, Cycles = 6f, Degrees = 5f;

	bool _cleared;

	void Start() {
		if (!_cleared && (MailArchive.UnreadCount() > 0 || BillsState.DueCount() > 0)) Buzz();
	}

	// Loops until Stop().
	void Buzz() {
		Tween.Run(this, "buzz", Period, v => {
			float k = Mathf.Clamp01(v / BuzzFraction);
			float rock = v < BuzzFraction ? Mathf.Sin(k * Mathf.PI * 2f * Cycles) * Degrees : 0f;
			transform.localRotation = Quaternion.Euler(0f, 0f, rock);
		}, Tween.Ease.Linear).Loop(0);
	}

	// The phone was opened: stay still for the rest of this visit.
	public void Stop() {
		_cleared = true;
		Tween.Cancel(this, "buzz");
		transform.localRotation = Quaternion.identity;
	}

	void OnDestroy() { Tween.Cancel(this, "buzz"); }
}
