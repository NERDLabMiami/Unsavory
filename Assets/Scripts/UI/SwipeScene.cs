using UnityEngine;

public class SwipeScene : MonoBehaviour
{
    public Transform[] places;
    public AudioClip[] backgroundMusic;

    [Header("Movement")]
    public float lerpTime = 1f;

    private Transform startPosition;
    private Transform endPosition;
    private float currentLerpTime = 0f;
    private bool moving = false;

    public event System.Action<int> OnPlaceChanged;

    private int _currentPlace;
    public int currentPlace
    {
        get => _currentPlace;
        set
        {
            if (_currentPlace == value) return;
            _currentPlace = value;
            OnPlaceChanged?.Invoke(_currentPlace);
        }
    }

    void Start()
    {
        if (places == null || places.Length == 0) return;

        // Start in the middle (Kitchen)
        currentPlace = Mathf.Clamp(places.Length / 2, 0, places.Length - 1);

        if (Camera.main)
            Camera.main.transform.position = places[currentPlace].position;

        TrySwapMusic();
    }

    void Update()
    {
        UpdateMovement();
        NavigateRooms();
    }

    private void UpdateMovement()
    {
        if (!moving) return;

        currentLerpTime += Time.deltaTime;
        if (currentLerpTime >= lerpTime)
        {
            currentLerpTime = lerpTime;
            moving = false;
            TrySwapMusic();
        }

        float perc = (lerpTime <= 0f) ? 1f : (currentLerpTime / lerpTime);

        if (Camera.main)
            Camera.main.transform.position = Vector3.Lerp(startPosition.position, endPosition.position, perc);
    }

    private void NavigateRooms()
    {
        var input = GameInput.Instance;
        if (moving || input == null) return;

        // D-pad / shoulders / arrows / A-D; a modal's navigation must not pan the room behind it.
        if (ModalFocus.HasFocus ||
            (GameplayGate.Instance != null && !GameplayGate.Instance.InteractionEnabled))
            return;

        if (input.PreviousRoomPressed)
            TryMove(-1);   // Left -> Bedroom
        else if (input.NextRoomPressed)
            TryMove(+1);   // Right -> Living Room / Door
    }

    private void TryMove(int delta)
    {
        if (places == null || places.Length == 0) return;

        int next = Mathf.Clamp(currentPlace + delta, 0, places.Length - 1);
        if (next == currentPlace) return;

        startPosition = places[currentPlace];
        currentPlace = next; // fires OnPlaceChanged
        endPosition = places[currentPlace];

        currentLerpTime = 0f;
        moving = true;
    }

    private void TrySwapMusic()
    {
        var src = GetComponent<AudioSource>();
        if (!src) return;

        if (backgroundMusic != null &&
            currentPlace >= 0 &&
            currentPlace < backgroundMusic.Length &&
            src.clip != backgroundMusic[currentPlace])
        {
            src.clip = backgroundMusic[currentPlace];
            src.Play();
        }
    }
}
