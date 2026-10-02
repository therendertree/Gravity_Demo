using UnityEngine;
using UnityEngine.UI;

// shows the controls in the top-left corner of the screen
public class ControlsUI : MonoBehaviour{
    // the text shown on screen
    [TextArea(3, 6)]
    public string controlsText =
        "Left / Right Arrow : Move\n" +
        "Space : Jump\n" +
        "G : Flip gravity\n" +
        "F : Switch color";

    public int fontSize = 28;
    public Color textColor = Color.white;

    void Start(){
        // canvas that draws on top of the game
        GameObject canvasObject = new GameObject("ControlsCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; // below the level complete screen (100)

        // keeps the text the same size on any screen
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        // he text object
        GameObject textObject = new GameObject("ControlsText", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(canvasObject.transform, false);

        //SO it's om  the top-left corner
        RectTransform rt = textObject.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(30, -30);
        rt.sizeDelta = new Vector2(600, 300);

        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = controlsText;
        text.fontSize = fontSize;
        text.color = textColor;
        text.alignment = TextAnchor.UpperLeft;
        text.raycastTarget = false;
    }
}
