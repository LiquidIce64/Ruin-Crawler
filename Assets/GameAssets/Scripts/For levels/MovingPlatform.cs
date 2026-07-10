using UnityEngine;

public class MovingPlatformZ : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    private float speed = 12f;
    private float waitTime = 3f;

    private Rigidbody rb;
    private Vector3 targetPosition;
    private bool isWaiting;
    private float waitTimer;
    public bool IsMoving => !isWaiting;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        targetPosition = pointB.position;
    }

    private void FixedUpdate()
    {
        if (isWaiting)
        {
            waitTimer += Time.fixedDeltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                targetPosition = targetPosition == pointA.position ? pointB.position : pointA.position;
            }
            return;
        }

        Vector3 newPos = Vector3.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector3.Distance(rb.position, targetPosition) < 0.01f)
        {
            isWaiting = true;
            waitTimer = 0f;
        }
    }
}