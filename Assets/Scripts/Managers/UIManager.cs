using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Static Properties
    public static UIManager Instance;
    
    // Serialized Properties
    public DialogWindow dialogWindowPrefab;
    public GameObject dialogContainer;
    public Sprite testSprite;

    private void Awake()
    {
        Instance = this;
    }

    public void ShowTestDialogWindow(string title, Sprite sprite)
    {
        var dialogWindow = Instantiate(dialogWindowPrefab, dialogContainer.transform);
        dialogWindow.SetHeader(sprite, title);
        for (var i = 0; i < 3; i++)
        {
            var text = $"This is test page {i + 1}. Here is some extra text to pad the length and see how it looks.";
            var pageSprite = i % 2 == 1 ? testSprite : null;
            dialogWindow.AddPage(pageSprite, text, i == 0);
        }
        dialogWindow.SetAcceptButtonActive(true);
    }
}
