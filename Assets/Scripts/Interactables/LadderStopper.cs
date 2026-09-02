using UnityEngine;

public class LadderStopper : MonoBehaviour
{
    [SerializeField] private InteractOnTrigger trigger;
    [SerializeField] private LadderInteractor ladder;
    
    private void OnEnable()
    {
        if (trigger)
        {
            trigger.onEnter.AddListener(OnEnter);
        }
    }

    private void OnDisable()
    {
        if (trigger)
        {
            trigger.onEnter.RemoveListener(OnEnter);
        }
    }
    
    private void OnEnter(Collider other)
    {
        if (!trigger)
            return;
        
        Debug.Log(other.name);

        if (!other.TryGetComponent(out Character character))
        {
            return;
        }

        if (!character.IsOnLadder && (character != ladder.CurrentController || ladder.OtherEnd.CurrentController))
        {
            return;
        }

        ladder.OnInteractExit();
        ladder.OtherEnd.OnInteractExit();
    }
}
