using UnityEngine;

namespace Assets.Minijuegos.Scripts
{
    public class PaddleController : MonoBehaviour
    {
        [SerializeField]
        private float moveSpeed;

        Rigidbody2D rb;


        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if(Input.GetMouseButton(0))
            {
                Vector2 touchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                if(touchPosition.x < 0) // Si la posición del toque es menor a 0 (izquierda)
                {
                    rb.velocity = Vector2.left * moveSpeed;
                }
                else // Si la posición del toque es mayor a 0 (derecha)
                {
                    rb.velocity = Vector2.right * moveSpeed;
                }
            }
            else if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if(touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                {
                    Vector2 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);

                    if(touchPosition.x < 0) // Si la posición del toque es menor a 0 (izquierda)
                    {
                        rb.velocity = Vector2.left * moveSpeed;
                    }
                    else // Si la posición del toque es mayor a 0 (derecha)
                    {
                        rb.velocity = Vector2.right * moveSpeed;
                    }
                }
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }

    }
}