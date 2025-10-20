using System;
using UnityEngine;
using System.Collections;

using static Define;

[Serializable]
public class Block : MonoBehaviour
{
    public int x;
    public int y;
    public BlockType type;
    
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField]  private GameObject objImage = null;
    [SerializeField] private GameObject objCrush = null;
    [SerializeField] private Animator topSpinAni = null;
    [SerializeField] private Material particleMat;

    private float elapsedTime = 0;
    private float movingTime = 0.2f;

    public GameObject BlockObject => objBlock;
    private GameObject objBlock = null;

    private bool isSelectBlock = false;

    private Coroutine coMove = null;
    private bool isMoving = false;

    public bool IsLock => isLock;
    private bool isLock = false;

    public Block(int x, int y, BlockType type)
    {
        this.x = x;
        this.y = y;
        this.type = type;

        objCrush.SetActive(false);
    }

    public void Set(int x, int y, BlockType type, GameObject obj)
    {
        this.x = x;
        this.y = y;
        this.type = type;
        this.objBlock = obj;

        spriteRenderer.sprite = ResourceManager.Instance.GetSprite(type.GetImage());
        objImage.SetActive(true);

        if (type == BlockType.TopSpin)
            isLock = true;
        else
            isLock = false;

        //isSelectBlock = false;
    }

    private void OnDisable()
    {
        ResetBlock();
    }

    // 블럭 재사용을 위한 초기화
    private void ResetBlock()
    {
        isLock = false;
        elapsedTime = 0;
        movingTime = 0.2f;
        isMoving = false;
        objBlock = null;

        objImage.SetActive(true);
        objCrush.SetActive(false);

        topSpinAni.enabled = false;
    }

    public void ClearBlock()
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
        if (type != BlockType.TopSpin)
            return;

        isLock = false;
        topSpinAni.enabled = true;
    }


    private void ChangeParticleTexture()
    {
        particleMat.mainTexture = ResourceManager.Instance.GetTexture(type.GetParticleTexture());
    }

    private void OnMouseDown()
    {
        //if (this.type == BlockType.None)
        //    return;

        isSelectBlock = true;
        GameManager.Instance.SelectBlock(this);
    }

    private void OnMouseUp()
    {
        isSelectBlock = false;
        GameManager.Instance.SelectBlock(null);
    }

    private void OnMouseEnter()
    {
        if (GameManager.Instance.IsSelectedBlock)
            GameManager.Instance.SwapBlock(this);
    }

    //private void OnMouseExit()
    //{
    //    if (isSelectBlock)
    //        GameManager.Instance.SwapBlock(this);
    //}
}