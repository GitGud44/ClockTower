using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class AccessibilityMenu : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject accessibilityMenuPanel;
    public GameObject teleport;
    public GameObject move;
    public GameObject snapTurn;
    public GameObject continuousTurn;
    public XRRayInteractor teleportInteractor;

    public Toggle teleportToggle;
    public Toggle moveToggle;
    public Toggle snapTurnToggle;
    public Toggle continuousTurnToggle;
    public Slider speedSlider;
    public Slider sensitivitySlider;

    private DesktopPlayer desktopPlayer;

    void Start()
    {
        if (teleportToggle != null) teleportToggle.onValueChanged.AddListener(OnTeleportToggleChanged);
        if (moveToggle != null) moveToggle.onValueChanged.AddListener(OnMoveToggleChanged);
        if (snapTurnToggle != null) snapTurnToggle.onValueChanged.AddListener(OnSnapTurnToggleChanged);
        if (continuousTurnToggle != null) continuousTurnToggle.onValueChanged.AddListener(OnContinuousTurnToggleChanged);
        if (speedSlider != null)
        {
            speedSlider.onValueChanged.AddListener(OnSpeedChanged);
        }
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
        }

        RefreshSettings();
    }

    void OnEnable()
    {
        RefreshSettings();
    }

    public void BackToMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (accessibilityMenuPanel != null) accessibilityMenuPanel.SetActive(false);
    }

    private void ApplyLocomotionMode(bool useTeleport, bool save)
    {
        if (teleportToggle != null) teleportToggle.SetIsOnWithoutNotify(useTeleport);
        if (moveToggle != null) moveToggle.SetIsOnWithoutNotify(!useTeleport);

        if (teleport != null) teleport.SetActive(useTeleport);
        if (move != null) move.SetActive(!useTeleport);

        if (teleportInteractor != null)
            teleportInteractor.enabled = useTeleport;

        if (save)
        {
            SettingsState.SetLocomotionMode(useTeleport);
        }
    }

    private void ApplyTurnMode(bool useContinuousTurn, bool save)
    {
        if (continuousTurnToggle != null) continuousTurnToggle.SetIsOnWithoutNotify(useContinuousTurn);
        if (snapTurnToggle != null) snapTurnToggle.SetIsOnWithoutNotify(!useContinuousTurn);

        bool canToggleDistinctTurnObjects = continuousTurn != null &&
                                            snapTurn != null &&
                                            continuousTurn != snapTurn;

        if (canToggleDistinctTurnObjects)
        {
            continuousTurn.SetActive(useContinuousTurn);
            snapTurn.SetActive(!useContinuousTurn);
        }

        if (save)
        {
            SettingsState.SetTurnMode(useContinuousTurn);
        }
    }

    void OnTeleportToggleChanged(bool isOn)
    {
        if (isOn)
            ApplyLocomotionMode(true, true);
    }

    void OnMoveToggleChanged(bool isOn)
    {
        if (isOn)
            ApplyLocomotionMode(false, true);
    }

    void OnSnapTurnToggleChanged(bool isOn)
    {
        if (isOn)
            ApplyTurnMode(false, true);
    }

    void OnContinuousTurnToggleChanged(bool isOn)
    {
        if (isOn)
            ApplyTurnMode(true, true);
    }

    void OnSpeedChanged(float speed)
    {
        SettingsState.SetPlayerSpeed(speed, desktopPlayer);
    }

    void OnSensitivityChanged(float sensitivity)
    {
        SettingsState.SetMouseSensitivity(sensitivity, desktopPlayer);
    }

    private void RefreshSettings()
    {
        RebindRuntimeTargets();

        ApplyLocomotionMode(SettingsState.GetUseTeleport(), false);
        ApplyTurnMode(SettingsState.GetUseContinuousTurn(), false);

        if (speedSlider != null)
            speedSlider.SetValueWithoutNotify(SettingsState.GetPlayerSpeed());

        if (sensitivitySlider != null)
            sensitivitySlider.SetValueWithoutNotify(SettingsState.GetMouseSensitivity());

        SettingsState.ApplyRuntimeSettings();
    }

    private void RebindRuntimeTargets()
    {
        desktopPlayer = FindFirstObjectByType<DesktopPlayer>();

        if (teleportInteractor == null)
            teleportInteractor = FindFirstObjectByType<XRRayInteractor>();

        if (GameManager.Instance == null || GameManager.Instance.CurrentPlayMode != GameManager.PlayMode.VR)
            return;

        if (teleport == null)
            teleport = FindSceneObject("Teleport");

        if (move == null)
            move = FindSceneObject("Move");

        if (continuousTurn == null)
            continuousTurn = FindSceneObject("Turn");
    }

    private GameObject FindSceneObject(string objectName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] roots = activeScene.GetRootGameObjects();

        for (int index = 0; index < roots.Length; index++)
        {
            Transform match = FindChildRecursive(roots[index].transform, objectName);
            if (match != null)
                return match.gameObject;
        }

        return null;
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent.name == childName)
            return parent;

        for (int index = 0; index < parent.childCount; index++)
        {
            Transform match = FindChildRecursive(parent.GetChild(index), childName);
            if (match != null)
                return match;
        }

        return null;
    }
}
