using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static Define;

public class Board : MonoBehaviour
{
    [Header("배경 블록")]
    [SerializeField] private GameObject goBackgroudBTile = null;
    [SerializeField] private Transform trBackround = null;

    [Header("색 블록")]
    [SerializeField] private Transform trBlock = null;

    [Header("레벨 데이터")]
    [SerializeField] private LevelData levelData = null;

    public BackgroundTile GetBackgroundTile(int x, int y)
    {
        if (backTiles.TryGetValue((x, y), out BackgroundTile tile))
            return tile;
        return null;
    }

    public Block GetBlock(int x, int y)
    {
        if (board.TryGetValue((x, y), out Block block))
            return block;
        return null;
    }

    private Dictionary<(int, int), BackgroundTile> backTiles = new Dictionary<(int, int), BackgroundTile>();
    private Dictionary<(int, int), Block> board = new Dictionary<(int, int), Block>();

    private float blockSize = 0.8f;
    private int width = 7;
    private int height = 6;
    private (int, int) spwanPoint = (3, 6);

    public bool IsMoving => isMoving;
    private bool isMoving = false;

    public System.Action<List<Block>> OnClearBlock = null;
    public System.Action<bool> OnEndSwap = null;

    public void OnGenerateBoard()
    {
        board.Clear();

        // 배경 타일 생성
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (InRhombus(x, y) == false)
                    continue;

                Vector3 pos = BlockToWorld(x, y);
                var backTile = Instantiate(goBackgroudBTile, pos, Quaternion.identity, trBackround); // 배경타일 생성
				backTile.name = $"Tile_{x}_{y}";
                backTiles[(x, y)] = backTile.GetComponent<BackgroundTile>();
                board[(x, y)] = null;
            }
        }

        // 레벨 데이터로 보드 생성
        var levelBlocks = levelData.GetData();
        foreach (var blockInfo in levelBlocks)
        {
            var (x, y) = blockInfo.Key;

            if (InRhombus(x, y) == false)
                continue;

            var type = blockInfo.Value;
            board[(x, y)] = GenerateBlock(x, y, type);
        }

        // 랜덤 생성 
        //GenerateBoardWithoutMatches();
    }

    private Block GenerateBlock(int x, int y, BlockType type)
    {
        Vector3 pos = BlockToWorld(x, y);
        var blockObject = BlockPool.Instance.GetBlock();
        blockObject.transform.SetParent(trBlock);
        blockObject.transform.position = pos;
        blockObject.transform.localScale = Vector3.one;
        blockObject.name = $"Block_{type}";

        var block = blockObject.GetComponent<Block>();
        block.Set(x, y, type, blockObject);

        return block;
    }

    private Vector3 BlockToWorld(int x, int y)
    {
        var newX = (1.5f * x);
        var newY = (Math.Sqrt(3) / 2 * x + Math.Sqrt(3) * y);
        newX = newX * (blockSize / 2f);
        newY = newY * ((blockSize * 0.93f) / Math.Sqrt(3));
        return new Vector3(newX, (float)newY) + new Vector3(-1.8f, -3f);
    }

    private bool InRhombus(int x, int y)
    {
        if (x + y < 3 || x + y > 8)
            return false;

        return true;
    }

    private void GenerateBoardWithoutMatches()
    {
        var keys = board.Keys.ToList();
        foreach (var (x, y) in keys)
        {
            BlockType type;
            int safety = 0;
            do
            {
                type = GetRandomBlockType();

                safety++;
                if (safety > 100)
                    break;
            }
            while (IsNeighborMatch(x, y, type));

            board[(x, y)] = GenerateBlock(x, y, type);
        }
    }

    private BlockType GetRandomBlockType()
    {
        return (BlockType) UnityEngine.Random.Range(0, (int)BlockType.ColorMax);
    }

    private bool IsNeighborMatch(int x, int y, BlockType type)
    {
        // 아래 2개의 블록에서 같은 색의 블록이 있는지 체크
        var (dX, dY) = Define.Directions[(int)Dir.Down];
        if (!IsEmptyBlock(x + dX * 2, y + dY * 2))
        {
            if (IsSameBlock(x + dX, y + dY, type) && IsSameBlock(x + dX * 2, y + dY * 2, type))
                return true;
        }

        var (ldX, ldY) = Define.Directions[(int)Dir.LeftDown];
        if (!IsEmptyBlock(x + ldX * 2, y + ldY * 2))
        {
            if (IsSameBlock(x + ldX, y + ldY, type) && IsSameBlock(x + ldX * 2, y + ldY * 2, type))
                return true;
        }

        var (rdX, rdY) = Define.Directions[(int)Dir.RightDown];
        if (!IsEmptyBlock(x + rdX * 2, y + rdY * 2))
        {
            if (IsSameBlock(x + rdX, y + rdY, type) && IsSameBlock(x + rdX * 2, y + rdY * 2, type))
                return true;
        }

        // 4개 만들어지는지 체크
        if (IsSameBlock(x + dX, y + dY, type) && IsSameBlock(x + ldX, y + ldY, type) && IsSameBlock(x + rdX, y + rdY, type))
            return true;

        return false;
    }

    private bool IsSameBlock(int x, int y, BlockType type)
    {
        if (IsEmptyBlock(x, y))
            return false;

        return board[(x, y)].type == type;
    }

    private bool IsEmptyBlock(int x, int y)
    {
        if (board.TryGetValue((x, y), out Block block) && block != null)
            return false;

        return true;
    }

    private bool IsTopSpin(int x, int y)
    {
        if (IsEmptyBlock(x, y))
            return false;

        return board[(x, y)].type == BlockType.TopSpin;
    }

    private bool IsTopSpin(Block block)
    {
        return block.type == BlockType.TopSpin;
    }

    // 블록 스왑
    public void SwapBlock(Block blockA, Block blockB)
    {
        isMoving = true;

        StartCoroutine(SwapBlockCoroutine(blockA, blockB));
    }

    // 블록 스왑 코루틴
    IEnumerator SwapBlockCoroutine(Block blockA, Block blockB)
    {
        var posA = (blockA.x, blockA.y);
        var posB = (blockB.x, blockB.y);
        var destA = BlockToWorld(blockB.x, blockB.y);
        var destB = BlockToWorld(blockA.x, blockA.y);

        blockA.Move(posB.x, posB.y, destA);
        blockB.Move(posA.x, posA.y, destB);
        board[(posA.x, posA.y)] = blockB;
        board[(posB.x, posB.y)] = blockA;

        yield return new WaitForSeconds(Define.BlockMoveTime);

        var matches = FindMatches();
        if (matches.Count > 0)
        {
            Debug.Log($"Match !! {matches.Count}");
            StartCoroutine(ClearBlock(matches));
        }
        else
        {
            Debug.Log($"Not Match !!");
            // 매치되지 않으니 블록 다시 되돌림.
            blockA.Move(posA.x, posA.y, destB);
            blockB.Move(posB.x, posB.y, destA);
            board[(posA.x, posA.y)] = blockA;
            board[(posB.x, posB.y)] = blockB;

            yield return new WaitForSeconds(Define.BlockMoveTime);

            isMoving = false;
            OnEndSwap?.Invoke(false);
        }
    }

    // 매치된 블록 찾기
    private List<Block> FindMatches()
    {
        var matched = new HashSet<Block>();
        var dirs = new (int, int)[] { Directions[(int)Dir.Up], Directions[(int)Dir.LeftUp], Directions[(int)Dir.RightUp] };

        foreach (var position in board)
        {
            var block = position.Value;
            if (block.type == BlockType.TopSpin)
                continue;

            var matcheBlocks = FindMatch(block);

            matched.AddRange(matcheBlocks);
        }

        return matched.ToList();
    }

    // 특정 블록 기준으로 매치되는 블록 찾기
    private HashSet<Block> FindMatch(Block block)
    {
        var matched = new HashSet<Block>();

        var dirs = new (int, int)[] { Directions[(int)Dir.Up], Directions[(int)Dir.LeftUp], Directions[(int)Dir.RightUp] };
        var match4 = new List<Block>() { block };

        foreach (var dir in dirs)
        {
            var line = new List<Block>() { block };
            // Up 방향
            int dx = block.x + dir.Item1;
            int dy = block.y + dir.Item2;

            if (IsSameBlock(dx, dy, block.type)) // 라인 체크하면서 dx, dy 값이 변경되어 4매치 먼저 체크
                match4.Add(board[(dx, dy)]);

            while (board.TryGetValue((dx, dy), out var dBlock) && dBlock.type == block.type)
            {
                line.Add(dBlock);
                dx += dir.Item1;
                dy += dir.Item2;
            }

            // Down 방향
            dx = block.x - dir.Item1;
            dy = block.y - dir.Item2;

            if (IsSameBlock(dx, dy, block.type))
                match4.Add(board[(dx, dy)]);

            while (board.TryGetValue((dx, dy), out var dBlock) && dBlock.type == block.type)
            {
                line.Add(dBlock);
                dx -= dir.Item1;
                dy -= dir.Item2;
            }

            if (line.Count >= 3)
            {
                foreach (var c in line)
                    matched.Add(c);
            }
        }

        if (match4.Count >= 4)
        {
            foreach (var c in match4)
                matched.Add(c);
        }

        return matched;
    }

    // 매치된 블록 제거
    IEnumerator ClearBlock(List<Block> matches)
    {
        HashSet<(int, int)> unlockBlocks = null;
        HashSet<Block> topspinBlocks = null;

        foreach (var match in matches)
        {
            match.ClearBlock();
            foreach (var dir in Define.Directions)
            {
                var (dx, dy) = dir;
                var x = match.x + dx;
                var y = match.y + dy;

                if (IsTopSpin(x, y))
                {
                    if (board[(x, y)].IsLock)
                    {
                        if (unlockBlocks == null)
                            unlockBlocks = new HashSet<(int, int)>();
                        unlockBlocks.Add((x, y));
                    }
                    else
                    {
                        if (topspinBlocks == null)
                            topspinBlocks = new HashSet<Block>();
                        
                        // 이미 팽이가 추가 된 경우 제외
                        if (topspinBlocks.Add(board[(x, y)]))
                            board[(x, y)].ClearBlock();
                    }
                }
            }
        }

        // 팽이를 돌리자
        if (unlockBlocks != null)
        {
            foreach (var pos in unlockBlocks)
            {
                var (x, y) = pos;
                var block = board[(x, y)];
                block.UnlockTopSpin();
            }
        }

        if (topspinBlocks != null)
            matches.AddRange(topspinBlocks);

        OnClearBlock?.Invoke(matches);

        yield return new WaitForSeconds(Define.ClearBlockDelayTime);

        foreach (var match in matches)
        {
            BlockPool.Instance.ReturnBlock(match.BlockObject);
            board[(match.x, match.y)] = null;
        }

        StartCoroutine(FillBlock());
    }

    // 빈 블록 채우기
    IEnumerator FillBlock()
    {
        var emptyBlocks = board.Where(_ => _.Value == null).OrderBy(_ => _.Key.Item1).ThenByDescending(_ => _.Key.Item2).ToList();

        bool isAddEmpty = false;

        while (emptyBlocks.Count > 0)
        {
            isAddEmpty = false;
            foreach (var emptyBlock in emptyBlocks)
            {
                var (x, y) = emptyBlock.Key;
                var (dX, dY) = Define.Directions[(int)Dir.Up];
                if (board.TryGetValue((x + dX, y + dY), out var upBlock) && upBlock != null)
                {
                    var destPos = BlockToWorld(x, y);
                    upBlock.Move(x, y, destPos);
                    board[(x, y)] = upBlock;
                    board[(x + dX, y + dY)] = null;
                    isAddEmpty = true;
                }
                else
                {
                    var neighbor = GetNeighbor(x, y);
                    if (neighbor != null)
                    {
                        var nighborPosX = neighbor.x;
                        var nighborPosY = neighbor.y;
                        var destPos = BlockToWorld(x, y);
                        neighbor.Move(x, y, destPos);
                        board[(x, y)] = neighbor;
                        board[(nighborPosX, nighborPosY)] = null;
                        isAddEmpty = true;
                    }
                }
            }

            if (isAddEmpty)
            {
                SpawnBlock();
                yield return new WaitForSeconds(Define.BlockMoveTime);
            }
            else
            {
                emptyBlocks = board.Where(_ => _.Value == null).OrderBy(_ => _.Key.Item1).ThenByDescending(_ => _.Key.Item2).ToList();
                if (emptyBlocks.Count > 0)
                {
                    SpawnBlock();
                    yield return new WaitForSeconds(Define.BlockMoveTime);
                }
                else
                {
                    break;
                }
            }

            emptyBlocks = board.Where(_ => _.Value == null).OrderBy(_ => _.Key.Item1).ThenByDescending(_ => _.Key.Item2).ToList();
        }

        ChainReaction();
    }

    // 빈 블록의 이웃 블록 중에서 굴러올 수 있는 블록 반환
    public Block GetNeighbor(int x, int y)
    {
        var (upX, upY) = Define.Directions[(int)Dir.Up];

        var (dx, dy) = Define.Directions[(int)Dir.LeftUp];
        // 왼쪽 위 블록 위에 블록이 없으면 굴러오도록 함.
        if (!IsEmptyBlock(x + dx, y + dy))
        {
            if (IsEmptyBlock(x + dx + upX, y + dy + upY))
            {
                return board[(x + dx, y + dy)];
            }
        }

        (dx, dy) = Define.Directions[(int)Dir.RightUp];
        // 오른쪽 위도 체크
        if (!IsEmptyBlock(x + dx, y + dy))
        {
            if (IsEmptyBlock(x + dx + upX, y + dy + upY))
            {
                return board[(x + dx, y + dy)];
            }
        }

        return null;
    }

    // 새 블록 스폰
    public void SpawnBlock()
    {
        var (x, y) = spwanPoint;

        if (board[(x, y - 1)] != null)
        {
            //Debug.Log("Spawn Block Failed!");
            return;
        }

        var type = GetRandomBlockType();
        var block = GenerateBlock(x, y, type);

        var destPos = BlockToWorld(x, y - 1);
        block.Move(x, y - 1, destPos);

        board[(x, y - 1)] = block;
    }

    public void ChainReaction()
    {
        var matches = FindMatches();
        if (matches.Count > 0)
        {
            Debug.Log($"Chain Reaction Match !! {matches.Count}");
            StartCoroutine(ClearBlock(matches));
        }
        else
        {
            var matchCount = FindMatchBlock().Count;
            bool isShuffled = false;
            while (matchCount == 0)
            {
                Shuffle();
                matchCount = FindMatchBlock().Count;
                isShuffled = true;
            }

            isMoving = false;
            OnEndSwap?.Invoke(isShuffled);
        }
    }

    private void Shuffle()
    {
        var blocks = board.Values.Where(_ => _ != null && _.type != BlockType.TopSpin).ToList();
        var types = blocks.Select(_ => _.type).ToList();

        // 블록 타입 섞기
        var rand = new System.Random();
        types = types.OrderBy(_ => rand.Next()).ToList();

        // 섞인 타입으로 블록 변경
        for (var i = 0; i < blocks.Count; i++)
        {
            blocks[i].ChangeBlockColor(types[i]);
        }
    }


    #region Hint
    public List<Block> FindHint()
    {
        return FindMatchBlock();
    }

    private List<Block> FindMatchBlock()
    {
        List<Block> result = new List<Block>();

        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                if (!board.ContainsKey((x, y)))
                    continue;

                var currentBlock = board[(x, y)];
                if (currentBlock.type == BlockType.TopSpin)
                    continue;

                foreach (var (dx, dy) in Define.Directions)
                {
                    int nX = currentBlock.x + dx;
                    int nY = currentBlock.y + dy;

                    if (!board.ContainsKey((nX, nY)))
                        continue;

                    var neighborBlock = board[(nX, nY)];
                    if (neighborBlock.type == currentBlock.type)
                        continue;

                    // 임시로 데이터만 스왑
                    TempSwap(currentBlock, neighborBlock);

                    // 스왑 후 주변에 매치가 생겼는지 검사
                    var matches = FindMatch(currentBlock);
                    if (matches.Count > 0)
                    {
                        result.AddRange(matches);
                        result.Add(neighborBlock);
                        // 임시로 스왑 한 블록 복구
                        TempSwap(currentBlock, neighborBlock);

                        return result;
                    }
                    // 임시로 스왑 한 블록 복구
                    TempSwap(currentBlock, neighborBlock);
                }
            }
        }

        return result;
    }

    // 임시로 블록 스왑
    private void TempSwap(Block blockA, Block blockB)
    {
        var posA = (blockA.x, blockA.y);
        var posB = (blockB.x, blockB.y);

        blockA.x = posB.x;
        blockA.y = posB.y;

        blockB.x = posA.x;
        blockB.y = posA.y;

        board[(posA.x, posA.y)] = blockB;
        board[(posB.x, posB.y)] = blockA;
    }
    #endregion
}
