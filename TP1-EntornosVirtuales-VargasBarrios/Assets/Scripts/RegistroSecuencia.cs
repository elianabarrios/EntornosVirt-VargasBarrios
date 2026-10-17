using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegistroSecuencia : MonoBehaviour
{
    public SemaforoLuces semaforo;
    public float pausaEntreRondas = 1f;
    public float tiempoPulsacion = 0.2f;

    public List<int> secuencia = new List<int>();
    public List<string> coloresPulsados = new List<string>();

    public int ronda;
    public bool turnoJugador;

    void OnEnable()
    {
        StartCoroutine(EsperarSemaforo());
    }

    IEnumerator EsperarSemaforo()
    {
        yield return null;

        while (semaforo == null || !semaforo.isActiveAndEnabled)
        {
            semaforo = FindAnyObjectByType<SemaforoLuces>();
            yield return new WaitForSeconds(0.2f);
        }

        IniciarJuego();
    }

    public void IniciarJuego()
    {
        StopAllCoroutines();
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
        if (!turnoJugador || semaforo == null || !semaforo.isActiveAndEnabled) return;

        StopAllCoroutines();
        coloresPulsados.Add(nombre);
        Debug.Log("Pulso " + nombre);
        int posicion = coloresPulsados.Count - 1;

        if (color != secuencia[posicion])
        {
            turnoJugador = false;
            Debug.Log("Te equivocaste en la ronda " + ronda + ". Reinicia para jugar otra vez.");
            StartCoroutine(MostrarPulsacion(color, false));
            return;
        }

        bool rondaCompleta = coloresPulsados.Count == secuencia.Count;
        if (rondaCompleta)
        {
            turnoJugador = false;
            Debug.Log("Ronda completada.");
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

        if (rondaCompleta) StartCoroutine(SiguienteRonda());
    }

    public void ReiniciarSecuencia()
    {
        IniciarJuego();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        turnoJugador = false;
        if (semaforo != null) semaforo.ApagarTodo();
    }
}
