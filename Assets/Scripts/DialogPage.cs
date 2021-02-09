using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogPage : MonoBehaviour
{
    public Image image;
    public TextMeshProUGUI text;

    private void Update()
    {
        image.gameObject.SetActive(image.sprite != null);
    }
}
