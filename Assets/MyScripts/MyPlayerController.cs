using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public class MyPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Joystick Reference (optional)")]
    public FixedJoystick joyStick; // leave empty if using keyboard

    [Header("Jump Button Reference")]
    public Button jumpButton;

    [Header("Respawn Settings")]
    public float waterLevelY = 11.03f;
    public Vector3 respawnPosition = new Vector3(507f, 12.16f, 499.76f);
    

    private Rigidbody rb;
    private bool isJumping = false;
    private bool isGrounded = true;
    private bool hasFallen = false;

    [Header("Table UI References")]
    public GameObject mobilePanel;
    public GameObject boatOarPanel;


    [Header("Underwater Effect")]
    public GameObject underwaterOverlay;
    public GameObject RespawnPanel;
    public GameObject respawnButton;
    public GameObject quitButton;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        //if (respawnButton != null)
        //{
        //    respawnButton.SetActive(false);
        //}

        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(JumpButtonPressed);
        }
    }

    void Update()
    {
        float horizontal = joyStick != null ? joyStick.Horizontal : Input.GetAxis("Horizontal");
        float vertical = joyStick != null ? joyStick.Vertical : Input.GetAxis("Vertical");

        // Calculate direction based on the player's current forward orientation updated by the camera
        Vector3 moveDir = (transform.right * horizontal) + (transform.forward * vertical);
        Vector3 moveVelocity = moveDir.normalized * moveSpeed;

        rb.linearVelocity = new Vector3(moveVelocity.x, rb.linearVelocity.y, moveVelocity.z);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        CheckFall();
        UpdateJumpButtonState();
    }

    public void Jump()
    {
        if (isGrounded && !isJumping)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = true;
            isGrounded = false;
        }
    }

    public void JumpButtonPressed()
    {
        Jump();
    }

    private void UpdateJumpButtonState()
    {
        if (jumpButton != null)
        {
            jumpButton.interactable = isGrounded;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isJumping = false;
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

   /* private void CheckFall()
    {
        if (!hasFallen && transform.position.y < waterLevelY)
        {
            hasFallen = true;

            if (respawnButton != null)
            {
                respawnButton.SetActive(true);
            }
        }
    }*/

   /* public void RespawnPlayer()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = respawnPosition;

        hasFallen = false;
        isJumping = false;
        isGrounded = true;

        if (respawnButton != null)
        {
            respawnButton.SetActive(false);
        }
    } */

    // Code for Table UI panels 
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Phone"))
        {
            if (mobilePanel != null) mobilePanel.SetActive(true);
        }
        else if (other.CompareTag("BoatOar"))
        {
            if (boatOarPanel != null) boatOarPanel.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Phone"))
        {
            if (mobilePanel != null) mobilePanel.SetActive(false);
        }
        else if (other.CompareTag("BoatOar"))
        {
            if (boatOarPanel != null) boatOarPanel.SetActive(false);
        }
    }

    private void CheckFall()
    {
        if (!hasFallen && transform.position.y < waterLevelY)
        {
            hasFallen = true;

            if (RespawnPanel != null)
            {
                RespawnPanel.SetActive(true);
            }
        }
    }

    public void RespawnPlayer()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = respawnPosition;

        hasFallen = false;
        isJumping = false;
        isGrounded = true;

        if (RespawnPanel != null)
        {
            RespawnPanel.SetActive(false);
        }
    }

    public void QuitGame()
    {
        
        Application.Quit();
    }

}
