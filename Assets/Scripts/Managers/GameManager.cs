using System.Collections;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Respawn")]
    [SerializeField] private Vector3 spawnPoint = Vector3.zero;
    [SerializeField] private float respawnDelay = 2f;

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject menuCamera;
    [SerializeField] private GameObject gameplayCamera;

    [Header("Cutscene")]
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [SerializeField] private Transform brainTarget;
    [SerializeField] private CanvasGroup fadePanel;
    [SerializeField] private RawImage videoDisplay;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip cutsceneVideo;
    [SerializeField] private GameObject skipButton;

    [Header("Cutscene Settings")]
    [SerializeField] private float menuFadeDuration = 1f;
    [SerializeField] private float zoomDuration = 2f;
    [SerializeField] private float zoomDistance = 0.1f;
    [SerializeField] private AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Player")]
    [SerializeField] private PlayerReferences _playerReferences;

    [Header("UI")]
    [SerializeField] private GameObject healthBar;

    [Header("Audio")]
    [SerializeField] private AudioSource menuMusicSource;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string mixerVolumeParameter = "MasterVolume";

    [Header("Player Animation")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private string standUpTrigger = "StartGame";
    [SerializeField] private float standUpAnimationDuration = 2f;

    private bool _gameStarted = false;
    private bool _isTransitioning = false;
    private Vector3 _menuCameraStartPos;
    private Quaternion _menuCameraStartRot;
    private CinemachineCamera _menuCinemachine;
    private CinemachineCamera _gameplayCinemachine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 120;
        InitializeCutsceneSystem();
        ShowMainMenu();
    }

    private void InitializeCutsceneSystem()
    {
        // Get Cinemachine cameras
        if (menuCamera != null)
        {
            _menuCinemachine = menuCamera.GetComponent<CinemachineCamera>();

            // Store the actual camera transform position
            Camera cam = menuCamera.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                _menuCameraStartPos = cam.transform.position;
                _menuCameraStartRot = cam.transform.rotation;
            }
            else
            {
                _menuCameraStartPos = menuCamera.transform.position;
                _menuCameraStartRot = menuCamera.transform.rotation;
            }
        }

        if (gameplayCamera != null)
        {
            _gameplayCinemachine = gameplayCamera.GetComponent<CinemachineCamera>();
        }

        // Setup video player
        if (videoPlayer != null && cutsceneVideo != null)
        {
            videoPlayer.clip = cutsceneVideo;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            videoPlayer.playOnAwake = false;

            // Create render texture if not assigned
            if (videoPlayer.targetTexture == null)
            {
                videoPlayer.targetTexture = new RenderTexture(1920, 1080, 24);
            }

            if (videoDisplay != null)
            {
                videoDisplay.texture = videoPlayer.targetTexture;
                videoDisplay.gameObject.SetActive(false);
            }

            videoPlayer.loopPointReached += OnVideoFinished;
            videoPlayer.Stop();
        }
        else
        {
            if (videoPlayer == null) Debug.LogWarning("Video player not assigned!");
            if (cutsceneVideo == null) Debug.LogWarning("Cutscene video not assigned!");
        }

        // Setup fade panel
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
        }

        // Setup skip button
        if (skipButton != null)
        {
            skipButton.SetActive(false);
        }

        // Setup menu canvas group
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1f;
        }
    }

    private void OnEnable()
    {
        PlayerHealth.OnPlayerDeath += HandleDeath;
    }

    private void OnDisable()
    {
        PlayerHealth.OnPlayerDeath -= HandleDeath;
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    private void HandleDeath()
    {
        Invoke(nameof(RespawnPlayer), respawnDelay);
    }

    private void RespawnPlayer()
    {
        _playerReferences.Health.Respawn(spawnPoint);
    }

    private void ShowMainMenu()
    {
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
        }

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1f;
        }

        if (_menuCinemachine != null) _menuCinemachine.Priority = 20;
        if (_gameplayCinemachine != null) _gameplayCinemachine.Priority = 10;

        _playerReferences.Input.enabled = false;
        gameplayCamera.SetActive(false);
        SetGameUIEnabled(false);

        Cursor.lockState = CursorLockMode.None;

        _gameStarted = false;
    }

    private void BeginGameplay()
    {
        if (_menuCinemachine != null) _menuCinemachine.Priority = 10;
        if (_gameplayCinemachine != null) _gameplayCinemachine.Priority = 20;

        gameplayCamera.SetActive(true);
        SetGameUIEnabled(true);
        _playerReferences.Input.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;

        // Fade in from black
        StartCoroutine(FadeFromBlack(1f));
    }

    private IEnumerator ReturnToMainMenuBackground()
    {
        // Re-enable Cinemachine on menu camera if it was disabled
        if (_menuCinemachine != null)
        {
            _menuCinemachine.enabled = true;
        }

        // Reset menu camera to original position
        if (menuCamera != null)
        {
            Camera actualCamera = menuCamera.GetComponentInChildren<Camera>();
            if (actualCamera != null)
            {
                actualCamera.transform.position = _menuCameraStartPos;
                actualCamera.transform.rotation = _menuCameraStartRot;
            }
            else
            {
                menuCamera.transform.position = _menuCameraStartPos;
                menuCamera.transform.rotation = _menuCameraStartRot;
            }
            menuCamera.SetActive(true);
        }

        // Set menu camera priority (player is in the menu scene)
        if (_menuCinemachine != null) _menuCinemachine.Priority = 20;
        if (_gameplayCinemachine != null) _gameplayCinemachine.Priority = 10;

        // Keep menu UI hidden
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;
        }

        // Fade from black to reveal the scene
        yield return StartCoroutine(FadeFromBlack(1f));
    }

    private IEnumerator PlayStandUpAnimation()
    {
        // Trigger the standup animation
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(standUpTrigger);
        }

        // Wait for animation to complete
        yield return new WaitForSeconds(standUpAnimationDuration);
    }

    private void SwitchToGameplayCamera()
    {
        // Switch camera priorities
        if (_menuCinemachine != null) _menuCinemachine.Priority = 10;
        if (_gameplayCinemachine != null) _gameplayCinemachine.Priority = 20;

        // Enable gameplay camera
        if (gameplayCamera != null)
        {
            gameplayCamera.SetActive(true);
        }

        // Enable game UI
        SetGameUIEnabled(true);

        // Enable player input
        if (_playerReferences != null && _playerReferences.Input != null)
        {
            _playerReferences.Input.enabled = true;
        }

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    private IEnumerator FadeOutMusic(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume; // Reset volume for next time
    }

    private void SetGameUIEnabled(bool enabled)
    {
        if (healthBar != null) healthBar.SetActive(enabled);
    }

    public void StartGame()
    {
        if (_gameStarted || _isTransitioning) return;
        _gameStarted = true;

        // Start the cutscene sequence
        StartCoroutine(PlayCutsceneSequence());
    }

    private IEnumerator PlayCutsceneSequence()
    {
        _isTransitioning = true;

        // Mute all audio via mixer
        if (audioMixer != null)
        {
            audioMixer.SetFloat(mixerVolumeParameter, -80f);
        }

        // Stop menu music
        if (menuMusicSource != null && menuMusicSource.isPlaying)
        {
            menuMusicSource.Stop();
        }

        yield return StartCoroutine(FadeMenuOut());

        yield return StartCoroutine(ZoomToBrain());

        yield return StartCoroutine(FadeToBlack(0.5f));

        // Unmute audio for video playback
        if (audioMixer != null)
        {
            audioMixer.SetFloat(mixerVolumeParameter, 0f); // Back to normal volume
        }

        // Step 4: Play video cutscene
        if (videoPlayer != null && cutsceneVideo != null)
        {
            yield return StartCoroutine(PlayVideoCutscene());
        }

        // Mute again after video
        if (audioMixer != null)
        {
            audioMixer.SetFloat(mixerVolumeParameter, -80f);
        }

        // After video, return to main menu background (still hidden UI)
        // Menu stays at alpha 0, buttons hidden
        yield return StartCoroutine(ReturnToMainMenuBackground());

        // Trigger standup animation
        yield return StartCoroutine(PlayStandUpAnimation());

        // Switch to gameplay camera and enable input
        SwitchToGameplayCamera();

        // Unmute audio for gameplay
        if (audioMixer != null)
        {
            audioMixer.SetFloat(mixerVolumeParameter, 0f);
        }

        _isTransitioning = false;
    }

    private IEnumerator FadeMenuOut()
    {
        if (menuCanvasGroup == null) yield break;

        float elapsed = 0f;

        while (elapsed < menuFadeDuration)
        {
            elapsed += Time.deltaTime;
            menuCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / menuFadeDuration);
            yield return null;
        }

        menuCanvasGroup.alpha = 0f;

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
    }

    private IEnumerator ZoomToBrain()
    {
        if (brainTarget == null)
        {
            Debug.LogWarning("Brain target not assigned! Skipping zoom.");
            yield break;
        }

        if (menuCamera == null)
        {
            Debug.LogWarning("Menu camera not assigned! Skipping zoom.");
            yield break;
        }

        // Make sure menu camera is active and has priority
        menuCamera.SetActive(true);
        if (_menuCinemachine != null)
        {
            // Temporarily disable Cinemachine to manually control camera
            _menuCinemachine.enabled = false;
        }

        // Get the actual camera component
        Camera actualCamera = menuCamera.GetComponentInChildren<Camera>();
        Transform cameraTransform;

        if (actualCamera != null)
        {
            cameraTransform = actualCamera.transform;
        }
        else
        {
            cameraTransform = menuCamera.transform;
        }

        float elapsed = 0f;
        Vector3 startPos = cameraTransform.position;
        Quaternion startRot = cameraTransform.rotation;

        // Calculate target position (close to brain)
        Vector3 directionToBrain = (brainTarget.position - startPos).normalized;
        Vector3 targetPos = brainTarget.position - directionToBrain * zoomDistance;
        Quaternion targetRot = Quaternion.LookRotation(brainTarget.position - targetPos);

        while (elapsed < zoomDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / zoomDuration;

            // Use animation curve for smooth easing
            float curveT = zoomCurve.Evaluate(t);

            cameraTransform.position = Vector3.Lerp(startPos, targetPos, curveT);
            cameraTransform.rotation = Quaternion.Slerp(startRot, targetRot, curveT);

            yield return null;
        }

        cameraTransform.position = targetPos;
        cameraTransform.rotation = targetRot;
    }

    private IEnumerator FadeToBlack(float duration)
    {
        if (fadePanel == null) yield break;

        fadePanel.blocksRaycasts = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }

        fadePanel.alpha = 1f;
    }

    private IEnumerator FadeFromBlack(float duration)
    {
        if (fadePanel == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }

        fadePanel.alpha = 0f;
        fadePanel.blocksRaycasts = false;
    }

    private IEnumerator PlayVideoCutscene()
    {
        Debug.Log("Starting video playback");

        // Keep menu camera active but lower priority
        if (menuCamera != null)
        {
            menuCamera.SetActive(true);
        }

        // Disable gameplay camera
        if (gameplayCamera != null)
        {
            gameplayCamera.SetActive(false);
        }

        // Show video display and make sure it's on top
        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(true);
            videoDisplay.transform.SetAsLastSibling(); // Bring to front
            Canvas.ForceUpdateCanvases();
        }
        else
        {
            Debug.LogError("Video display is not assigned!");
        }

        // Show skip button
        if (skipButton != null)
        {
            skipButton.SetActive(true);
            skipButton.transform.SetAsLastSibling();
        }

        // Prepare and start video
        if (videoPlayer != null)
        {
            videoPlayer.Prepare();

            // Wait for video to be prepared
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            videoPlayer.Play();

            // Wait a frame to ensure playback started
            yield return null;

            // Wait for video to finish
            while (videoPlayer.isPlaying)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogError("Video player is not assigned!");
        }

        // Hide video and skip button
        if (videoDisplay != null)
        {
            videoDisplay.gameObject.SetActive(false);
        }

        if (skipButton != null)
        {
            skipButton.SetActive(false);
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        // Video finished naturally
        Debug.Log("Video finished");
    }

    public void SkipVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif

        Application.Quit();
    }
}