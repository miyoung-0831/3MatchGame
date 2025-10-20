using UnityEngine;

public class BackgroundTile : MonoBehaviour
{
    [SerializeField] GameObject objHint = null;

    public void SetHintActive(bool isActive)
    {
        if (objHint != null)
            objHint.SetActive(isActive);
    }
}
