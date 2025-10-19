using System.Collections.Generic;
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

    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    public Sprite GetSprite(string name)
    {
        if (spriteCache.TryGetValue(name, out Sprite sprite) && sprite != null)
        {
            return sprite;
        }

        var res = spriteAtlas.GetSprite(name);
        spriteCache[name] = res;

        return res;
    }

    private Dictionary<string, Texture> textureCache = new Dictionary<string, Texture>();

    public Texture GetTexture(string name)
    {
        if (textureCache.TryGetValue(name, out Texture texture) && texture != null)
        {
            return texture;
        }

        var res = Resources.Load<Texture>(name);
        textureCache[name] = res;

        return res;
    }
}
