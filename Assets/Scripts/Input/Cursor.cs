using System;
using UnityEngine;
using Zenject;

public class Cursor : MonoBehaviour
{
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float maxRayDistance = 100f;
    
    [Inject] private readonly PlayerInputHandler _playerInputHandler;
    private Camera _camera;
    
    public Action<Vector3> OnCursorMoved;

    private void OnEnable()
    {
        _camera = Camera.main;
        UnityEngine.Cursor.visible = false;
        _playerInputHandler.OnLook += Follow;
    }

    private void OnDisable()
    {
        UnityEngine.Cursor.visible = true;
        _playerInputHandler.OnLook -= Follow;
    }

    private void Follow(Vector2 screenPosition)
    {
        var ray = _camera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out var hit, maxRayDistance, groundMask))
        {
            transform.position = hit.point;
            OnCursorMoved?.Invoke(hit.point);
        }
    }
}