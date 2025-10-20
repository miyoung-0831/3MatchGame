using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using BlockType = Define.BlockType;

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

    public GameObject BlockObject => objBlock;
    private GameObject objBlock = null;

    private Coroutine coMove = null;
    private bool isMoving = false;

    public bool IsLock => isLock;
    private bool isLock = false;

    private List<Vector3> destPositions = new List<Vector3>();
    private int moveIndex = 0; // 이동 중에 여러번 이동 명령이 들어올 때 순서대로 처리하기 위한 인덱스

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

    public void ChangeBlockColor(BlockType newType)
    {
        type = newType;
        spriteRenderer.sprite = ResourceManager.Instance.GetSprite(type.GetImage());
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
            moveIndex = 0;
        }

        if (destPositions == null)
            destPositions = new List<Vector3>();
        destPositions.Add(dest);

        coMove = StartCoroutine(MoveCoroutine());
    }

    private IEnumerator MoveCoroutine()
    {
        isMoving = true;
        Vector3 startPos = transform.position;

        while (elapsedTime < Define.BlockMoveTime && moveIndex < destPositions.Count)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, destPositions[moveIndex], elapsedTime / Define.BlockMoveTime);

            if (Vector3.Distance(this.transform.position, destPositions[moveIndex]) < 0.01f && moveIndex < destPositions.Count - 1)
            {
                moveIndex++;
                elapsedTime = 0;
                startPos = this.transform.position;
            }

            yield return null;
        }

        transform.position = destPositions.Last();
        coMove = null;
        isMoving = false;
        destPositions.Clear();
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
        GameManager.Instance.SelectBlock(this);
    }

    private void OnMouseUp()
    {
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