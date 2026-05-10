using UnityEngine;
using UnityEngine.Events;

public class ProximityTrigger : MonoBehaviour
{
    public UnityEvent OnPlayerEnter;
    public UnityEvent OnPlayerExit;

    private void OnTriggerEnter(Collider other){
        if (other.CompareTag("Player")){
            OnPlayerEnter.Invoke();
        }
    }

    private void OnTriggerExit(Collider other){
        if (other.CompareTag("Player")){
            OnPlayerExit.Invoke();
        }
    }
}
