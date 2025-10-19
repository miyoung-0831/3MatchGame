using UnityEngine;
using UnityEngine.U2D;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance { get; private set; }

    public SpriteAtlas spriteAtlas;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    private void Start()
    {
        spriteAtlas = Resources.Load<SpriteAtlas>("Atlas/TileAtlas");
    }

    public Sprite Get(string name)
    {
        return spriteAtlas.GetSprite(name);
    }
}
