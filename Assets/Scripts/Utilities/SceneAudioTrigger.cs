using UnityEngine;

public class SceneAudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip musicForThisScene;

    private void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMusic(musicForThisScene);
        }
    }
}
