using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static Define;

public class Board : MonoBehaviour
{
    [Header("배경 타일")]
    [SerializeField] private GameObject goBackgroudTile = null;
    [SerializeField] private Transform trBackround = null;

    [Header("색 타일")]
    [SerializeField] private GameObject goTile = null;
    [SerializeField] private Transform trTile = null;

    [Header("레벨 데이터")]
    [SerializeField] private LevelData levelData = null;

    private float tileSize = 1f;

    private Dictionary<(int, int), Tile> board = new Dictionary<(int, int), Tile>();

    private int width = 7;
    private int height = 6;
    private (int, int) spwanPoint = (3, 6);

    public bool IsMoving => isMoving;
    private bool isMoving = false;

    public void OnGenerateBoard()
    {
        board.Clear();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (InRhombus(x, y) == false)
                    continue;

                Vector3 pos = TileToWorld(x, y);
                var backTile = Instantiate(goBackgroudTile, pos, Quaternion.identity, trBackround); // 배경타일 생성
                backTile.name = $"BackTile_{x}_{y}";
                board[(x, y)] = null;
            }
        }

        GenerateBoardWithoutMatches();
    }

    private Vector3 TileToWorld(int x, int y)
    {
        var newX = (1.5f * x);
        var newY = (Math.Sqrt(3) / 2 * x + Math.Sqrt(3) * y);
        newX = newX * (tileSize / 2f);
        newY = newY * ((tileSize * 0.93f) / Math.Sqrt(3));
        return new Vector3(newX, (float)newY) + new Vector3(-2.3f, -3f);
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
            TileType type;

            int safety = 0;

            do
            {
                type = GetRandomTileType();

                safety++;
                if (safety > 100)
                    break;
            }
            while (IsNeighborMatch(x, y, type));

            Vector3 pos = TileToWorld(x, y);
            var tileObject = Instantiate(goTile, pos, Quaternion.identity, trTile);
            tileObject.name = $"Tile_{x}_{y}";

            var tile = tileObject.GetComponent<Tile>();
            tile.objTile = tileObject;
            tile.Set(x, y, type);

            board[(x, y)] = tile;
        }
    }

    private TileType GetRandomTileType()
    {
        return (TileType) UnityEngine.Random.Range(0, (int)TileType.Max);
    }

    private bool IsNeighborMatch(int x, int y, TileType type)
    {
        // 아래 2개의 타일에서 같은 색의 타일이 있는지 체크
        var (dX, dY) = Define.Directions[(int)Dir.Down];
        if (!IsEmptyTile(x + dX * 2, y + dY * 2))
        {
            if (IsSameTile(x + dX, y + dY, type) && IsSameTile(x + dX * 2, y + dY * 2, type))
                return true;
        }

        var (ldX, ldY) = Define.Directions[(int)Dir.LeftDown];
        if (!IsEmptyTile(x + ldX * 2, y + ldY * 2))
        {
            if (IsSameTile(x + ldX, y + ldY, type) && IsSameTile(x + ldX * 2, y + ldY * 2, type))
                return true;
        }

        var (rdX, rdY) = Define.Directions[(int)Dir.RightDown];
        if (!IsEmptyTile(x + rdX * 2, y + rdY * 2))
        {
            if (IsSameTile(x + rdX, y + rdY, type) && IsSameTile(x + rdX * 2, y + rdY * 2, type))
                return true;
        }

        // 4개 만들어지는지 체크
        if (IsSameTile(x + dX, y + dY, type) && IsSameTile(x + ldX, y + ldY, type) && IsSameTile(x + rdX, y + rdY, type))
            return true;

        return false;
    }

    private bool IsSameTile(int x, int y, TileType type)
    {
        if (IsEmptyTile(x, y))
            return false;

        return board[(x, y)].type == type;
    }

    private bool IsEmptyTile(int x, int y)
    {
        if (board.TryGetValue((x, y), out Tile tile) && tile != null)
        {
            return false;
        }

        return true;
    }

    // 타일 스왑
    public void SwapTile(Tile tileA, Tile tileB, int dx, int dy)
    {
        isMoving = true;

        StartCoroutine(SwapTileCoroutine(tileA, tileB, dx, dy));
    }

    // 타일 스왑 코루틴
    IEnumerator SwapTileCoroutine(Tile tileA, Tile tileB, int dx, int dy)
    {
        var posA = (tileA.x, tileA.y);
        var posB = (tileB.x, tileB.y);
        var destA = TileToWorld(tileB.x, tileB.y);
        var destB = TileToWorld(tileA.x, tileA.y);

        tileA.Move(posB.x, posB.y, destA);
        tileB.Move(posA.x, posA.y, destB);
        board[(posA.x, posA.y)] = tileB;
        board[(posB.x, posB.y)] = tileA;

        yield return new WaitForSeconds(0.2f);

        var matches = FindMatches();
        if (matches.Count > 0)
        {
            Debug.Log($"Match !! {matches.Count}");
            //RemoveTiles(matches);
            //RefillTile(matches);
            StartCoroutine(ClearTile(matches));
        }
        else
        {
            Debug.Log($"Not Match !!");
            // 매치되지 않으니 타일 다시 되돌림.
            tileA.Move(posA.x, posA.y, destB);
            tileB.Move(posB.x, posB.y, destA);
            board[(posA.x, posA.y)] = tileA;
            board[(posB.x, posB.y)] = tileB;

            yield return new WaitForSeconds(0.2f);
            isMoving = false;
        }
    }

    // 매치된 타일 찾기
    private List<Tile> FindMatches()
    {
        var matched = new HashSet<Tile>();
        var dirs = new (int, int)[] { Directions[(int)Dir.Up], Directions[(int)Dir.LeftUp], Directions[(int)Dir.RightUp] };

        foreach (var position in board)
        {
            var tile = position.Value;

            var match4 = new List<Tile>() { tile };

            // 3개 이상 라인 체크
            foreach (var dir in dirs)
            {
                var line = new List<Tile>() { tile };
                // Up 방향
                int dx = tile.x + dir.Item1;
                int dy = tile.y + dir.Item2;

                if (IsSameTile(dx, dy, tile.type)) // 라인 체크하면서 dx, dy 값이 변경되어 4매치 먼저 체크
                    match4.Add(board[(dx, dy)]);

                while (board.TryGetValue((dx, dy), out var dTile) && dTile.type == tile.type)
                { 
                    line.Add(dTile);
                    dx += dir.Item1;
                    dy += dir.Item2;
                }

                // Down 방향
                dx = tile.x - dir.Item1;
                dy = tile.y - dir.Item2;

                if (IsSameTile(dx, dy, tile.type))
                    match4.Add(board[(dx, dy)]);

                while (board.TryGetValue((dx, dy), out var dTile) && dTile.type == tile.type)
                {
                    line.Add(dTile);
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
        }

        return matched.ToList();
    }

    // 매치된 타일 제거
    IEnumerator ClearTile(List<Tile> matches)
    {
        foreach (var match in matches)
        {
            match.ClearTile();
            board[(match.x, match.y)] = null;
        }

        yield return new WaitForSeconds(0.5f);

        foreach (var match in matches)
        {
            GameObject.DestroyImmediate(match.objTile);
        }

        StartCoroutine(FillTile());
    }

    // 빈 타일 채우기
    IEnumerator FillTile()
    {
        var emptyTiles = board.Where(_ => _.Value == null).OrderBy(_ => _.Key.Item1).ThenByDescending(_ => _.Key.Item2).ToList();

        bool isAddEmpty = false;

        while (emptyTiles.Count > 0)
        {
            isAddEmpty = false;
            foreach (var emptyTile in emptyTiles)
            {
                var (x, y) = emptyTile.Key;
                var (dX, dY) = Define.Directions[(int)Dir.Up];
                if (board.TryGetValue((x + dX, y + dY), out var upTile) && upTile != null)
                {
                    var destPos = TileToWorld(x, y);
                    upTile.Move(x, y, destPos);
                    board[(x, y)] = upTile;
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
                        var destPos = TileToWorld(x, y);
                        neighbor.Move(x, y, destPos);
                        board[(x, y)] = neighbor;
                        board[(nighborPosX, nighborPosY)] = null;
                        isAddEmpty = true;
                    }
                }
            }

            if (isAddEmpty)
            {
                SpawnTile();
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                emptyTiles = board.Where(_ => _.Value == null).OrderBy(_ => _.Key.Item1).ThenByDescending(_ => _.Key.Item2).ToList();
                if (emptyTiles.Count > 0)
                {
                    SpawnTile();
                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    break;
                }
            }

            emptyTiles = board.Where(_ => _.Value == null).OrderBy(_ => _.Key.Item1).ThenByDescending(_ => _.Key.Item2).ToList();
        }

        ChainReaction();
    }

    // 빈 타일의 이웃 타일 중에서 굴러올 수 있는 타일 반환
    public Tile GetNeighbor(int x, int y)
    {
        var (upX, upY) = Define.Directions[(int)Dir.Up];

        var (dx, dy) = Define.Directions[(int)Dir.LeftUp];
        // 왼쪽 위에 타일 위에 타일이 없으면 굴러오도록 함.
        if (!IsEmptyTile(x + dx, y + dy))
        {
            if (IsEmptyTile(x + dx + upX, y + dy + upY))
            {
                return board[(x + dx, y + dy)];
            }
        }

        (dx, dy) = Define.Directions[(int)Dir.RightUp];
        // 오른쪽 위도 체크
        if (!IsEmptyTile(x + dx, y + dy))
        {
            if (IsEmptyTile(x + dx + upX, y + dy + upY))
            {
                return board[(x + dx, y + dy)];
            }
        }

        return null;
    }

    // 새 타일 스폰
    public void SpawnTile()
    {
        var (x, y) = spwanPoint;

        if (board[(x, y - 1)] != null)
        {
            Debug.Log("Spawn Tile Failed!");
            return;
        }

        var type = GetRandomTileType();
        Vector3 pos = TileToWorld(x, y);
        var tileObject = Instantiate(goTile, pos, Quaternion.identity, trTile);
        tileObject.name = $"Tile_{x}_{y}";
        var tile = tileObject.GetComponent<Tile>();
        tile.Set(x, y, type);
        tile.objTile = tileObject;

        var destPos = TileToWorld(x, y - 1);
        tile.Move(x, y - 1, destPos);
        board[(x, y - 1)] = tile;
    }

    public void ChainReaction()
    {
        var matches = FindMatches();
        if (matches.Count > 0)
        {
            Debug.Log($"Chain Reaction Match !! {matches.Count}");
            StartCoroutine(ClearTile(matches));
        }
        else
        {
            isMoving = false;
        }
    }
}
