using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogWindow : MonoBehaviour
{
    
    // Serialized Properties
    public Button acceptButton;
    public Button nextButton;
    public Button previousButton;
    public Button tapAnywhereButton;
    public DialogPage pagePrefab;
    public float buttonPulseSpeed = 1.0f;
    public float buttonPulseScaleAmount = 1.15f;
    public GameObject pageContainer;
    public GameObject pageContainerNoImage;
    public GameObject portraitContainer;
    public Image portrait;
    public TextMeshProUGUI title;

    // Public Properties
    public bool AllowTapAnywhere
    {
        set
        {
            if (tapAnywhereButton != null)
            {
                tapAnywhereButton.gameObject.SetActive(value);
            }
        }
    }
    
    // Private Properties
    private bool _isPulsingUp = true;
    private bool _shouldHaveAcceptButton;
    private float _acceptInitialScale = 1.0f;
    private float _buttonPulseScaleCounter = 0.0f;
    private float _nextPrevInitialScale = 1.0f;
    private int _currentIndex;

    private void Start()
    {
        _nextPrevInitialScale = nextButton.transform.localScale.x;
        _acceptInitialScale = acceptButton.transform.localScale.x;
        SoundManager.Instance.PlaySound(SoundManager.Instance.dialogStart, 1.0f);
    }

    private void Update()
    {
        if (portrait != null)
        {
            portraitContainer.gameObject.SetActive(portrait.sprite != null);
        }

        transform.SetAsLastSibling();

        var pageCount = pageContainer.transform.childCount;
        previousButton.gameObject.SetActive(_currentIndex > 0);
        nextButton.gameObject.SetActive(_currentIndex < pageCount - 1);
        acceptButton.gameObject.SetActive(_currentIndex >= pageCount - 1 && _shouldHaveAcceptButton);

        if (_isPulsingUp)
        {
            if (_buttonPulseScaleCounter < buttonPulseSpeed)
            {
                _buttonPulseScaleCounter += Time.deltaTime;
                var buttonPulseScale = Mathf.Lerp(_nextPrevInitialScale, 
                                                  _nextPrevInitialScale * buttonPulseScaleAmount, 
                                                  _buttonPulseScaleCounter / buttonPulseSpeed);
                var acceptPulseScale = Mathf.Lerp(_acceptInitialScale, 
                                                  _acceptInitialScale * buttonPulseScaleAmount,
                                                  _buttonPulseScaleCounter / buttonPulseSpeed);
                var scaleVector = new Vector3(acceptPulseScale, acceptPulseScale, acceptPulseScale);
                var negativeScaleVector = new Vector3(buttonPulseScale, buttonPulseScale, buttonPulseScale);
                nextButton.transform.localScale = negativeScaleVector;
                acceptButton.transform.localScale = scaleVector;
            }
            else
            {
                _buttonPulseScaleCounter = buttonPulseSpeed;
                _isPulsingUp = false;
            }
        }
        else
        {
            if (_buttonPulseScaleCounter > 0.0f)
            {
                _buttonPulseScaleCounter -= Time.deltaTime;
                var buttonPulseScale = Mathf.Lerp(_nextPrevInitialScale, 
                    _nextPrevInitialScale * buttonPulseScaleAmount, 
                    _buttonPulseScaleCounter / buttonPulseSpeed);
                var acceptPulseScale = Mathf.Lerp(_acceptInitialScale, 
                    _acceptInitialScale * buttonPulseScaleAmount,
                    _buttonPulseScaleCounter / buttonPulseSpeed);
                var scaleVector = new Vector3(acceptPulseScale, acceptPulseScale, acceptPulseScale);
                var negativeScaleVector = new Vector3(buttonPulseScale, buttonPulseScale, buttonPulseScale);
                nextButton.transform.localScale = negativeScaleVector;
                acceptButton.transform.localScale = scaleVector;
            }
            else
            {
                _buttonPulseScaleCounter = 0.0f;
                _isPulsingUp = true;
            }
        }
    }

    public void AcceptDialog()
    {
        SoundManager.Instance.PlaySound(SoundManager.Instance.dialogAccept, 0.5f);
        Destroy(gameObject);
    }

    public DialogPage AddPage(Sprite sprite, string text, bool isActive = false)
    {
        // TODO: Figure out how to remove left offset when no sprite present while still keeping single page container
        var newPage = Instantiate(pagePrefab, pageContainer.transform);
        newPage.image.sprite = sprite;
        newPage.text.text = text;
        newPage.image.gameObject.SetActive(sprite != null);
        newPage.gameObject.SetActive(isActive);
        return newPage;
    }

    public void LinkTerms()
    {
        Application.OpenURL("https://www.sdpp.site");
    }

    public void NextPanel()
    {
        pageContainer.transform.GetChild(_currentIndex).gameObject.SetActive(false);
        var childCount = pageContainer.transform.childCount;
        _currentIndex++;

        if (_currentIndex > childCount - 1)
        {
            nextButton.interactable = false;
            _currentIndex = childCount - 1;
        }
        
        pageContainer.transform.GetChild(_currentIndex).gameObject.SetActive(true);

        if (_currentIndex > 0)
        {
            previousButton.interactable = true;
        }

        SoundManager.Instance.PlaySound(SoundManager.Instance.pageTurn, 0.5f);
    }

    public void PreviousPanel()
    {
        pageContainer.transform.GetChild(_currentIndex).gameObject.SetActive(false);
        var childCount = pageContainer.transform.childCount;
        _currentIndex--;

        if (_currentIndex < 0)
        {
            previousButton.interactable = false;
            _currentIndex = 0;
        }

        pageContainer.transform.GetChild(_currentIndex).gameObject.SetActive(true);

        if (_currentIndex < childCount - 1)
        {
            nextButton.interactable = true;
        }
        
        SoundManager.Instance.PlaySound(SoundManager.Instance.pageTurn, 0.5f);
    }

    public void SetAcceptButtonActive(bool isActive)
    {
        _shouldHaveAcceptButton = isActive;
    }

    public void SetHeader(Sprite newPortrait, string newTitle)
    {
        portrait.sprite = newPortrait;
        title.text = newTitle;
    }

    public void TappedAnywhere()
    {
        if (nextButton.gameObject.activeSelf)
        {
            NextPanel();
        }
        else if (acceptButton.gameObject.activeSelf)
        {
            AcceptDialog();
        }
    }

}
