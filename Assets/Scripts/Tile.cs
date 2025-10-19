using System;
using UnityEngine;
using System.Collections;

using static Define;

[Serializable]
public class Tile : MonoBehaviour
{
    public Point point;
    public TileType type;
    
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private GameObject objImage = null;
    [SerializeField]
    private GameObject objCrush = null;

    private bool isSelectTile = false;
    private Coroutine coMove = null;

    public Tile(Point point, TileType type)
    {
        this.point = point;
        this.type = type;

        objCrush.SetActive(false);
    }

    public void Set(Point point, TileType type)
    {
        this.point = point;
        this.type = type;

        spriteRenderer.sprite = SpriteManager.Instance.Get(type.GetImage());
        objImage.SetActive(true);
        //isSelectTile = false;
    }

    public void ChangeTile()
    {
        this.spriteRenderer.sprite = SpriteManager.Instance.Get(type.GetImage());
    }

    public void ClearTile()
    {
        objImage.SetActive(false);
        objCrush.SetActive(true);

        StartCoroutine(EndCrush());
    }

    private IEnumerator EndCrush()
    {
        yield return new WaitForSeconds(0.4f);

        GameObject.Destroy(this.gameObject);
    }

    public void Move(Point point, Vector3 dest)
    {
        if (coMove != null)
        {
            StopCoroutine(coMove);
            coMove = null;
        }

        this.point = point;
        coMove = StartCoroutine(MoveCoroutine(dest, 0.1f));
    }

    private IEnumerator MoveCoroutine(Vector3 dest, float time)
    {
        Vector3 startPos = transform.position;

        float elapsedTime = 0;
        while (elapsedTime <= time)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, dest, elapsedTime / time);
            yield return null;
        }

        transform.position = dest;
        coMove = null;
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