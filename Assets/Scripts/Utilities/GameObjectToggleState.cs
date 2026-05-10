using UnityEngine;

public class GameObjectToggleState : MonoBehaviour
{
    public void DisableGO(){gameObject.SetActive(false);}
    public void EnableGO(){gameObject.SetActive(true);}
}
