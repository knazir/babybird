using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldCharacter : MonoBehaviour
{
    // Static Properties
    public static OverworldCharacter MainCharacter;
    
    // Serialized Properties
    public bool isMainCharacter;

    // Private Properties
    private HumanoidAnimator _humanoidAnimator;

    private void Awake()
    {
        if (isMainCharacter)
        {
            if (MainCharacter != null)
            {
                throw new Exception("Multiple main characters detected");
            }
            MainCharacter = this;
        }
        _humanoidAnimator = GetComponent<HumanoidAnimator>();

        StartCoroutine(_Test());
    }
    
    private void Update()
    {
        
    }

    private IEnumerator _Test()
    {
        yield return new WaitForSeconds(2.0f);
        var sprite = UIManager.Instance.testSprite;
        UIManager.Instance.ShowTestDialogWindow("Test Dialog", sprite);
    }

    public void AnimateWakeUp()
    {
        
    }
}
