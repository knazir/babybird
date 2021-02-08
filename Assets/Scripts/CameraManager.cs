using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraManager : MonoBehaviour
{
    // Helper Classes
    private class CameraEvent
    {
        public Vector3 Point;
    }

    private enum RaycastTarget
    {
        Ground,
    }
    
    // Constants and Readonly Properties
    private const float PanStartThreshold = 0.05f;
    private const float MaxRaycastDistance = 1000.0f;
    private const float PinchDeltaDampening = 50.0f;
    private const float ScrollWheelCameraZoomSensitivity = 3.0f;
    private const float TapPositionMagnitudeThreshold = 1000000.0f;
    private const float VelocityDampeningTime = 1.0f;
    private readonly Vector3 _positiveInfinity = new Vector3(float.PositiveInfinity,
                                                             float.PositiveInfinity, 
                                                             float.PositiveInfinity);
    
    // Static Properties
    public static CameraManager Instance;

    // Serialized Properties
    public float maxZoomY = 20.0f;
    public float minZoomY = 3.0f;
    public CinemachineVirtualCamera movableCamera;
    public EventSystem eventSystem;
    
    // Public Properties
    public ICinemachineCamera CurrentCamera => _brain.ActiveVirtualCamera;

    // Private Properties
    private bool _canPan;
    private bool _hasTouchCountChanged;
    private bool _isLocked;
    private bool _isMouseDownOverUi;
    private bool _isPanning;
    private bool _isPanningStarted;
    private bool _isZoomingStarted;
    private Camera _attachedCamera;
    private CinemachineBrain _brain;
    private float _dampeningTimer;
    private float _previousPinchDistance;
    private int _layerMaskGroundCollider;
    private int _previousTouchCount;
    private List<RaycastResult> _cameraRaycastResults;
    private Vector2 _touchPosition;
    private Vector3 _dragOrigin;
    private Vector3 _panVelocity;
    private Vector3 _previousPanPoint;
    private Vector3 _tapGroundStartPosition;
    private Vector3 _touchPoint1;
    private Vector3 _touchPoint2;

    private void Awake()
    {
        Instance = this;
        _brain = GetComponent<CinemachineBrain>();
        _attachedCamera = GetComponent<Camera>();
        _cameraRaycastResults = new List<RaycastResult>();
        _layerMaskGroundCollider = LayerMask.GetMask("GroundCollider");
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isMouseDownOverUi = _IsUsingUI();
        }

        if (_isMouseDownOverUi)
        {
            return;
        }

        if (ReferenceEquals(CurrentCamera, movableCamera))
        {
            _UpdateScenePan();
            _UpdateSceneZoom();
        }
    }

    private void _ClampCamera()
    {
        // TODO
    }

    private bool _IsUsingUI()
    {
        if (_isPanningStarted)
        {
            return false;
        }
        var pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };
        _cameraRaycastResults.Clear();
        eventSystem.RaycastAll(pointerData, _cameraRaycastResults);
        return _cameraRaycastResults.Count > 0;
    }

    private void _OnChangeTouchCountScenePan(CameraEvent evt)
    {
        _previousPanPoint = evt.Point;
    }
    
    private void _PanScene(CameraEvent evt)
    {
        var delta = _previousPanPoint - evt.Point;
        
        // For now, panning should not change the Y value, assume all terrain is (mostly) flat
        // delta.y = 0.0f;
        
        var pos = movableCamera.transform.position;
        movableCamera.transform.position += delta;
        _ClampCamera();
        _panVelocity = movableCamera.transform.position - pos;
    }

    private void _RefreshTouchValues()
    {
        _hasTouchCountChanged = false;
        var touchCount = 0;
        var isInEditor = false;

        if (Input.touchCount == 0)
        {
            isInEditor = Input.GetMouseButtonDown(0) || Input.GetMouseButton(0);
            touchCount = isInEditor ? 1 : 0;
        }
        else
        {
            var hasEndedTouch = Input.GetTouch(0).phase == TouchPhase.Ended;
            touchCount = hasEndedTouch ? 0 : Input.touchCount;
        }

        if (touchCount != _previousTouchCount && touchCount != 0)
        {
            _hasTouchCountChanged = true;
        }

        if (isInEditor)
        {
            _touchPosition = Input.mousePosition;
        }
        else if (touchCount == 1)
        {
            _touchPosition = Input.GetTouch(0).position;
        }
        else if (touchCount >= 2)
        {
            _touchPosition = (Input.GetTouch(0).position + Input.GetTouch(1).position) / 2.0f;
        }

        _canPan = touchCount > 0 && !_isLocked;
        _previousTouchCount = touchCount;
    }

    private Vector3 _TryGetRaycastHit(Vector2 touch, RaycastTarget target)
    {
        var hit = new RaycastHit();
        var ray = _attachedCamera.ScreenPointToRay(touch);
        var didFindHit = false;
        switch (target)
        {
            case RaycastTarget.Ground:
                didFindHit = Physics.Raycast(ray, out hit, MaxRaycastDistance, _layerMaskGroundCollider);
                break;
        }
        return didFindHit ? hit.point : _positiveInfinity;
    }

    private void _UpdateOrthographicZoomFromY()
    {
        movableCamera.m_Lens.OrthographicSize = movableCamera.transform.position.y / 2.0f;
        if (ReferenceEquals(movableCamera, CurrentCamera))
        {
            _attachedCamera.orthographicSize = movableCamera.m_Lens.OrthographicSize;
        }
    }

    private void _UpdatePanInertia()
    {
        if (_isLocked)
        {
            return;
        }

        if (_panVelocity.magnitude < PanStartThreshold)
        {
            _dampeningTimer = 0.0f;
            _panVelocity = Vector3.zero;
        }

        if (_panVelocity != Vector3.zero)
        {
            _dampeningTimer += Time.deltaTime;
            _panVelocity = Vector3.Lerp(_panVelocity, Vector3.zero, _dampeningTimer / VelocityDampeningTime);
            movableCamera.transform.localPosition += _panVelocity;
            _ClampCamera();
        }
    }

    private void _UpdateScenePan()
    {
        _RefreshTouchValues();
        
        if (_hasTouchCountChanged)
        {
            _tapGroundStartPosition = _TryGetRaycastHit(_touchPosition, RaycastTarget.Ground);
        }

        if (!_canPan)
        {
            return;
        }
        
        var currentTapPosition = _TryGetRaycastHit(_touchPosition, RaycastTarget.Ground);
        if (_hasTouchCountChanged && currentTapPosition.sqrMagnitude < TapPositionMagnitudeThreshold)
        {
            var evt = new CameraEvent()
            {
                Point = currentTapPosition,
            };
            _OnChangeTouchCountScenePan(evt);
        }

        var panDistance = Vector3.Distance(_tapGroundStartPosition, currentTapPosition);
        if (!_isPanningStarted && panDistance >= PanStartThreshold)
        {
            _isPanningStarted = true;
            _previousPanPoint = currentTapPosition;
        }

        if (_isPanningStarted && currentTapPosition.sqrMagnitude < TapPositionMagnitudeThreshold)
        {
            CameraEvent evt = new CameraEvent()
            {
                Point = currentTapPosition,
            };
            _isPanning = true;
            _PanScene(evt);
        }
        else
        {
            _isPanning = false;
            if (_isPanningStarted)
            {
                _isPanningStarted = false;
            }
        }
            
        if (!_isPanning)
        {
            _UpdatePanInertia();
        }

    }

    private void _UpdateSceneZoom()
    {
        if (_isLocked)
        {
            return;
        }
        
        var cameraDelta = 0.0f;
        
        // Editor
        var scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        if (Math.Abs(scrollAmount) > 0.0f)
        {
            cameraDelta += scrollAmount * ScrollWheelCameraZoomSensitivity;
        }
        
        // Android
        if (Input.touchCount == 0)
        {
            _isZoomingStarted = false;
        }

        if (Input.touchCount == 2)
        {
            _touchPoint1 = Input.GetTouch(0).position;
            _touchPoint2 = Input.GetTouch(1).position;
            if (!_isZoomingStarted)
            {
                _isZoomingStarted = true;
                _previousPinchDistance = (_touchPoint2 - _touchPoint1).magnitude;
            }
        }

        if (_isZoomingStarted)
        {
            var currentPinchDistance = (_touchPoint2 - _touchPoint1).magnitude;
            var delta = _previousPinchDistance - currentPinchDistance;
            _previousPinchDistance = currentPinchDistance;
            cameraDelta = -delta / PinchDeltaDampening;
        }

        if (Math.Abs(cameraDelta) > 0.0f)
        {
            var cameraTransform = movableCamera.transform;
            var offset = cameraTransform.forward * cameraDelta;
            var newPos = cameraTransform.position + offset;
            var adjustY = 0.0f;
            if (newPos.y > maxZoomY)
            {
                adjustY = -offset.y + (maxZoomY - movableCamera.transform.position.y);
            }
            else if (newPos.y < minZoomY)
            {
                adjustY = offset.y + (minZoomY - movableCamera.transform.position.y);
            }

            if (Math.Abs(adjustY) > 0.0f)
            {
                cameraDelta += adjustY / Vector3.Dot(movableCamera.transform.forward, Vector3.up);
            }
            
            movableCamera.transform.Translate(Vector3.forward * cameraDelta);
        }

        _UpdateOrthographicZoomFromY();
    }
}
