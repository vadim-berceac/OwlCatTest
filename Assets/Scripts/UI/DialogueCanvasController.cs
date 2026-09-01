using System;
using TMPro;
using UnityEngine;
using Zenject;
using System.Threading;
using Cysharp.Threading.Tasks;

public class DialogueCanvasController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;

    [Inject] private readonly AnimationStates _animationStates;

    private CancellationTokenSource _deactivationCts;

    public void ActivateCanvasWithText(string text)
    {
        CancelDeactivation();

        gameObject.SetActive(true);
        animator.SetBool(_animationStates.HashActivePara, true);
        textMeshProUGUI.text = text;
    }

    public void DeactivateCanvasWithDelay(float delay)
    {
        CancelDeactivation();

        _deactivationCts = new CancellationTokenSource();
        DeactivateWithDelayAsync(delay, _deactivationCts.Token).Forget();
    }

    private async UniTask DeactivateWithDelayAsync(float delay, CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.Delay(
                TimeSpan.FromSeconds(delay),
                ignoreTimeScale: false,
                cancellationToken: cancellationToken
            );

            if (this == null || !gameObject) return;

            animator.SetBool(_animationStates.HashActivePara, false);
        }
        catch (OperationCanceledException)
        {
            
        }
    }

    private void CancelDeactivation()
    {
        if (_deactivationCts != null)
        {
            _deactivationCts.Cancel();
            _deactivationCts.Dispose();
            _deactivationCts = null;
        }
    }

    private void OnDestroy()
    {
        CancelDeactivation();
    }
}
