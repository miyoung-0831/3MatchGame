using System;
using UnityEngine;
using System.Collections;

using static Define;

[Serializable]
public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public TileType type;
    
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private GameObject objImage = null;
    [SerializeField]
    private GameObject objCrush = null;

    public GameObject objTile = null;

    private bool isSelectTile = false;
    private Coroutine coMove = null;
    private bool isMoving = false;

    public Tile(int x, int y, TileType type)
    {
        this.x = x;
        this.y = y;
        this.type = type;

        objCrush.SetActive(false);
    }

    public void Set(int x, int y, TileType type)
    {
        this.x = x;
        this.y = y;
        this.type = type;

        spriteRenderer.sprite = SpriteManager.Instance.Get(type.GetImage());
        objImage.SetActive(true);
        //isSelectTile = false;
    }

    public void ClearTile()
    {
        objImage.SetActive(false);
        objCrush.SetActive(true);

        //StartCoroutine(EndCrush());
    }

    //private IEnumerator EndCrush()
    //{
    //    yield return new WaitForSeconds(0.4f);

    //    GameObject.Destroy(this.gameObject);
    //}

    public void Move(int x, int y, Vector3 dest)
    {
        if (coMove != null)
        {
            StopCoroutine(coMove);
            coMove = null;
        }

        this.x = x;
        this.y = y;

        if (!isMoving)
        {
            elapsedTime = 0;
            movingTime = 0.2f;
        }
        else
            movingTime += 0.2f;

        coMove = StartCoroutine(MoveCoroutine(dest, movingTime));
    }

    private float elapsedTime = 0;
    private float movingTime = 0.2f;

    private IEnumerator MoveCoroutine(Vector3 dest, float time)
    {
        isMoving = true;
        Vector3 startPos = transform.position;

        while (elapsedTime <= time)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, dest, elapsedTime / time);
            yield return null;
        }

        transform.position = dest;
        coMove = null;
        isMoving = false;
    }

    private void OnMouseDown()
    {
        //if (this.type == TileType.None)
        //    return;

        isSelectTile = true;
        GameManager.Instance.SelectTile(this);
    }

    private void OnMouseUp()
    {
        isSelectTile = false;
        GameManager.Instance.SelectTile(null);
    }

    private void OnMouseEnter()
    {
        if (GameManager.Instance.IsSelectedTile)
            GameManager.Instance.SwapTile(this);
    }

    //private void OnMouseExit()
    //{
    //    if (isSelectTile)
    //        GameManager.Instance.SwapTile(this);
    //}
}