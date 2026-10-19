using System.Collections;
using UnityEngine;

public class BichitoLogica : MonoBehaviour
{
    public AudioSource sonidoPasos;
    public Animator animatorBichito;

    public Transform puntoOculto;
    public Transform puntoAsomado;

    public float velocidad = 1f;
    public float tiempoQuieto = 2f;

    private bool aparecio = false;


    void Start()
    {
        transform.position = puntoOculto.position;
        animatorBichito.gameObject.SetActive(false);
    }

    public void Aparecer()
    {
        if (aparecio) return;

        aparecio = true;
        StartCoroutine(MostrarBichito());
    }

    IEnumerator MostrarBichito()
    {
        animatorBichito.gameObject.SetActive(true);

        animatorBichito.Play("Caminar");
        yield return MoverHasta(puntoAsomado.position);

        animatorBichito.Play("Idle");
        yield return new WaitForSeconds(tiempoQuieto);

        animatorBichito.Play("Caminar");
        yield return MoverHasta(puntoOculto.position);

        animatorBichito.gameObject.SetActive(false);
    }

    IEnumerator MoverHasta(Vector3 destino)
    {
        sonidoPasos.Play();

        while (Vector3.Distance(transform.position, destino) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                destino,
                velocidad * Time.deltaTime
            );

            yield return null;
        }

        transform.position = destino;

        sonidoPasos.Stop();
    }


}
