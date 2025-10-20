using UnityEngine;
using UnityEngine.Pool;

public class BlockPool : MonoBehaviour
{
    public static BlockPool Instance { get; private set; }

    public GameObject blockPrefab = null;

    private IObjectPool<GameObject> blockPool;

    private int initSize = 50;
    private int maxSize = 60;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;

        DontDestroyOnLoad(gameObject);

        blockPool = new ObjectPool<GameObject>(CreateBlock, OnActiveBlock, OnReleaseBlock, DestroyBlock, true, initSize, maxSize);
    }

    private GameObject CreateBlock()
    {
        return Instantiate(blockPrefab);
    }

    private void OnActiveBlock(GameObject block)
    {
        block.SetActive(true);
    }

    private void OnReleaseBlock(GameObject block)
    {
        block.SetActive(false);
    }

    private void DestroyBlock(GameObject block)
    {
        Destroy(block);
    }

    public GameObject GetBlock()
    {
        return blockPool.Get();
    }

    public void ReturnBlock(GameObject block)
    {
        blockPool.Release(block);
    }

}