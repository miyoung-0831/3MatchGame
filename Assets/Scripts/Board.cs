using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

using static Define;
using static LevelData;
using static UnityEditor.IMGUI.Controls.PrimitiveBoundsHandle;

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

    public void OnGenerateBoard()
    {
        board.Clear();

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
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (InRhombus(x, y) == false)
                    continue;

                TileType type;
                int safety = 0;
                do
                {
                    Vector3 pos = TileToWorld(x, y);
                    
                    Instantiate(goBackgroudTile, pos, Quaternion.identity, trBackround); // 배경타일 생성
                    var tileObject = Instantiate(goTile, pos, Quaternion.identity, trTile);
                    var tile = tileObject.GetComponent<Tile>();
                    type = GetRandomTileType();
                    tile.Set(new Point(x, y), type);
                    board[(x, y)] = tile;
                    
                    safety++;
                    // 너무 많은 반복 방지 (이론상 거의 안 일어남)
                    if (safety > 100)
                        break;
                }
                while (IsNeighborMatch(x, y, type));
            }
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

    public void SwapTile(Tile tileA, Tile tileB, int dx, int dy)
    {
        StartCoroutine(SwapTileCoroutine(tileA, tileB, dx, dy));
    }

    IEnumerator SwapTileCoroutine(Tile tileA, Tile tileB, int dx, int dy)
    {
        var posA = tileA.point;
        var posB = tileB.point;
        var destA = TileToWorld(tileB.point.x, tileB.point.y);
        var destB = TileToWorld(tileA.point.x, tileA.point.y);

        tileA.Move(posB, destA);
        tileB.Move(posA, destB);
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
            tileA.Move(posA, destB);
            tileB.Move(posB, destA);
            board[(posA.x, posA.y)] = tileA;
            board[(posB.x, posB.y)] = tileB;
        }
    }

    private List<Tile> FindMatches()
    {
        var matched = new HashSet<Tile>();
        var dirs = new (int, int)[] { Directions[(int)Dir.Up], Directions[(int)Dir.LeftUp], Directions[(int)Dir.RightUp] };

        foreach (var position in board)
        {
            var tile = position.Value;
            // 3개 이상 라인 체크
            foreach (var dir in dirs)
            {
                var line = new List<Tile>() { tile };
                // Up 방향
                int dx = tile.point.x + dir.Item1;
                int dy = tile.point.y + dir.Item2;
                while (board.TryGetValue((dx, dy), out var dTile) && dTile.type == tile.type)
                { 
                    line.Add(dTile);
                    dx += dir.Item1;
                    dy += dir.Item2;
                }

                // Down 방향
                dx = tile.point.x - dir.Item1;
                dy = tile.point.y - dir.Item2;
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

            var match4 = new List<Tile>() { tile };
            foreach ((int dx, int dy) in Directions4)
            {
                if (IsSameTile(tile.point.x + dx, tile.point.y + dy, tile.type))
                    match4.Add(tile);
            }

            if (match4.Count >= 4)
            {
                foreach (var c in match4)
                    matched.Add(c);
            }
        }

        return matched.ToList();
    }

    IEnumerator ClearTile(List<Tile> matches)
    {
        foreach (var match in matches)
        {
            match.ClearTile();
            board[(match.point.x, match.point.y)] = null;
        }

        yield return new WaitForSeconds(0.5f);

        FillTile();
    }

    public void FillTile()
    {
        var emptyTiles = board.Where(_ => _.Value == null).ToList();
        var dropTiles = board.Where(_ => emptyTiles.FindIndex(t => t.Key.Item1 == _.Key.Item1 && t.Key.Item2 != _.Key.Item2 && _.Value != null) > -1).ToList();

        foreach (var tile in dropTiles)
        {
            Debug.Log($"drop {tile.Key} / {tile.Value.type}");
        }
    }
}
