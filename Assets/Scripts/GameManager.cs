using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

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

    private Coroutine hintCoroutine = null;
    private WaitForSeconds hintTime = new WaitForSeconds(Define.HintDelayTime);
    List<Block> hintBlocks = null;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        board.OnClearBlock = OnClearBlock;
        board.OnEndSwap = OnEndSwap;
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

        StartHintTitmer();
    }

    private void Setup()
    {
        board.OnGenerateBoard();
    }

    // 이동 시킬 블록 선택
    public void SelectBlock(Block block)
    {
        if (board.IsMoving)
            return;

        isSelectedBlock = block != null;
        selectBlock = block;
    }

    // 블록 스왑
    public void SwapBlock(Block block)
    {
        if (selectBlock == block)
            return;

        if (board.IsMoving)
            return;

        // 인접 블록이 아니면 한칸만 이동
        Vector2 dir = new Vector2(Mathf.Abs(block.x - selectBlock.x), Mathf.Abs(block.y - selectBlock.y));
        if (dir.x > 1 || dir.y > 1)
        {
            dir = new Vector2(block.x - selectBlock.x, block.y - selectBlock.y);
            dir.x = dir.x == 0 ? 0 : dir.x / Mathf.Abs(dir.x);
            dir.y = dir.y == 0 ? 0 : dir.y / Mathf.Abs(dir.y);

            block = board.GetBlock(selectBlock.x + (int)dir.x, selectBlock.y + (int)dir.y);

            if (block == null)
                return;
        }

        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }

        HideHint();

        Debug.Log($"{selectBlock.type} <=> {block.type}");
        board.SwapBlock(selectBlock, block);

        isSelectedBlock = false;
        swapCount++;

        uiGame.UpdateCount(swapCount);
    }

    // 블록이 제거되었을 때 호출되는 콜백 함수
    public void OnClearBlock(List<Block> blocks)
    {
        var topSpin = blocks.Where(_ => _.type == Define.BlockType.TopSpin).Count();
        var normalBlock = blocks.Count - topSpin;

        score += normalBlock * Define.NormalBlockScore;
        score += topSpin * Define.TopSpinBlockScore;

        uiGame.UpdateScore(score);
        
        if (topSpin > 0)
            uiGame.UpdateTopSpin(topSpin);

        StartHintTitmer();
    }

    // Swap이 끝났을 때 호출되는 콜백 함수
    public void OnEndSwap(bool isShuffle)
    {
        StartHintTitmer();

        if (isShuffle)
            uiGame.ShowShuffle();
    }

    // 힌트 타이머 시작
    private void StartHintTitmer()
    {
        if (hintCoroutine != null)
            StopCoroutine(hintCoroutine);

        hintCoroutine = StartCoroutine(HintTimer());
    }

    IEnumerator HintTimer()
    {
        while (board.IsMoving)
            yield return null;

        yield return hintTime;

        hintBlocks = board.FindHint();

        if (hintBlocks != null && hintBlocks.Count > 0)
            ShowHint();

        hintCoroutine = null;
    }

    private void ShowHint()
    {
        foreach (var block in hintBlocks)
        {
            var tile = board.GetBackgroundTile(block.x, block.y);
            tile.SetHintActive(true);
        }
    }

    private void HideHint()
    {
        if (hintBlocks == null || hintBlocks.Count == 0)
            return;

        foreach (var block in hintBlocks)
        {
            var tile = board.GetBackgroundTile(block.x, block.y);
            tile.SetHintActive(false);
        }

        hintBlocks.Clear();
    }
}