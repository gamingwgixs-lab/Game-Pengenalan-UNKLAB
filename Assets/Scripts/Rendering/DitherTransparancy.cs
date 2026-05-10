using UnityEngine;
using System.Collections;

public class DitherTransparency : MonoBehaviour
{
    [Header("Pengaturan Dither")]
    [Tooltip("Nilai transparansi saat player di belakang objek (0 = hilang total, 1 = solid)")]
    [Range(0f, 1f)]
    public float targetFade = 0.4f; 
    
    [Tooltip("Kecepatan transisi memudar (semakin besar semakin cepat)")]
    public float fadeSpeed = 5f;

    [Header("Pengaturan Reset")]
    [Tooltip("Jika dicentang, objek akan kembali solid saat player warping. Jika tidak, objek tetap pada status terakhirnya.")]
    public bool resetOnWarp = true;

    private Material wallMaterial;
    private float currentFade = 1f;
    private Coroutine fadeCoroutine;

    // Sesuai dengan nama variabel di Shader Graph Blackboard kamu
    private static readonly int FadeID = Shader.PropertyToID("_FadeAmount");

    void Start()
    {
        // Otomatis mengambil material dari objek ini agar tidak perlu drag-and-drop manual
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // Menggunakan .material akan membuat instance (aman jika tiap gedung punya fade mandiri)
            wallMaterial = rend.material; 
        }
        else
        {
            Debug.LogError("Tidak ada komponen Renderer di objek ini!");
        }

        // Pastikan game dimulai dengan tembok solid (1f)
        if (wallMaterial != null)
        {
            wallMaterial.SetFloat(FadeID, 1f);
        }
    }

    // Dipanggil SEKALI saja saat Player menyentuh area pemicu
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && wallMaterial != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(targetFade));
        }
    }

    // Dipanggil SEKALI saja saat Player keluar dari area pemicu
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && wallMaterial != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(1f));
        }
    }

    public void ProxyEnter(Collider other)
    {
        if (wallMaterial != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(targetFade));
        }
    }

    public void ProxyExit(Collider other)
    {
        if (wallMaterial != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(1f));
        }
    }

    // Fungsi untuk membuat transisi memudarnya halus (Smooth Lerp), bukan tiba-tiba hilang
    private IEnumerator FadeRoutine(float target)
    {
        while (!Mathf.Approximately(currentFade, target))
        {
            // Bergerak perlahan dari nilai sekarang ke nilai target
            currentFade = Mathf.MoveTowards(currentFade, target, fadeSpeed * Time.deltaTime);
            if (wallMaterial != null) wallMaterial.SetFloat(FadeID, currentFade);
            
            // Tunggu frame berikutnya
            yield return null; 
        }
    }

    // Fungsi baru untuk memaksa objek kembali solid (tidak transparan) seketika
    public void ForceSolid(bool forceIgnoreSetting = false)
    {
        // Jika resetOnWarp dimatikan, abaikan perintah reset kecuali dipaksa (forceIgnoreSetting)
        if (!resetOnWarp && !forceIgnoreSetting) return;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        currentFade = 1f;
        if (wallMaterial != null)
        {
            wallMaterial.SetFloat(FadeID, 1f);
        }
    }

    // Menjamin saat objek dimatikan (pindah scene), statusnya kembali solid
    private void OnDisable()
    {
        ForceSolid(true); // Saat disable, kita paksa reset demi keamanan memori
    }
}