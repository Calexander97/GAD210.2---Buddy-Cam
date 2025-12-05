using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStateManagerTMP : MonoBehaviour
{
    [Header("Messages")]
    [TextArea] public string deathMessage = "Mission Failed\nPress R to Restart";
    [TextArea] public string successMessage = "Mission Complete\nPress R to Restart";

    [Header("Optional prebuilt UI (TMP)")]
    public GameObject messagePanel;   // optional: your own overlay panel
    public TMP_Text messageTMP;       // optional: your own TMP_Text

    bool waitingForRestart = false;
    bool hookedHero = false;
    bool hookedObjectives = false;

    HeroHealth hero;
    ObjectiveManager objMgr;

    void Awake()
    {
        if (messagePanel) messagePanel.SetActive(false);
    }

    void OnEnable()
    {
        TryHook();
    }

    void Update()
    {
        // Keep trying to hook in case Hero/ObjectiveManager are spawned later
        if (!hookedHero || !hookedObjectives) TryHook();

        if (!waitingForRestart) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void TryHook()
    {
        if (!hookedHero)
        {
            hero = FindFirstObjectByType<HeroHealth>();
            if (hero != null)
            {
                // Hook both: UnityEvent + C# event (covers all bases)
                hero.onDeath.AddListener(OnHeroDeathUnity);
                hero.OnDied += OnHeroDied;
                hookedHero = true;
            }
        }

        if (!hookedObjectives)
        {
            objMgr = ObjectiveManager.I;
            if (objMgr != null)
            {
                objMgr.onMissionComplete.AddListener(OnMissionComplete);
                hookedObjectives = true;
            }
        }
    }

    // ===== Event sinks =====
    void OnHeroDeathUnity() => OnHeroDied();
    void OnHeroDied() => EnterRestartState(deathMessage);
    void OnMissionComplete() => EnterRestartState(successMessage);

    // In case you prefer wiring via Inspector (UnityEvent) on ObjectiveManager:
    // ObjectiveManager.onMissionComplete -> GameStateManagerTMP.NotifyMissionComplete
    public void NotifyMissionComplete() => OnMissionComplete();

    void EnterRestartState(string msg)
    {
        if (waitingForRestart) return;

        EnsureOverlayExists();
        messageTMP.text = msg;
        if (messagePanel) messagePanel.SetActive(true);

        Time.timeScale = 0f;
        waitingForRestart = true;
    }

    // ===== Minimal TMP overlay builder (uses default TMP font) =====
    void EnsureOverlayExists()
    {
        if (messageTMP != null && messagePanel != null) return;

        // Build overlay Canvas
        var root = new GameObject("RestartOverlay_Tmp");
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000; // top-most
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();

        // Dim BG
        var bg = new GameObject("BG").AddComponent<Image>();
        bg.transform.SetParent(root.transform, false);
        bg.color = new Color(0f, 0f, 0f, 0.6f);
        var bgRt = (RectTransform)bg.transform;
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;

        // TMP label
        var label = new GameObject("Message").AddComponent<TextMeshProUGUI>();
        label.transform.SetParent(root.transform, false);
        var rt = (RectTransform)label.transform;
        rt.anchorMin = new Vector2(0.15f, 0.35f);
        rt.anchorMax = new Vector2(0.85f, 0.65f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        // Make sure we have a font (important!)
        if (TMP_Settings.defaultFontAsset != null)
            label.font = TMP_Settings.defaultFontAsset;

        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 42;
        label.color = Color.white;
        label.enableWordWrapping = true;
        label.raycastTarget = false;

        messagePanel = root;
        messageTMP = label;
        messagePanel.SetActive(false);
    }

    // Safety: unhook on destroy
    void OnDisable()
    {
        if (hero != null)
        {
            hero.onDeath.RemoveListener(OnHeroDeathUnity);
            hero.OnDied -= OnHeroDied;
        }
        if (objMgr != null)
        {
            objMgr.onMissionComplete.RemoveListener(OnMissionComplete);
        }
    }
}
