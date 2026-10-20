using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;



[DefaultExecutionOrder(-100)]

public class ControladorJuego : MonoBehaviour
{
    [Header("Introducción")]
    public VideoPlayer video;
    public GameObject panelIntro;

    [Header("Finales")]
    public Button toqueVictoria;
    public GameObject panelFinal;
    public CanvasGroup grupoFinal;
    public float duracionFundido = 0.8f;
    public TMP_Text textoFinal;
    public float segundosAntesDeCerrar = 4f;

    public bool IntroTerminada { get; private set; }
    public bool PartidaFinalizada { get; private set; }

    private bool victoria;
    private bool mostrandoFinal;

    void Awake()
    {
        panelIntro.SetActive(true);
        panelFinal.SetActive(false);
        toqueVictoria.gameObject.SetActive(false);

        toqueVictoria.onClick.AddListener(MostrarFinalVictoria);

        video.playOnAwake = false;
        video.isLooping = false;
        video.loopPointReached += TerminarIntro;
        video.errorReceived += ErrorVideo;
    }

    void Start()
    {
        video.Play();
    }

    void TerminarIntro(VideoPlayer reproductor)
    {
        if (IntroTerminada) return;

        IntroTerminada = true;
        reproductor.Stop();
        panelIntro.SetActive(false);
    }

    void ErrorVideo(VideoPlayer reproductor, string mensaje)
    {
        Debug.LogWarning("No se pudo reproducir la intro: " + mensaje);
        TerminarIntro(reproductor);
    }

    public void RegistrarVictoria()
    {
        if (PartidaFinalizada) return;

        PartidaFinalizada = true;
        victoria = true;
    }

    // Se llama cuando el bichito termina su recorrido.
    public void HabilitarToqueVictoria()
    {
        if (!victoria || mostrandoFinal) return;

        toqueVictoria.gameObject.SetActive(true);
    }

    void MostrarFinalVictoria()
    {
        if (!victoria || mostrandoFinal) return;

        MostrarFinal("Te están observando");
    }

    public void RegistrarDerrota(float espera)
    {
        if (PartidaFinalizada) return;

        PartidaFinalizada = true;
        StartCoroutine(EsperarDerrota(espera));
    }

    IEnumerator EsperarDerrota(float espera)
    {
        yield return new WaitForSeconds(espera);
        MostrarFinal("La puerta ha desaparecido");
    }

    void MostrarFinal(string mensaje)
    {
        if (mostrandoFinal) return;

        mostrandoFinal = true;
        toqueVictoria.gameObject.SetActive(false);

        textoFinal.text = mensaje;

        grupoFinal.alpha = 0f;
        panelFinal.SetActive(true);

        StartCoroutine(AparecerFinal());
    }
    IEnumerator AparecerFinal()
    {
        float tiempo = 0f;
        float duracion = Mathf.Max(0.01f, duracionFundido);

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;

            grupoFinal.alpha = Mathf.SmoothStep(
                0f,
                1f,
                Mathf.Clamp01(tiempo / duracion)
            );

            yield return null;
        }

        grupoFinal.alpha = 1f;

        yield return CerrarAplicacion();
    }

    IEnumerator CerrarAplicacion()
    {
        yield return new WaitForSecondsRealtime(
            Mathf.Max(1f, segundosAntesDeCerrar)
        );

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void OnDestroy()
    {
        if (video != null)
        {
            video.loopPointReached -= TerminarIntro;
            video.errorReceived -= ErrorVideo;
        }

        if (toqueVictoria != null)
            toqueVictoria.onClick.RemoveListener(MostrarFinalVictoria);
    }
}
