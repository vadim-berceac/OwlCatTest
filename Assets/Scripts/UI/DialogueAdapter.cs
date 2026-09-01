using UnityEngine;
using Zenject;

public class DialogueAdapter : MonoBehaviour
{
    [SerializeField] private string phrase;
    [Inject] private readonly DialogueCanvasController _dialogueCanvasController;

    public void ActivateCanvasWithText()
    {
        if(!_dialogueCanvasController) return;
        _dialogueCanvasController.ActivateCanvasWithText(phrase);
    }

    public void DeactivateCanvasWithDelay(float delay)
    {
        if(!_dialogueCanvasController) return;
        _dialogueCanvasController.DeactivateCanvasWithDelay(delay);
    }
}
