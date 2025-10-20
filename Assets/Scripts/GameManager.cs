using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] Board board = null;
    [SerializeField] UIGame uiGame = null;

    public bool IsSelectedBlock => isSelectedBlock;
    private bool isSelectedBlock = false;
    private Block selectBlock = null;

    private int score = 0;
    private int swapCount = 0;

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
        board.OnClearBlock = ClearBlock;

        StartCoroutine(OnGameStart());
    }

    private IEnumerator OnGameStart()
    {
        yield return null;
        
        Setup();
        isSelectedBlock = false;
    }

    private void Setup()
    {
        board.OnGenerateBoard();
    }

    public void SelectBlock(Block block)
    {
        if (board.IsMoving)
            return;

        isSelectedBlock = block != null;
        selectBlock = block;
    }

    public void SwapBlock(Block block)
    {
        if (board.IsMoving)
            return;

        if (selectBlock != block)
        {
            var dir = new Vector2(block.x - selectBlock.x, block.y - selectBlock.y);
            dir.Normalize();

            Debug.Log($"{selectBlock.type} <=> {block.type} / dir {dir} / {Mathf.RoundToInt(dir.x)} , {Mathf.RoundToInt(dir.y)}");
            board.SwapBlock(selectBlock, block, Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y));

            isSelectedBlock = false;

            swapCount++;

            uiGame.UpdateCount(swapCount);
        }
    }

    public void ClearBlock(List<Block> blocks)
    {
        uiGame.ClearBlock(blocks);
    }
}