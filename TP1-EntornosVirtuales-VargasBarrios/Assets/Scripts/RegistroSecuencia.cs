using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class RegistroSecuencia : MonoBehaviour
{

    public ControladorJuego controlador;
    public SemaforoLuces semaforo;
    public float pausaEntreRondas = 1f;
    public float tiempoPulsacion = 0.2f;

    public List<int> secuencia = new List<int>();
    public List<string> coloresPulsados = new List<string>();

    public int ronda;
    public bool turnoJugador;
    [Min(1)] public int rondasParaGanar = 3;
    public UnityEvent alTerminarAparicion = new UnityEvent();
    public UnityEvent alGanar = new UnityEvent();
    public bool JuegoGanado { get; private set; }

    [Header("Efectos de resultado")]
    [Min(0.01f)] public float tiempoDestelloDerrota = 0.12f;
    public AudioSource fuenteAudio;
    public AudioClip sonidoDerrota;
    public AudioClip sonidoVictoria;



    void OnEnable()
    {
        StartCoroutine(EsperarSemaforo());
    }

    IEnumerator EsperarSemaforo()
    {
        yield return null;

        while (
                (controlador != null && !controlador.IntroTerminada)
                || semaforo == null
                || !semaforo.isActiveAndEnabled
            )
        {
            if (controlador != null && controlador.PartidaFinalizada)
                yield break;

            if (semaforo == null || !semaforo.isActiveAndEnabled)
                semaforo = FindAnyObjectByType<SemaforoLuces>();

            yield return new WaitForSeconds(0.2f);
        }

        IniciarJuego();
    }

    public void IniciarJuego()
    {
        if (controlador != null &&
            (!controlador.IntroTerminada || controlador.PartidaFinalizada))
        {
            return;
        }
        StopAllCoroutines();
        if (fuenteAudio != null) fuenteAudio.Stop();

        JuegoGanado = false;
        rondasParaGanar = Mathf.Max(1, rondasParaGanar);

        turnoJugador = false;
        ronda = 0;
        secuencia.Clear();
        coloresPulsados.Clear();

        if (semaforo == null || !semaforo.isActiveAndEnabled)
        {
            Debug.Log("Esperando al semaforo.");
            StartCoroutine(EsperarSemaforo());
            return;
        }

        semaforo.ApagarTodo();
        StartCoroutine(SiguienteRonda());
    }

    IEnumerator SiguienteRonda()
    {
        turnoJugador = false;
        yield return new WaitForSeconds(pausaEntreRondas);

        if (semaforo == null || !semaforo.isActiveAndEnabled) yield break;

        secuencia.Add(Random.Range(0, 3));
        ronda = secuencia.Count;
        coloresPulsados.Clear();

        Debug.Log("Ronda " + ronda + ": mira las luces.");
        semaforo.MostrarSecuencia(secuencia.ToArray());

        while (semaforo != null && semaforo.EstaReproduciendo)
        {
            yield return null;
        }

        if (semaforo == null || !semaforo.isActiveAndEnabled) yield break;

        turnoJugador = true;
        Debug.Log("Tu turno. Repite los " + secuencia.Count + " colores.");
    }

    public void PulsarRojo()
    {
        RegistrarColor(0, "Rojo");
    }

    public void PulsarAmarillo()
    {
        RegistrarColor(1, "Amarillo");
    }

    public void PulsarVerde()
    {
        RegistrarColor(2, "Verde");
    }

    void RegistrarColor(int color, string nombre)

    {
        if (JuegoGanado || !turnoJugador || semaforo == null || !semaforo.isActiveAndEnabled) return;

        StopAllCoroutines();
        coloresPulsados.Add(nombre);
        Debug.Log("Pulso " + nombre);
        int posicion = coloresPulsados.Count - 1;

        if (color != secuencia[posicion])
        {
            turnoJugador = false;

            if (controlador != null)
            {
                float espera = 4f * Mathf.Max(0.01f, tiempoDestelloDerrota);
                controlador.RegistrarDerrota(espera);
            }

            StartCoroutine(MostrarDerrota());
            return;
        }

        bool rondaCompleta = coloresPulsados.Count == secuencia.Count;
        if (rondaCompleta)
        {
            turnoJugador = false;
        }

        StartCoroutine(MostrarPulsacion(color, rondaCompleta));
    }

    IEnumerator MostrarPulsacion(int color, bool rondaCompleta)
    {
        if (color == 0) semaforo.EncenderRojo();
        if (color == 1) semaforo.EncenderAmarillo();
        if (color == 2) semaforo.EncenderVerde();

        yield return new WaitForSeconds(tiempoPulsacion);
        if (semaforo != null) semaforo.ApagarTodo();

        if (!rondaCompleta) yield break;

        if (ronda >= rondasParaGanar)
        {
            GanarJuego();
        }
        else
        {
            StartCoroutine(SiguienteRonda());
        }
    }

    void GanarJuego()
    {
        if (JuegoGanado) return;

        JuegoGanado = true;
        turnoJugador = false;
        if (semaforo != null) semaforo.EncenderTodo();
        ReproducirSonido(sonidoVictoria);
        Debug.Log("Puzzle completado. Ganaste en la ronda " + ronda + ".");
        if (controlador != null)
            controlador.RegistrarVictoria();
        alGanar.Invoke();
    }

    IEnumerator MostrarDerrota()
    {
        ReproducirSonido(sonidoDerrota);
        float duracion = Mathf.Max(0.01f, tiempoDestelloDerrota);

        for (int i = 0; i < 2; i++)
        {
            if (semaforo == null || !semaforo.isActiveAndEnabled) yield break;
            semaforo.EncenderTodo();
            yield return new WaitForSeconds(duracion);

            if (semaforo == null) yield break;
            semaforo.ApagarTodo();
            yield return new WaitForSeconds(duracion);
        }
    }

    void ReproducirSonido(AudioClip clip)
    {
        if (fuenteAudio == null || clip == null) return;
        fuenteAudio.Stop();
        fuenteAudio.PlayOneShot(clip);
    }

    public void ReiniciarSecuencia()
    {
        IniciarJuego();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        if (fuenteAudio != null) fuenteAudio.Stop();
        turnoJugador = false;
        if (semaforo != null) semaforo.ApagarTodo();
    }

}
