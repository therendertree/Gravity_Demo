using UnityEngine;

//the two colors that we use in the game
//pink  = #FD5ED0 
//Blue  = #87EAEC 
public enum GameColor { Pink, Cyan }

// press F to switch between pink and blue
[RequireComponent(typeof(Collider2D))]
public class PlayerColorSwitcher : MonoBehaviour{
    // let other scripts access the current color of the player
    private static PlayerColorSwitcher instance;

    // access to the current instance of the PlayerColorSwitcher script
    public static PlayerColorSwitcher Instance{
        get { return instance; }
        private set { instance = value; }
    }
    //the color the player starts with
    public GameColor startColor = GameColor.Pink;

    //key used to switch color
    public KeyCode switchKey = KeyCode.F;

    // the current color of the player
    private GameColor currentColor;
    public GameColor CurrentColor{
        get { return currentColor; }
        private set { currentColor = value; }
    }
    Collider2D playerCollider;
    SpriteRenderer spriteRenderer;

    //refresh the collision settings for all obstacles when the player color change
    void Awake(){
        Instance = this;
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //set the initial color of the player and apply it to the sprite renderer
    void Start(){
        CurrentColor = startColor;
        ApplyColor();
    }
    // check for input to switch color
    void Update(){
        if (Input.GetKeyDown(switchKey)){
            TrySwitchColor();
        }
    }

    void TrySwitchColor(){
        //determine the next color to switch to
        GameColor nextColor;

        if (CurrentColor == GameColor.Pink){
            nextColor = GameColor.Cyan;
        }
        else{
            nextColor = GameColor.Pink;
        }

        //don't allow switching if the player is inside an obstacle
        foreach (ColorObstacle obstacle in ColorObstacle.All){
            if (obstacle.obstacleColor != nextColor && playerCollider.Distance(obstacle.Col).isOverlapped){
                return;
            }
        }

        CurrentColor = nextColor;
        ApplyColor();
    }

    void ApplyColor(){
        if (spriteRenderer != null) spriteRenderer.color = ToUnityColor(CurrentColor);

        foreach (ColorObstacle obstacle in ColorObstacle.All)
            RefreshCollision(obstacle);
    }

    // Matching color = ignore collision (pass through). Different color = solid.
    public void RefreshCollision(ColorObstacle obstacle){
        bool passThrough = obstacle.obstacleColor == CurrentColor;
        Physics2D.IgnoreCollision(playerCollider, obstacle.Col, passThrough);
    }

    // convert gameColor to Unity Color
    public static Color ToUnityColor(GameColor c)
    {
        switch (c) {   // Unity Color32 uses RGBA values in the range 0-255
            case GameColor.Pink: return new Color32(0xFD, 0x5E, 0xD0, 0xFF); // #FD5ED0
            case GameColor.Cyan: return new Color32(0x87, 0xEA, 0xEC, 0xFF); // #87EAEC
            default:             return Color.white;
        }
    }
}
