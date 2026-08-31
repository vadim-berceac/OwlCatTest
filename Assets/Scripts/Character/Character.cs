using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class Character : MonoBehaviour
{
    [field: SerializeField] public CharacterType CharacterType { get; private set; } = CharacterType.AI;
    
    [Header("Player only Settings")]
    [SerializeField] private float rotationToCursorSpeed = 5f;
    [SerializeField] private float minRotationDistance = 1f;
    [SerializeField] private float closeRotationDistance = 2f;
    [SerializeField] private float maxCloseRotationAngle = 45f;

    [Inject] private readonly ICharacterInput _currentInput;
    [Inject] private readonly Cursor _cursor;
    [Inject] private readonly Transform _transform;
    [Inject] private readonly Animator _animator;
    [Inject] private readonly AnimationStates _animationStates;

    private Quaternion _targetRotation;
    private CancellationTokenSource _rotationCts;
    private float _lastRotationDirection;
    
    public event Action<float> OnRotationDirection;

    private void OnEnable()
    {
        if (CharacterType != CharacterType.Player)
        {
            return;
        }

        _targetRotation = _transform.rotation;
        _cursor.OnCursorMoved += RotatePlayerToCursor;
        StartRotationLoop();
    }

    private void OnDisable()
    {
        if (CharacterType != CharacterType.Player)
        {
            return;
        }
        
        _cursor.OnCursorMoved -= RotatePlayerToCursor;
        StopRotationLoop();
    }

    private void StartRotationLoop()
    {
        _rotationCts = new CancellationTokenSource();
        RotationLoopAsync(_rotationCts.Token).Forget();
    }

    private void StopRotationLoop()
    {
        _rotationCts?.Cancel();
        _rotationCts?.Dispose();
        _rotationCts = null;
    }

    private async UniTaskVoid RotationLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var previousRotation = _transform.rotation;
            _transform.rotation = Quaternion.Slerp(_transform.rotation, _targetRotation, rotationToCursorSpeed * Time.deltaTime);

            var direction = 0f;
            if (Quaternion.Angle(previousRotation, _transform.rotation) > 0.01f)
            {
                var angle = Vector3.SignedAngle(previousRotation * Vector3.forward, _transform.rotation * Vector3.forward, Vector3.up);
                direction = angle > 0f ? -1f : 1f;
            }

            if (Mathf.Abs(direction - _lastRotationDirection) > 0.01f)
            {
                _lastRotationDirection = direction;
                OnRotationDirection?.Invoke(direction);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private void RotatePlayerToCursor(Vector3 cursorPosition)
    {
        if (_animator.GetCurrentAnimatorStateInfo(0).shortNameHash == _animationStates.InteractStateHash)
        {
            return;
        }

        var direction = cursorPosition - _transform.position;
        direction.y = 0f;

        var sqrMagnitude = direction.sqrMagnitude;
        if (sqrMagnitude < minRotationDistance * minRotationDistance)
        {
            return;
        }

        var targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        if (sqrMagnitude < closeRotationDistance * closeRotationDistance)
        {
            var angle = Quaternion.Angle(_transform.rotation, targetRotation);
            if (angle > maxCloseRotationAngle)
            {
                return;
            }
        }

        _targetRotation = targetRotation;
    }
}