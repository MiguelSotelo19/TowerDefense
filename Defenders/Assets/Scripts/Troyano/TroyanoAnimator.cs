using UnityEngine;

public class TroyanoAnimator : MonoBehaviour
{
    public Transform A;
    public Transform B;
    public Transform C;

    public Vector3 posA_Unido = new Vector3(0,0,0);
    public Vector3 posB_Unido = new Vector3(0.3f,0,0);
    public Vector3 posC_Unido = new Vector3(-0.3f,0,0);

    public Vector3 posA_Separado = new Vector3(-2,0,0);
    public Vector3 posB_Separado = new Vector3(0,0,0);
    public Vector3 posC_Separado = new Vector3(2,0,0);

    public float speed = 2f;

    bool dividido = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            dividido = !dividido; // alternar
        }

        if (dividido)
        {
            // Separarse
            A.localPosition = Vector3.Lerp(A.localPosition, posA_Separado, Time.deltaTime * speed);
            B.localPosition = Vector3.Lerp(B.localPosition, posB_Separado, Time.deltaTime * speed);
            C.localPosition = Vector3.Lerp(C.localPosition, posC_Separado, Time.deltaTime * speed);
        }
        else
        {
            // Reunirse
            A.localPosition = Vector3.Lerp(A.localPosition, posA_Unido, Time.deltaTime * speed);
            B.localPosition = Vector3.Lerp(B.localPosition, posB_Unido, Time.deltaTime * speed);
            C.localPosition = Vector3.Lerp(C.localPosition, posC_Unido, Time.deltaTime * speed);
        }
    }
}
