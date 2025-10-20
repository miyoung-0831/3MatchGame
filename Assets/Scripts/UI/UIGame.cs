using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIGame : MonoBehaviour
{
    [SerializeField] private Text textCount;
    [SerializeField] private Text textTopSpin;
    [SerializeField] private Text textScore;
    [SerializeField] Slider sliderScore;

    private int count = 0;
    private int score = 0;
    private int topSpin = 0;

    void Start()
    {
        textCount.text = count.ToString();
        textTopSpin.text = topSpin.ToString();
        textScore.text = score.ToString();
    }

    public void UpdateCount(int count)
    {
        this.count = count;
        textCount.text = count.ToString();
    }

    public void ClearBlock(List<Block> blocks)
    {
        var topSpin = blocks.Where(_ => _.type == Define.BlockType.TopSpin).Count();
        var normalBlock = blocks.Count - topSpin;

        score += normalBlock * Define.NormalBlockScore;
        score += topSpin * Define.TopSpinBlockScore;

        UpdateScore();

        if (topSpin > 0)
            UpdateTopSpin(topSpin);
    }

    public void UpdateScore()
    {
        textScore.text = String.Format("{0:#,0}", score);
        sliderScore.value = score / 10000;
    }

    public void UpdateTopSpin(int count)
    {
        this.topSpin += count;
        textTopSpin.text = topSpin.ToString();
    }
}
