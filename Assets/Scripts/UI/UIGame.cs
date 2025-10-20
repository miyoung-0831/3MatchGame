using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIGame : MonoBehaviour
{
    [SerializeField] private Text textCount;
    [SerializeField] private Text textTopSpin;
    [SerializeField] private Text textScore;
    [SerializeField] private Slider sliderScore;
    [SerializeField] private GameObject objShuffle;

    private Coroutine shuffleCoroutine = null;

    void Start()
    {
        textCount.text = "0";
        textTopSpin.text = "0";
        textScore.text = "0";
        objShuffle.SetActive(false);
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

    public void ShowShuffle()
    {
        objShuffle.SetActive(true);

        if (shuffleCoroutine != null)
            StopCoroutine(shuffleCoroutine);

        shuffleCoroutine = StartCoroutine(HideShuffle());
    }

    IEnumerator HideShuffle()
    {
        yield return new WaitForSeconds(0.5f);

        objShuffle.SetActive(false);
        shuffleCoroutine = null;
    }
}
