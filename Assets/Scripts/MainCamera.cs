using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainCamera : MonoBehaviour
{
    public static MainCamera Instance;

    public float dragSpeed;

    private Vector3 _dragOrigin;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            _dragOrigin = Input.mousePosition;
        }

        if (!Input.GetMouseButton(0) || Camera.main == null)
        {
            return;
        }

        var pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - _dragOrigin);
        var move = new Vector3(pos.x * dragSpeed, 0.0f, pos.y * dragSpeed);
        
        // Negate move to pan in opposite direction from drag
        transform.Translate(-move, Space.World);
#else
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            var touchTranslation = Input.GetTouch(0).deltaPosition * -panSpeed;
            transform.Translate(touchTranslation.x, touchTranslation.y, 0.0f);
        }
#endif
    }
}
