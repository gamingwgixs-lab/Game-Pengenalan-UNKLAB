using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance;

    [Header("Referensi UI")]
    public Image panelHitam;
    
    [Header("Pengaturan")]
    public float kecepatanFade = 1.5f;
    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (panelHitam != null)
        {
            panelHitam.gameObject.SetActive(false);
            StartCoroutine(FadeIn());
        }
    }

    public void PindahScene(int indexSceneTujuan)
    {
        StartCoroutine(FadeOutDanLoad(indexSceneTujuan));
    }

    private IEnumerator FadeIn()
    {
        Color warna = panelHitam.color;
        warna.a = 1f;
        panelHitam.color = warna;

        while (warna.a > 0f)
        {
            warna.a -= Time.deltaTime * kecepatanFade;
            panelHitam.color = warna;
            yield return null; 
        }

        panelHitam.gameObject.SetActive(false); 
    }

    private IEnumerator FadeOutDanLoad(int indexScene)
    {
        panelHitam.gameObject.SetActive(true);

        Color warna = panelHitam.color;
        warna.a = 0f;
        panelHitam.color = warna;

        while (warna.a < 1f)
        {
            // Tambah ketebalan warna hitam perlahan-lahan
            warna.a += Time.deltaTime * kecepatanFade;
            panelHitam.color = warna;
            yield return null;
        }

        SceneManager.LoadScene(indexScene);
    }
}