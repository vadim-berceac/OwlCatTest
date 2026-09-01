using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class InteractOnTrigger : MonoBehaviour
{
    public UnityEvent<Collider> onEnter;
    public UnityEvent<Collider> onExit;

    private void OnTriggerEnter(Collider other)
    {
        ExecuteOnEnter(other);
    }
        
    private void OnTriggerExit(Collider other)
    {
        ExecuteOnExit(other);
    }

    private void ExecuteOnEnter(Collider other)
    {
        onEnter.Invoke(other);
    }

    private void ExecuteOnExit(Collider other)
    {
        onExit.Invoke(other);
    }
        
    private void OnDestroy()
    {
        onExit?.Invoke(null);
    }
} 
