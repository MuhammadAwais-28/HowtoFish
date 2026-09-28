using UnityEngine;
using System.Collections;

public class CharacterWanderer : MonoBehaviour
{
    public float moveSpeed = 2f;
    public Animator animator;

    private Vector3 moveDirection;

    void Start()
    {
        // Start moving forward
        moveDirection = transform.forward;
    }

    void Update()
    {
        // Move character continuously
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

        // Play walking animation
        animator.SetBool("isWalking", true);

        // Smoothly face movement direction
        if (moveDirection != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Boundary"))
        {
            // Rotate 180° when hitting boundary
            moveDirection = Quaternion.Euler(0, 180, 0) * moveDirection;
        }
    }
}
