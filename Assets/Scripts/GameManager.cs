using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] Board board = null;

    public bool IsSelectedTile => isSelectedTile;
    private bool isSelectedTile = false;
    private Tile selectTile = null;

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
        isSelectedTile = false;
    }

    private void Setup()
    {
        board.OnGenerateBoard();
    }

    public void SelectTile(Tile tile)
    {
        if (board.IsMoving)
            return;

        isSelectedTile = tile != null;
        selectTile = tile;
    }

    public void SwapTile(Tile tile)
    {
        if (board.IsMoving)
            return;

        if (selectTile != tile)
        {
            var dir = new Vector2(tile.x - selectTile.x, tile.y - selectTile.y);
            dir.Normalize();

            Debug.Log($"{selectTile.type} <=> {tile.type} / dir {dir} / {Mathf.RoundToInt(dir.x)} , {Mathf.RoundToInt(dir.y)}");
            board.SwapTile(selectTile, tile, Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y));

            isSelectedTile = false;
        }
    }
}