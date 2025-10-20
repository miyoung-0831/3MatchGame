using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
    private Dictionary<string, Texture> textureCache = new Dictionary<string, Texture>();

    public SpriteAtlas spriteAtlas;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        spriteAtlas = Resources.Load<SpriteAtlas>("Atlas/BlockAtlas");
    }


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
