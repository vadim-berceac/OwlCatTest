using UnityEngine;
using Zenject;

public class DialogueAdapter : MonoBehaviour
{
    [SerializeField] private string phrase;
    [SerializeField] private float deactivateDelay;
    
    [Inject] private readonly DialogueCanvasController _dialogueCanvasController;
    private bool _enabled = true;

    public void Enable(bool value)
    {
        _enabled = value;

        if (!_enabled)
        {
            DeactivateCanvasWithDelay();
        }
    }

    public void ActivateCanvasWithText()
    {
        if(!_dialogueCanvasController || !_enabled) return;
        _dialogueCanvasController.ActivateCanvasWithText(phrase);
    }

    public void DeactivateCanvasWithDelay()
    {
        if(!_dialogueCanvasController) return;
        _dialogueCanvasController.DeactivateCanvasWithDelay(deactivateDelay);
    }

    private void OnDisable()
    {
        DeactivateCanvasWithDelay();
    }
}
