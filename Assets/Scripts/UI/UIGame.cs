using System;
using UnityEngine;
using UnityEngine.UI;

public class UIGame : MonoBehaviour
{
    [SerializeField] private Text textCount;
    [SerializeField] private Text textTopSpin;
    [SerializeField] private Text textScore;
    [SerializeField] Slider sliderScore;

    void Start()
    {
        textCount.text = "0";
        textTopSpin.text = "0";
        textScore.text = "0";
    }

    public void UpdateCount(int count)
    {
        textCount.text = count.ToString();
    }

    public void UpdateScore(int score)
    {
        textScore.text = String.Format("{0:#,0}", score);
        sliderScore.value = score / Define.MaxScore;
    }

    public void UpdateTopSpin(int count)
    {
        textTopSpin.text = count.ToString();
    }
}
