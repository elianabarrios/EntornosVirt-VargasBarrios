using UnityEngine;

public class PuertaPuzzle : MonoBehaviour
{
    public Animator animatorPuerta;
    public string triggerAbrir = "Abrir";
    public BichitoLogica bichito;

    public bool Abierta { get; private set; }

    //No olvidar conectarlo a registro
    public void Abrir()
    {
        if (Abierta) return;
        Abierta = true;

        if (animatorPuerta != null)
        {
            animatorPuerta.SetTrigger(triggerAbrir);
        }
        else
        {
            Debug.Log("Desbloqueada");
        }
    }

    public void MostrarBichito()
    {
        if (bichito != null)
        {
            bichito.Aparecer();
        }
    }
}
