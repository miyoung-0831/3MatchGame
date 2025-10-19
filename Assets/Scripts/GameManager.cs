using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEditor.Overlays;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] Board board = null;

    public bool IsSelectedTile => isSelectedTile;
    private bool isSelectedTile = false;
    private Tile selectTile = null;
    private Vector3 downPosition = Vector3.zero;

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
        isSelectedTile = tile != null;
        selectTile = tile;

        if (tile != null)
        {
            downPosition = Input.mousePosition;

            //Debug.Log($"Select Tile : {tile.x}, {tile.y}, {tile.type}");
        }
    }

    public void SwapTile(Tile tile)
    {
        if (selectTile != tile)
        {
            //var dir = new Vector2(selectTile.point.x - tile.point.x, selectTile.point.y - tile.point.y);
            var dir = new Vector2(tile.point.x - selectTile.point.x, tile.point.y - selectTile.point.y);
            dir.Normalize();

            Debug.Log($"{selectTile.type} <=> {tile.type} / dir {dir} / {Mathf.RoundToInt(dir.x)} , {Mathf.RoundToInt(dir.y)}");
            board.SwapTile(selectTile, tile, Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y));

            isSelectedTile = false;
        }
    }
}