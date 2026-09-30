using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Level complete screen: built entirely in code, no need to set up a Canvas in the scene
// Call LevelCompleteUI.Show(clearTime) to display it
public class LevelCompleteUI : MonoBehaviour
{
    const float FadeTime = 0.4f;

    CanvasGroup group;
    bool hasNextLevel;

    public static void Show(float clearTime)
    {
        var go = new GameObject("LevelCompleteUI");
        go.AddComponent<LevelCompleteUI>().Build(clearTime);
    }

    void Build(float clearTime)
    {
        EnsureEventSystem();

        int next = SceneManager.GetActiveScene().buildIndex + 1;
        hasNextLevel = next < SceneManager.sceneCountInBuildSettings;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Canvas
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;

        // Semi-transparent overlay covering the whole screen
        var dim = CreateImage("Dim", transform, new Color(0f, 0f, 0f, 0.6f));
        Stretch(dim.rectTransform);

        // Center panel
        var panel = CreateImage("Panel", transform, new Color(0.12f, 0.14f, 0.2f, 0.95f));
        panel.rectTransform.sizeDelta = new Vector2(640, 420);

        CreateText("Title", panel.transform, font, "Level Complete！", 72, new Color(1f, 0.85f, 0.3f), new Vector2(0, 110));
        CreateText("Time", panel.transform, font, $"Time  {FormatTime(clearTime)}", 40, Color.white, new Vector2(0, 10));

        string hint = hasNextLevel ? "Enter Next Level   ·   R Restart" : "R Restart";
        CreateText("Hint", panel.transform, font, hint, 24, new Color(1f, 1f, 1f, 0.5f), new Vector2(0, -175));

        if (hasNextLevel)
        {
            CreateButton("Restart", panel.transform, font, "Restart", new Vector2(-140, -90), Restart);
            CreateButton("Next", panel.transform, font, "Next Level", new Vector2(140, -90), NextLevel);
        }
        else
        {
            CreateButton("Restart", panel.transform, font, "Restart", new Vector2(0, -90), Restart);
        }

        StartCoroutine(FadeIn(panel.rectTransform));
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.rKey.wasPressedThisFrame) Restart();
        else if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
        {
            if (hasNextLevel) NextLevel();
            else Restart();
        }
    }

    IEnumerator FadeIn(RectTransform panel)
    {
        // Fade in + slight panel pop-in
        for (float t = 0f; t < FadeTime; t += Time.unscaledDeltaTime)
        {
            float k = t / FadeTime;
            group.alpha = k;
            panel.localScale = Vector3.one * Mathf.Lerp(0.8f, 1f, 1f - (1f - k) * (1f - k));
            yield return null;
        }
        group.alpha = 1f;
        panel.localScale = Vector3.one;
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void NextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    static string FormatTime(float seconds)
    {
        int m = (int)(seconds / 60f);
        float s = seconds - m * 60;
        return $"{m:00}:{s:00.00}";
    }

    // Buttons need an EventSystem to be clickable; create one if the scene has none (using the new Input System module)
    static void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    // ---------- Helpers for creating UI elements ----------

    static Image CreateImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    static Text CreateText(string name, Transform parent, Font font, string content, int size, Color color, Vector2 pos)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(600, size + 20);
        rt.anchoredPosition = pos;

        var text = go.GetComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        return text;
    }

    static void CreateButton(string name, Transform parent, Font font, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var img = CreateImage(name, parent, new Color(0.3f, 0.55f, 0.95f));
        img.rectTransform.sizeDelta = new Vector2(240, 70);
        img.rectTransform.anchoredPosition = pos;

        var btn = img.gameObject.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f);
        colors.pressedColor = new Color(0.8f, 0.8f, 0.8f);
        btn.colors = colors;
        btn.onClick.AddListener(onClick);

        var text = CreateText("Label", img.transform, font, label, 32, Color.white, Vector2.zero);
        text.rectTransform.sizeDelta = img.rectTransform.sizeDelta;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
