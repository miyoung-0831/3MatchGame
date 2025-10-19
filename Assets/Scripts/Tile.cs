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
    [SerializeField]
    private Animator topSpinAni = null;

    public GameObject TileObject => objTile;
    private GameObject objTile = null;

    private bool isSelectTile = false;

    private Coroutine coMove = null;
    private bool isMoving = false;

    public bool IsLock => isLock;
    private bool isLock = false;

    public Tile(int x, int y, TileType type)
    {
        this.x = x;
        this.y = y;
        this.type = type;

        objCrush.SetActive(false);
    }

    public void Set(int x, int y, TileType type, GameObject obj)
    {
        this.x = x;
        this.y = y;
        this.type = type;
        this.objTile = obj;

        spriteRenderer.sprite = SpriteManager.Instance.GetSprite(type.GetImage());
        objImage.SetActive(true);

        if (type == TileType.TopSpin)
            isLock = true;
        else
            isLock = false;

        //isSelectTile = false;
    }

    public void ClearTile()
    {
        ChangeParticleTexture();

        objImage.SetActive(false);
        objCrush.SetActive(true);
    }

    // 목적지까지 이동
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
            movingTime += 0.2f; // 우선 이동중이면 시간 누적

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

    // 팽이 잠금 해제 (팽이 돌아가는 애니메이션 재생)
    public void UnlockTopSpin()
    {
        if (type != TileType.TopSpin)
            return;

        isLock = false;
        topSpinAni.enabled = true;
    }

    [SerializeField] private Material particleMat;

    private void ChangeParticleTexture()
    {
        particleMat.mainTexture = SpriteManager.Instance.GetTexture(type.GetParticleTexture());
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