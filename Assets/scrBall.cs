using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class scrBall : MonoBehaviour
{
    private Rigidbody RB;

    private bool bOnPaddle = true;

    [SerializeField]
    private float fLaunchSpeed = 10f;

    [SerializeField]
    private float fFrictionTransfer = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        RB.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Launch
        if(bOnPaddle && Input.GetKeyDown(KeyCode.Space))
        {
            bOnPaddle = false;

            // Set to not kinematic so it can move
            RB.isKinematic = false;
            
            // Generate an angle
            float fAngle = Mathf.Deg2Rad * Random.Range(-45f, 45f);

            // Generate a launch vector from the angle
            Vector3 v3LaunchDir = new Vector3(Mathf.Sin(fAngle), Mathf.Cos(fAngle));

            // Set the ball's launch speed
            RB.AddForce(v3LaunchDir * fLaunchSpeed);
        }
    }

    private void LateUpdate()
    {
        // While on paddle, follow it
        if(bOnPaddle)
        {
            transform.position = new Vector3(scrPaddle.Instance.transform.position.x, transform.position.y);
        }
    }

    // After colliding
    private void OnCollisionExit(Collision collision)
    {
        // Get velocity from paddle
        if (collision.collider.tag == "Paddle")
        {
            // Store velocity
            Vector3 v3NewVelocity = RB.velocity;

            // Add vecloty of paddle
            v3NewVelocity.x += scrPaddle.Instance.GetRigidbody().velocity.x * fFrictionTransfer;

            // Update velocity
            RB.velocity = v3NewVelocity;
        }

        // Block collision
        if(collision.collider.tag == "Block")
        {
            // Destroy the block
            Destroy(collision.gameObject);

            // Add score
            scrGameManager.AddScore(100);
        }
    }

    // Trigger handling
    private void OnTriggerEnter(Collider other)
    {
        // KillZone
        if(other.gameObject.tag == "KillZone")
        {
            // Reset
            bOnPaddle = true;
            RB.velocity = Vector3.zero;
            RB.isKinematic = true;

            transform.position = scrPaddle.Instance.transform.position + new Vector3(0f, 1f, 0f);

            // Reduce score
            scrGameManager.AddScore(-200);
        }
    }
}
