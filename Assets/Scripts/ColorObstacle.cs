using System.Collections.Generic;
using UnityEngine;

//attach to any colored block so if the player's color matches the player passes through.
[RequireComponent(typeof(Collider2D))]
public class ColorObstacle : MonoBehaviour{
    private static List<ColorObstacle> all = new List<ColorObstacle>();

    public static List<ColorObstacle> All{
        get { return all; }
    }

    public GameColor obstacleColor = GameColor.Cyan;

    //if true the sprite will be tinted to match the obstacle color
    public bool tintSprite = true;

    //collider2D component of this obstacle used for collision detection.
    private Collider2D col;

    // access the collider2D component of this obstacle
    public Collider2D Col{
        get { return col; }
        private set { col = value; }
    }

    // update the sprite color in the editor and at runtime
    void Awake(){
        Col = GetComponent<Collider2D>();
        UpdateSpriteColor();
    }

    //add this obstacle to the list of all obstacles when enabled and remove it when disabled
    void OnEnable(){
        All.Add(this);
        if (PlayerColorSwitcher.Instance != null)
            PlayerColorSwitcher.Instance.RefreshCollision(this);
    }

    void OnDisable(){
        All.Remove(this);
    }
    void OnValidate(){
        UpdateSpriteColor();
    }

    void UpdateSpriteColor(){
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        // show the sprite's original colors
        if (tintSprite){
                sr.color = PlayerColorSwitcher.ToUnityColor(obstacleColor);
            }
            else{
                sr.color = Color.white;
            }
    }
}
