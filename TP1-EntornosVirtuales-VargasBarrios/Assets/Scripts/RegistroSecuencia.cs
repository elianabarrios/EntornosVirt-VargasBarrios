using System.Collections.Generic;
using UnityEngine;

public class RegistroSecuencia : MonoBehaviour
{
    // Lista para guardar los clores
    public List<string> coloresPulsados = new List<string>();

    public void PulsarRojo()
    {
        coloresPulsados.Add("Rojo");
        Debug.Log("Pulsó rojo");
        MostrarConteo();
    }

    public void PulsarAmarillo()
    {
        coloresPulsados.Add("Amarillo");
        Debug.Log("Pulsó amarillo");
        MostrarConteo();
    }

    public void PulsarVerde()
    {
        coloresPulsados.Add("Verde");
        Debug.Log("Pulsó verde");
        MostrarConteo();
    }

    // Para mostrar el count
    void MostrarConteo()
    {
        Debug.Log("Pulsaciones realizadas: " + coloresPulsados.Count);
    }

    // Reinicio 
    public void ReiniciarSecuencia()
    {
        coloresPulsados.Clear();
        Debug.Log("Conteo reiniciado");
    }
}
