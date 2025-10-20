using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] Board board = null;

    public bool IsSelectedBlock => isSelectedBlock;
    private bool isSelectedBlock = false;
    private Block selectBlock = null;

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
        }
    }
}