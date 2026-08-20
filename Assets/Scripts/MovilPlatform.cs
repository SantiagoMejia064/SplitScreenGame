using UnityEngine;

public class MovilPlatform : MonoBehaviour
{
    private Vector3 start;
    public Vector3 end;
    public float speed;
    private Rigidbody rb;

    
    public Vector3 DeltaMovimiento { get; private set; }
    private Vector3 posicionAnterior;

    void Start()
    {
        start = transform.position;
        end = end + transform.position;
        rb = GetComponent<Rigidbody>();
        posicionAnterior = transform.position;
    }

    void FixedUpdate()
    {
        Vector3 nuevaPosicion = Vector3.Lerp(start, end + start, (Mathf.Sin(speed * Time.time) + 1f) / 2);
        rb.MovePosition(nuevaPosicion);

       
        DeltaMovimiento = nuevaPosicion - posicionAnterior;
        posicionAnterior = nuevaPosicion;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (Application.isPlaying) Gizmos.DrawLine(start, end + start);
        else Gizmos.DrawLine(transform.position, end + transform.position);
    }
}