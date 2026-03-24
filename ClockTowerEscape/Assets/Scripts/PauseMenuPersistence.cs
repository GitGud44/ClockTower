using UnityEngine;

public class PauseMenuPersistence : MonoBehaviour
{
    public static PauseMenuPersistence Instance { get; private set; }
    public GameManager.PlayMode MenuMode { get; private set; } = GameManager.PlayMode.None;

    void Awake()
    {
        if (transform.root != transform)
        {
            PauseMenuPersistence rootPersistence = transform.root.GetComponent<PauseMenuPersistence>();
            if (rootPersistence == null)
                rootPersistence = transform.root.gameObject.AddComponent<PauseMenuPersistence>();

            Destroy(this);
            return;
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Initialize(GameManager.PlayMode mode)
    {
        MenuMode = mode;
    }
}
