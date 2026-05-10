using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditRollManager : MonoBehaviour
{
    [Header("--- Urutan Tampilan ---")]
    [Tooltip("CanvasGroup logo UXPLORE FILKOM.")]
    public CanvasGroup logoUxplore;

    [Tooltip("CanvasGroup informasi developer.")]
    public CanvasGroup developerInfo;

    [Tooltip("CanvasGroup quotes Martin Luther King Jr.")]
    public CanvasGroup quotesSection;

    [Header("--- Timing Logo ---")]
    public float logoFadeInDuration = 1.5f;
    public float logoHoldDuration = 2f;
    public float logoFadeOutDuration = 1f;

    [Header("--- Timing Developer Info ---")]
    public float devFadeInDuration = 1f;
    public float devHoldDuration = 4f;
    public float devFadeOutDuration = 1f;

    [Header("--- Timing Quotes ---")]
    public float quotesFadeInDuration = 1f;
    public float quotesHoldDuration = 5f;
    public float quotesFadeOutDuration = 1.5f;

    [Header("--- Jeda Antar Section ---")]
    [Tooltip("Jeda hitam antar section (detik).")]
    public float delayBetweenSections = 1f;

    [Header("--- Scene Tujuan ---")]
    public int mainMenuSceneIndex = 0;

    [Header("--- Audio ---")]
    [Tooltip("Background musik untuk credit scene.")]
    public AudioClip bgmClip;

    private bool canSkip = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Putar BGM
        if (bgmClip != null)
        {
            if (AudioManager.Instance != null)
            {
                // Gunakan AudioManager jika tersedia (transisi dari gameplay)
                AudioManager.Instance.PlayMusic(bgmClip);
            }
            else
            {
                // Fallback: buat AudioSource lokal (testing langsung dari scene ini)
                AudioSource localMusic = gameObject.AddComponent<AudioSource>();
                localMusic.clip = bgmClip;
                localMusic.loop = true;
                localMusic.volume = 0.5f;
                localMusic.Play();
            }
        }

        // Sembunyikan semua section di awal
        HideAll();

        StartCoroutine(EndCreditSequence());
    }

    private void HideAll()
    {
        if (logoUxplore != null) { logoUxplore.alpha = 0f; logoUxplore.gameObject.SetActive(false); }
        if (developerInfo != null) { developerInfo.alpha = 0f; developerInfo.gameObject.SetActive(false); }
        if (quotesSection != null) { quotesSection.alpha = 0f; quotesSection.gameObject.SetActive(false); }
    }

    /// <summary>
    /// Sequence utama:
    /// 1. Logo UXPLORE FILKOM  (fade in → tahan → fade out)
    /// 2. Developer Information (fade in → tahan → fade out)
    /// 3. Quotes MLK Jr        (fade in → tahan → fade out)
    /// 4. Kembali ke Main Menu
    /// </summary>
    private IEnumerator EndCreditSequence()
    {
        // ── 1. LOGO UXPLORE FILKOM ──
        yield return StartCoroutine(ShowSection(logoUxplore, logoFadeInDuration, logoHoldDuration, logoFadeOutDuration));

        yield return new WaitForSeconds(delayBetweenSections);

        // ── 2. DEVELOPER INFORMATION ──
        yield return StartCoroutine(ShowSection(developerInfo, devFadeInDuration, devHoldDuration, devFadeOutDuration));

        yield return new WaitForSeconds(delayBetweenSections);

        // ── 3. QUOTES MARTIN LUTHER KING JR ──
        canSkip = true;
        yield return StartCoroutine(ShowSection(quotesSection, quotesFadeInDuration, quotesHoldDuration, quotesFadeOutDuration));

        yield return new WaitForSeconds(delayBetweenSections);

        // ── SELESAI → Main Menu ──
        ReturnToMainMenu();
    }

    /// <summary>
    /// Menampilkan satu section: fade in → tahan → fade out.
    /// </summary>
    private IEnumerator ShowSection(CanvasGroup section, float fadeIn, float hold, float fadeOut)
    {
        if (section == null) yield break;

        section.gameObject.SetActive(true);

        // Fade In
        yield return StartCoroutine(FadeCanvasGroup(section, 0f, 1f, fadeIn));

        // Tahan
        yield return new WaitForSeconds(hold);

        // Fade Out
        yield return StartCoroutine(FadeCanvasGroup(section, 1f, 0f, fadeOut));

        section.gameObject.SetActive(false);
    }

    /// <summary>
    /// Utilitas fade alpha CanvasGroup.
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        float elapsed = 0f;
        group.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        group.alpha = to;
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    private void Update()
    {
        // Skip hanya bisa setelah quotes tampil
        if (canSkip && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space)))
        {
            ReturnToMainMenu();
        }
    }
}