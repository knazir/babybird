﻿using System;
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
    }
    
    private void Update()
    {
        
    }

    public void AnimateWakeUp()
    {
        
    }
}
