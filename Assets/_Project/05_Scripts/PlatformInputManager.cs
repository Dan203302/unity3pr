using UnityEngine;
using UnityEngine.InputSystem;

public class PlatformInputManager : MonoBehaviour
{
    [System.Serializable]
    public class InputSettings
    {
        [Header("PC Settings")]
        public float mouseSensitivity = 2f;
        public Key   interactKey = Key.E;
        public Key   jumpKey     = Key.Space;
        public Key   sprintKey   = Key.LeftShift;

        [Header("Mobile Settings")]
        public float touchSensitivity = 5f;
        public float joystickDeadZone = 0.2f;
        public bool  enableGyro = true;

        [Header("WebGL Settings")]
        public bool useWebGLSpecificControls = false;
    }

    [SerializeField] private InputSettings settings = new InputSettings();

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool    jumpPressed;
    private bool    interactPressed;
    private bool    sprintPressed;

    // Мобильное управление (простые флаги, без внешних джойстик-пакетов)
    private Vector2 touchMoveInput;
    private Vector2 touchLookInput;
    private bool    mobileJumpPressed;
    private bool    mobileInteractPressed;

    public static PlatformInputManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeForPlatform();
    }

    void InitializeForPlatform()
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        SetupPCControls();
#elif UNITY_ANDROID || UNITY_IOS
        SetupMobileControls();
#elif UNITY_WEBGL
        SetupWebGLControls();
#endif
    }

    void SetupPCControls()
    {
        Debug.Log("[PlatformInput] PC controls initialized.");
    }

    void SetupMobileControls()
    {
        Debug.Log("[PlatformInput] Mobile controls initialized.");
        if (SystemInfo.supportsGyroscope && settings.enableGyro)
            Input.gyro.enabled = true;
    }

    void SetupWebGLControls()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Debug.Log("[PlatformInput] WebGL controls initialized.");
    }

    void Update()
    {
#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL
        UpdatePCInput();
#elif UNITY_ANDROID || UNITY_IOS
        UpdateMobileInput();
#endif
    }

    void UpdatePCInput()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;

        if (kb != null)
        {
            float h = (kb.dKey.isPressed ? 1 : 0) - (kb.aKey.isPressed ? 1 : 0);
            float v = (kb.wKey.isPressed ? 1 : 0) - (kb.sKey.isPressed ? 1 : 0);
            moveInput = new Vector2(h, v);

            jumpPressed     = kb[settings.jumpKey].wasPressedThisFrame;
            interactPressed = kb[settings.interactKey].wasPressedThisFrame;
            sprintPressed   = kb[settings.sprintKey].isPressed;
        }

        if (mouse != null)
        {
            var delta = mouse.delta.ReadValue();
            lookInput = delta * settings.mouseSensitivity * 0.1f;
        }
    }

    void UpdateMobileInput()
    {
        // Простой тач-ввод: левая половина — движение, правая — взгляд
        moveInput = touchMoveInput;
        lookInput = touchLookInput;
        jumpPressed     = mobileJumpPressed;
        interactPressed = mobileInteractPressed;
        mobileJumpPressed = mobileInteractPressed = false;

        if (Input.touchCount == 0) return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            bool leftSide = t.position.x < Screen.width * 0.5f;

            if (t.phase == UnityEngine.TouchPhase.Moved)
            {
                if (leftSide)
                    touchMoveInput = t.deltaPosition.normalized;
                else
                    touchLookInput = t.deltaPosition * settings.touchSensitivity * 0.01f;
            }
            else if (t.phase == UnityEngine.TouchPhase.Ended || t.phase == UnityEngine.TouchPhase.Canceled)
            {
                if (leftSide) touchMoveInput = Vector2.zero;
                else          touchLookInput = Vector2.zero;
            }
        }

        if (moveInput.magnitude < settings.joystickDeadZone) moveInput = Vector2.zero;
    }

    // ── Публичное API ────────────────────────────────────────────────────────
    public Vector2 GetMoveInput()      => moveInput;
    public Vector2 GetLookInput()      => lookInput;
    public bool    IsJumpPressed()     => jumpPressed;
    public bool    IsInteractPressed() => interactPressed;
    public bool    IsSprintPressed()   => sprintPressed;

    public void SetMouseSensitivity(float v)
    {
        settings.mouseSensitivity = v;
        PlayerPrefs.SetFloat("MouseSensitivity", v);
    }

    public void SetTouchSensitivity(float v)
    {
        settings.touchSensitivity = v;
        PlayerPrefs.SetFloat("TouchSensitivity", v);
    }

    // Вызывается мобильными кнопками UI
    public void OnMobileJump()     => mobileJumpPressed     = true;
    public void OnMobileInteract() => mobileInteractPressed = true;
}
