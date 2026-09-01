using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class DoorController : MonoBehaviour
{
    [System.Serializable]
    private class Leaf
    {
        [field: SerializeField] public Transform LeafTransform { get; private set; }
        [field: SerializeField, Range(-180, 180)] public float OpenAngle { get; private set; }
        [field: SerializeField] public float OpenSpeed { get; private set; }
        [field: SerializeField] public bool CloseAutomatic { get; private set; }
        [field: SerializeField] public float CloseDelay { get; private set; }

        [HideInInspector] public bool isOpen;
        
        public UnityEvent onOpenStart, onOpenEnd, onCloseStart, onCloseEnd;
    }
    
    [SerializeField] private Leaf[] leafs;
    
    private CancellationTokenSource _cts;
    
    public void OpenLeafs()
    {
        OpenLeafsAsync().Forget();
    }

    public void CloseLeafs()
    {
        CloseLeafsAsync().Forget();
    }

    private async UniTask OpenLeafsAsync()
    {
        if (leafs == null || leafs.Length == 0)
        {
            return;
        }

        Cancel();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        var token = _cts.Token;

        var tasks = new UniTask[leafs.Length];
        for (var i = 0; i < leafs.Length; i++)
        {
            var leaf = leafs[i];
            if (leaf.isOpen)
            {
                tasks[i] = UniTask.CompletedTask;
                continue;
            }

            tasks[i] = OpenLeafAsync(leaf, token);
        }

        await UniTask.WhenAll(tasks);
    }

    private async UniTask CloseLeafsAsync()
    {
        if (leafs == null || leafs.Length == 0)
        {
            return;
        }

        Cancel();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
        var token = _cts.Token;

        var tasks = new UniTask[leafs.Length];
        for (var i = 0; i < leafs.Length; i++)
        {
            var leaf = leafs[i];
            if (!leaf.isOpen)
            {
                tasks[i] = UniTask.CompletedTask;
                continue;
            }

            tasks[i] = CloseLeafAsync(leaf, token);
        }

        await UniTask.WhenAll(tasks);
    }

    private async UniTask OpenLeafAsync(Leaf leaf, CancellationToken token)
    {
        leaf.onOpenStart?.Invoke();

        await RotateLeafAsync(leaf, leaf.OpenAngle, token);

        leaf.isOpen = true;
        leaf.onOpenEnd?.Invoke();

        if (leaf.CloseAutomatic)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(leaf.CloseDelay), cancellationToken: token);
            await CloseLeafAsync(leaf, token);
        }
    }

    private async UniTask CloseLeafAsync(Leaf leaf, CancellationToken token)
    {
        leaf.onCloseStart?.Invoke();

        await RotateLeafAsync(leaf, 0f, token);

        leaf.isOpen = false;
        leaf.onCloseEnd?.Invoke();
    }

    private async UniTask RotateLeafAsync(Leaf leaf, float targetAngle, CancellationToken token)
    {
        if (!leaf.LeafTransform)
        {
            return;
        }

        var transform = leaf.LeafTransform;
        var currentAngle = transform.localEulerAngles.y;
        var delta = Mathf.DeltaAngle(currentAngle, targetAngle);
        var duration = Mathf.Abs(delta) / Mathf.Max(leaf.OpenSpeed, 0.01f);

        var tween = transform.DOLocalRotate(
            new Vector3(transform.localEulerAngles.x, targetAngle, transform.localEulerAngles.z),
            duration
        ).SetEase(Ease.Linear);

        var tcs = new UniTaskCompletionSource();

        tween.OnComplete(() => tcs.TrySetResult());
        tween.OnKill(() => tcs.TrySetResult());

        CancellationTokenRegistration registration = default;
        registration = token.Register(() =>
        {
            if (tween.IsActive())
                tween.Kill();

            registration.Dispose();
        });

        await tcs.Task;
    }

    private void Cancel()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private void OnDestroy()
    {
        Cancel();
    }
}