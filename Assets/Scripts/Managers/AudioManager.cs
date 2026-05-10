using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource footstepSource; // Speaker khusus kaki

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ─────────────────────────────────────────
    // FOOTSTEP LOGIC (HARD OVERRIDE)
    // ─────────────────────────────────────────
    public void PlayFootstep(AudioClip clip, float volume = 1.0f)
    {
        if (clip == null) return;
        
        footstepSource.Stop(); // Matikan suara langkah sebelumnya
        footstepSource.clip = clip;
        footstepSource.volume = volume;
        footstepSource.Play(); // Mulai suara langkah baru
    }

    // ─────────────────────────────────────────
    // MUSIC LOGIC (Simple Switch)
    // ─────────────────────────────────────────
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.Play();
        // Volume akan mengikuti apa pun yang Anda set di Inspector AudioSource
    }

    /// <summary>
    /// Mendapatkan AudioClip musik yang sedang diputar saat ini.
    /// Digunakan oleh SessionManager untuk menyimpan dan mengembalikan musik saat berpindah scene.
    /// </summary>
    public AudioClip GetCurrentMusic()
    {
        return musicSource != null ? musicSource.clip : null;
    }

    // ─────────────────────────────────────────
    // SFX LOGIC
    // ─────────────────────────────────────────
    public void PlaySFX(AudioClip clip, float volumeScale = 1.0f)
    {
        if (clip == null) return;
        
        // Memutar suara pendek tanpa mengganggu musik
        sfxSource.PlayOneShot(clip, volumeScale);
    }
}