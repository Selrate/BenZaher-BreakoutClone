using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class scrBall : MonoBehaviour
{
    public static scrBall Instance { get; private set; } = null;

    private Rigidbody RB;
    private MeshRenderer MainRenderer;
    private bool bOnPaddle = true;

    [SerializeField]
    private float fLaunchSpeed = 10f;

    [SerializeField]
    private float fFrictionTransfer = 0.5f;

    [SerializeField]
    public GameObject Impact, Firework, SmallImpact, Respawn, DeathFizzle;

    [SerializeField]
    private AudioSource AudioSRC;


    private void Awake()
    {
        // Singleton
        if (!Instance) Instance = this;
        else Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        RB.isKinematic = true;
        MainRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Launch
        if(Input.GetKeyDown(KeyCode.Space) && !scrGameManager.Instance.GetWonGame())
        {
            if (bOnPaddle)
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
            else
            {
                Reset();
            }
        }

        // Redundancy reset
        if (Vector3.Distance(transform.position, scrPaddle.Instance.transform.position) > 100f) Reset();
    }

    private void LateUpdate()
    {
        // While on paddle, follow it
        if(bOnPaddle)
        {
            transform.position = new Vector3(scrPaddle.Instance.transform.position.x, transform.position.y);
        }
    }

    // On collision 
    private void OnCollisionEnter(Collision collision)
    {
  
         // Paddle impact
         GameObject NewImpact = Instantiate<GameObject>(SmallImpact, collision.GetContact(0).point, Quaternion.Euler(collision.GetContact(0).normal));
         Camera.main.DOShakePosition(0.05f, RB.velocity.magnitude * 0.002f);

         // Paddle sound

         // Get a random number
         int iRandom = Mathf.Clamp(Random.Range(0, 6), 0, 5);

         // Load the corresponding bounce sound
         AudioClip FireworkClip = Resources.Load<AudioClip>("Audio/Bounce" + iRandom.ToString());

         // Play it
         AudioSRC.PlayOneShot(FireworkClip);
        
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
            // Spawn firework
            GameObject NewFirework = Instantiate<GameObject>(Firework, new Vector3(0f, -10f, 10f), Quaternion.identity);

            // Set firework color
            VisualEffect Fireworks = NewFirework.GetComponent<VisualEffect>();
            Gradient NewGradient = Fireworks.GetGradient("ColorGradient");
            GradientColorKey[] Keys = NewGradient.colorKeys;
            Keys[1].color = collision.gameObject.GetComponent<Renderer>().material.color * 255f;
            NewGradient.SetKeys(Keys, NewGradient.alphaKeys);

            Fireworks.SetGradient("ColorGradient", NewGradient);
            Fireworks.SendEvent("SpawnFirework");

            // Impact
            GameObject ImpactSpawn = Instantiate<GameObject>(Impact, transform.position - new Vector3(0f,0f,5f), Quaternion.identity);
            ImpactSpawn.GetComponent<VisualEffect>().SetVector4("Color1", collision.gameObject.GetComponent<Renderer>().material.color*255f);

            // Destroy the block
            collision.gameObject.GetComponent<scrExplode>().StartExplosion();

            // Add score
            scrGameManager.AddScore(100);

            // Shake
            Camera.main.DOShakePosition(0.1f, RB.velocity.magnitude * 0.01f);

            // Chroma
            scrGameManager.ChromaticEffect();
        }
    }

    // Trigger handling
    private void OnTriggerEnter(Collider other)
    {
        // KillZone
        if(other.gameObject.tag == "KillZone")
        {
            // Reset
            Reset();
        }
    }

    public void Reset()
    {
        // Play death fizzle
        GameObject NewFizzle = Instantiate<GameObject>(DeathFizzle, transform.position, Quaternion.identity);

        // Reset
        bOnPaddle = true;
        RB.velocity = Vector3.zero;
        RB.isKinematic = true;

        transform.position = scrPaddle.Instance.transform.position + new Vector3(0f, 1f, 0f);

        // Play death sound
        AudioSRC.PlayOneShot(Resources.Load<AudioClip>("Audio/Death"));

        // Reduce score
        scrGameManager.AddScore(-200);

        // Play effect
        GameObject NewRespawn = Instantiate<GameObject>(Respawn, transform.position, Quaternion.identity);
        NewRespawn.GetComponent<VisualEffect>().SendEvent("TriggerEffect");

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 1f).SetEase(Ease.InOutBack);
    }

    public void NewGameReset()
    {
        // Reset
        bOnPaddle = true;
        RB.velocity = Vector3.zero;
        RB.isKinematic = true;

        transform.position = scrPaddle.Instance.transform.position + new Vector3(0f, 1f, 0f);

        // Play effect
        GameObject NewRespawn = Instantiate<GameObject>(Respawn, transform.position, Quaternion.identity);
        NewRespawn.GetComponent<VisualEffect>().SendEvent("TriggerEffect");

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 1f).SetEase(Ease.InOutBack);
    }

}
