using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

public class LadderInteractor : MonoBehaviour
{
   [SerializeField] private InteractOnTrigger trigger;
   [SerializeField] private float walkSpeed;
   [SerializeField] private float runSpeed;
   [SerializeField, Range(-1, 1)] private int direction;
   [SerializeField] private float rotationSpeed = 5f;
   [SerializeField] private Vector3 positionOffset;
   [SerializeField] private float enterDuration = 0.2f;

   public UnityEvent<Character> onInteractEnter, onInteractExit;

   [Inject] private readonly PlayerInputHandler _input;

   private Collider _currentCollider;
   private Character _currentController;
   private bool _isPlaying;
   private CancellationTokenSource _rotationCts;
   private CancellationTokenSource _enterCts;

   private void OnEnable()
   {
      if (trigger)
      {
         trigger.onEnter.AddListener(OnEnter);
         trigger.onExit.AddListener(OnExit);
      }

      _input.OnInteract += OnInteractEnter;
   }

   private void OnDisable()
   {
      if (trigger)
      {
         trigger.onEnter.RemoveListener(OnEnter);
         trigger.onExit.RemoveListener(OnExit);
      }

      if (_input != null)
      {
         _input.OnInteract -= OnInteractEnter;
      }

      StopAllAsync();
   }

   private void OnEnter(Collider other)
   {
      if (!trigger)
         return;

      _currentCollider = other;
   }

   private void OnExit(Collider other)
   {
      if (!trigger)
         return;

      if (_currentCollider == other)
         _currentCollider = null;
   }

   private void OnInteractEnter()
   {
      if (_isPlaying || !_currentCollider)
         return;

      if (!_currentCollider.gameObject.TryGetComponent<Character>(out var targetController))
         return;

      _currentController = targetController;

      if (_currentController.IsOnLadder)
         return;

      onInteractEnter?.Invoke(_currentController);

      _currentController.SetOnLadder(true, direction);
      _isPlaying = true;

      var targetPosition = transform.position + positionOffset;
      targetPosition.y = _currentController.transform.position.y;

      StopEnterTween();
      _enterCts = new CancellationTokenSource();
      EnterAsync(targetPosition, _enterCts.Token).Forget();
   }

   private async UniTask EnterAsync(Vector3 targetPosition, CancellationToken token)
   {
      var startPos = _currentController.transform.position;
      var startRot = _currentController.transform.rotation;
      var targetRot = transform.rotation;

      var elapsed = 0f;

      while (elapsed < enterDuration)
      {
         if (token.IsCancellationRequested || !_currentController)
            return;

         elapsed += Time.deltaTime;
         var t = Mathf.Clamp01(elapsed / enterDuration);

         _currentController.transform.position = Vector3.Lerp(startPos, targetPosition, t);
         _currentController.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);

         await UniTask.Yield(PlayerLoopTiming.Update, token);
      }

      if (_currentController)
      {
         _currentController.transform.position = targetPosition;
         _currentController.transform.rotation = targetRot;
      }

      if (!token.IsCancellationRequested)
         StartRotationLoop();
   }

   private void OnInteractExit()
   {
      if (!_currentController)
         return;

      if (!_currentController.IsOnLadder)
         return;

      onInteractExit?.Invoke(_currentController);

      _currentController.SetOnLadder(false);
      _currentController = null;
      _isPlaying = false;

      StopAllAsync();
   }

   private void StartRotationLoop()
   {
      StopRotationLoop();
      _rotationCts = new CancellationTokenSource();
      RotationLoopAsync(_rotationCts.Token).Forget();
   }

   private void StopRotationLoop()
   {
      _rotationCts?.Cancel();
      _rotationCts?.Dispose();
      _rotationCts = null;
   }

   private void StopEnterTween()
   {
      _enterCts?.Cancel();
      _enterCts?.Dispose();
      _enterCts = null;
   }

   private void StopAllAsync()
   {
      StopRotationLoop();
      StopEnterTween();
   }

   private async UniTask RotationLoopAsync(CancellationToken token)
   {
      while (!token.IsCancellationRequested)
      {
         if (_isPlaying && _currentController)
         {
            _currentController.transform.rotation = Quaternion.Slerp(
               _currentController.transform.rotation,
               transform.rotation,
               rotationSpeed * Time.deltaTime);

            var motionY = _currentController.GetMotionY();
            if (Mathf.Abs(motionY) > 0.01f)
            {
               var motionSpeed = Mathf.Clamp01(_currentController.GetMotionSpeed());
               var verticalSpeed = Mathf.Lerp(walkSpeed, runSpeed, motionSpeed);
               var position = _currentController.transform.position;
               position.y += motionY * verticalSpeed * Time.deltaTime;
               _currentController.transform.position = position;
            }
         }

         await UniTask.Yield(PlayerLoopTiming.Update, token);
      }
   }
}