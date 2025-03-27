using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class VerticalScrollController : MonoBehaviour
{
    [Header("Configuración Movimiento")]
    public float sensibilidad = 3f;
    public float suavizado = 0.1f;
    public float minY = -5f;
    public float maxY = 5f;

    [Header("Referencias")]
    private Vector2 touchStart;
    private float targetY;
    private float velocidadY;

    void Start()
    {
        targetY = transform.position.y;
    }

    void Update()
    {
        ManejarTouch();
        AplicarMovimiento();
    }

    void ManejarTouch()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStart = touch.position;
                targetY = transform.position.y;
                velocidadY = 0;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                Vector2 diferencia = touch.position - touchStart;
                targetY += diferencia.y * sensibilidad * Time.deltaTime;
                targetY = Mathf.Clamp(targetY, minY, maxY);
                touchStart = touch.position;
            }
        }
    }

    void AplicarMovimiento()
    {
        float nuevaY = Mathf.SmoothDamp(
            transform.position.y,
            targetY,
            ref velocidadY,
            suavizado
        );

        transform.position = new Vector3(
            transform.position.x,
            nuevaY,
            transform.position.z
        );
    }
}