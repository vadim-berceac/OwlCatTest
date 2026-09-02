using UnityEngine;

public class LadderStopper : MonoBehaviour
{
    [SerializeField] private InteractOnTrigger trigger;
    
    private void OnEnable()
    {
        if (trigger)
        {
            trigger.onEnter.AddListener(OnEnter);
            trigger.onExit.AddListener(OnExit);
        }
    }

    private void OnDisable()
    {
        if (trigger)
        {
            trigger.onEnter.RemoveListener(OnEnter);
            trigger.onExit.RemoveListener(OnExit);
        }
    }
    
    private void OnEnter(Collider other)
    {
        if (!trigger)
            return;
        
        Debug.Log(other.name);

    }

    private void OnExit(Collider other)
    {
        if (!trigger)
            return;
        Debug.Log(other.name);

    }
}
