using UnityEngine;
using System.Collections;

public class LightingController : MonoBehaviour
{
    public static LightingController Instance;

    [Header("Pengaturan Cahaya")]
    public Light sunLight; // Taruh Directional Light di sini
    public float fadeSpeed = 2f;
    
    [Header("Intensitas Cahaya")]
    [Tooltip("Intensitas matahari di luar ruangan (Gunakan 2.0 - 5.0 untuk hasil yang sangat cerah)")]
    public float defaultIntensity = 5f; 
    
    [Tooltip("Intensitas matahari di dalam ruangan (0.1 - 0.5 disarankan agar tidak gelap total)")]
    public float interiorIntensity = 0.2f; 

    [Header("Suhu Warna")]
    [Tooltip("Suhu warna default cahaya matahari dalam Kelvin (6500K = putih netral).")]
    public float defaultTemperature = 6500f;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (sunLight == null) sunLight = GetComponent<Light>();
        
        // Pastikan game dimulai dengan intensitas dan suhu warna default
        if (sunLight != null)
        {
            sunLight.intensity = defaultIntensity;
            sunLight.useColorTemperature = true;
            sunLight.colorTemperature = defaultTemperature;
        }
    }

    // Panggil ini saat masuk ruangan (dengan Fade)
    public void SetToInterior()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeLight(interiorIntensity));
    }

    // Panggil ini saat keluar ruangan (dengan Fade)
    public void SetToExterior()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeLight(defaultIntensity));
    }

    // Panggil ini saat Warping (Tanpa Fade / Seketika)
    public void SetLightInstant(float targetIntensity)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        
        if (sunLight != null)
        {
            sunLight.intensity = targetIntensity;
        }
    }

    // Atur suhu warna seketika (Tanpa Fade)
    public void SetTemperatureInstant(float targetTemperature)
    {
        if (sunLight != null)
        {
            sunLight.colorTemperature = targetTemperature;
        }
    }

    private IEnumerator FadeLight(float target)
    {
        if (sunLight == null) yield break;

        while (!Mathf.Approximately(sunLight.intensity, target))
        {
            sunLight.intensity = Mathf.MoveTowards(sunLight.intensity, target, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
