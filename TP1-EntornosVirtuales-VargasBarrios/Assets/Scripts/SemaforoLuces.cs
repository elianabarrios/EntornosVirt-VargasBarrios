using System.Collections;
using UnityEngine;

public class SemaforoLuces : MonoBehaviour
{
    [Header("Rojo")]
    public Renderer luzRoja;
    public Material rojoOff;
    public Material rojoOn;

    [Header("Amarillo")]
    public Renderer luzAmarilla;
    public Material amarilloOff;
    public Material amarilloOn;

    [Header("Verde")]
    public Renderer luzVerde;
    public Material verdeOff;
    public Material verdeOn;

    [Header("Tiempos")]
    public float tiempoEncendida = 0.8f;
    public float pausaEntreLuces = 0.3f;

    [Header("Prueba")]
    public bool probarAlIniciar;
    // 0 es rojo - 1 amarillo - 2 verde
    public int[] secuenciaPrueba = { 0, 1, 2, 0 };

    public bool EstaReproduciendo;
    bool listo;

    void Awake()
    {
        listo = luzRoja != null && luzAmarilla != null && luzVerde != null
            && rojoOff != null && rojoOn != null
            && amarilloOff != null && amarilloOn != null
            && verdeOff != null && verdeOn != null;

        if (!listo)
        {
            Debug.LogError("El semaforo no tiene material.", this);
            enabled = false;
            return;
        }

        ApagarTodo();
    }

    void Start()
    {
        if (probarAlIniciar) ProbarSecuencia();
    }

    // Primero apaga las tres luces y despues prede la elegida
    void CambiarColor(int color)
    {
        luzRoja.sharedMaterial = rojoOff;
        luzAmarilla.sharedMaterial = amarilloOff;
        luzVerde.sharedMaterial = verdeOff;

        if (color == 0) luzRoja.sharedMaterial = rojoOn;
        if (color == 1) luzAmarilla.sharedMaterial = amarilloOn;
        if (color == 2) luzVerde.sharedMaterial = verdeOn;
    }

    public void ApagarTodo()
    {
        StopAllCoroutines();
        EstaReproduciendo = false;
        if (listo) CambiarColor(-1);
    }

    public void EncenderRojo() { EncenderColor(0); }
    public void EncenderAmarillo() { EncenderColor(1); }
    public void EncenderVerde() { EncenderColor(2); }

    public void EncenderTodo()
    {
        if (!listo || !isActiveAndEnabled) return;

        ApagarTodo();

        luzRoja.sharedMaterial = rojoOn;
        luzAmarilla.sharedMaterial = amarilloOn;
        luzVerde.sharedMaterial = verdeOn;
    }

    void EncenderColor(int color)
    {
        if (!listo || !isActiveAndEnabled) return;
        ApagarTodo();
        CambiarColor(color);
    }

    public void ProbarSecuencia()
    {
        MostrarSecuencia(secuenciaPrueba);
    }

    public void MostrarSecuencia(int[] colores)
    {
        if (!listo || !isActiveAndEnabled) return;
        ApagarTodo();
        if (colores == null || colores.Length == 0) return;

        for (int i = 0; i < colores.Length; i++)
        {
            if (colores[i] < 0 || colores[i] > 2)
            {
                Debug.LogError("La secuencia debería ser 0, 1 o 2.", this);
                return;
            }
        }

        StartCoroutine(Reproducir(colores));
    }

    IEnumerator Reproducir(int[] colores)
    {
        EstaReproduciendo = true;
        yield return new WaitForSeconds(pausaEntreLuces);

        for (int i = 0; i < colores.Length; i++)
        {
            CambiarColor(colores[i]);
            yield return new WaitForSeconds(tiempoEncendida);
            CambiarColor(-1);
            yield return new WaitForSeconds(pausaEntreLuces);
        }

        EstaReproduciendo = false;
    }

    void OnDisable()
    {
        ApagarTodo();
    }
}
