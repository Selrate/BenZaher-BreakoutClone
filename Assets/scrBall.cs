using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.VFX;
using Mirror;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class scrBall : NetworkBehaviour
{
    public static List<scrBall> Balls { get; private set; } = new List<scrBall>();

    private Rigidbody RB;
    private MeshRenderer MainRenderer;

    [SyncVar]
    private bool bOnPaddle = true;
    public bool GetOnPaddle() { return bOnPaddle; }

    [SerializeField]
    private float fLaunchSpeed = 10f;

    [SerializeField]
    private float fFrictionTransfer = 0.5f;

    [SerializeField]
    public GameObject Impact, Firework, SmallImpact, Respawn, DeathFizzle;

    [SerializeField]
    private AudioSource AudioSRC;

    private scrPaddle Paddle;
    public void SetPaddle(scrPaddle _paddle) { Paddle = _paddle; }

    private bool bHasPaddle = false;

    private void Awake()
    {
        // Add self to list of balls
        Balls.Add(this);

        // Make sure there are no nulls
        Balls = Balls.Where(b => b != null).ToList();
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
        // If had a paddle before, and now doesn't, player must have disconnected. Destroy the ball.
        if (bHasPaddle && !Paddle) Destroy(gameObject);

        // Make sure the ball has a paddle
        if (!Paddle) Paddle = scrPaddle.Paddles.OrderBy(p => Vector3.Distance(transform.position, p.transform.position)).FirstOrDefault();

        // Make sure paddle's ball ref is this (for client syncing)
        if (Paddle.GetBall() == null) Paddle.SetBall(this);

        // Flag as having set paddle
        bHasPaddle = true;

        // Redundancy reset
        if (Vector3.Distance(transform.position, Paddle.transform.position) > 100f) Reset();
    }

    [Command(requiresAuthority = false)]
    public void Launch()
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

    private void LateUpdate()
    {
        if (!Paddle) return;

        // While on paddle, follow it
        if (bOnPaddle && Paddle)
        {
            transform.position = new Vector3(Paddle.transform.position.x, transform.position.y);
        }
    }

    // On collision 
    private void OnCollisionEnter(Collision collision)
    {
        if (!isServer) return;
        if (!Paddle) return;

        // Paddle impact
        GameObject NewImpact = Instantiate<GameObject>(SmallImpact, collision.GetContact(0).point, Quaternion.Euler(collision.GetContact(0).normal));
        NetworkServer.Spawn(NewImpact);
        Camera.main.DOShakePosition(0.05f, RB.velocity.magnitude * 0.002f);

        // Get a random number
        int iRandom = Mathf.Clamp(Random.Range(0, 6), 0, 5);

        // Play clip
        PlayClip("Audio/Bounce" + iRandom.ToString());    
    }

    // After colliding
    private void OnCollisionExit(Collision collision)
    {
        if (!isServer) return;
        if (!Paddle) return;

        // Get velocity from paddle
        if (collision.collider.tag == "Paddle")
        {
            // Store velocity
            Vector3 v3NewVelocity = RB.velocity;

            // Add vecloty of paddle
            v3NewVelocity.x += Paddle.GetRigidbody().velocity.x * fFrictionTransfer;

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

            NetworkServer.Spawn(NewFirework);

            // Impact
            GameObject ImpactSpawn = Instantiate<GameObject>(Impact, transform.position - new Vector3(0f,0f,5f), Quaternion.identity);
            ImpactSpawn.GetComponent<VisualEffect>().SetVector4("Color1", collision.gameObject.GetComponent<Renderer>().material.color*255f);
            NetworkServer.Spawn(ImpactSpawn);

            // Destroy the block
            Debug.Log("BlockCall!");
            collision.gameObject.GetComponent<scrExplode>().StartExplosion();

            // Add score
            scrGameManager.AddScore(100);

            // Shake
            Camera.main.DOShakePosition(0.1f, RB.velocity.magnitude * 0.01f);

            // Chroma
            scrGameManager.Instance.ChromaticEffect();
        }
    }

 

    // Trigger handling
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (!Paddle) return;

        // KillZone
        if (other.gameObject.tag == "KillZone")
        {
            // Reset
            Reset();
        }
    }

    [Command(requiresAuthority = false)]
    public void Reset()
    {
        if (!isServer) return;
        if (!Paddle) return;

        // Play death fizzle
        GameObject NewFizzle = Instantiate<GameObject>(DeathFizzle, transform.position, Quaternion.identity);
        NetworkServer.Spawn(NewFizzle);

        // Reset
        bOnPaddle = true;
        RB.velocity = Vector3.zero;
        RB.isKinematic = true;

        transform.position = Paddle.transform.position + new Vector3(0f, 1f, 0f);

        // Play death sound
        PlayClip("Audio/Death");

        // Reduce score
        scrGameManager.AddScore(-200);

        // Play effect
        GameObject NewRespawn = Instantiate<GameObject>(Respawn, transform.position, Quaternion.identity);
        NetworkServer.Spawn(NewRespawn);
        NewRespawn.GetComponent<VisualEffect>().SendEvent("TriggerEffect");

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 1f).SetEase(Ease.InOutBack);
    }

    [ClientRpc]
    void PlayClip(string _sClipPath)
    {
        AudioSRC.PlayOneShot(Resources.Load<AudioClip>(_sClipPath));
    }

    public void NewGameReset()
    {
        if (!isServer) return;
        if (!Paddle) return;

        // Reset
        bOnPaddle = true;
        RB.velocity = Vector3.zero;
        RB.isKinematic = true;

        transform.position = Paddle.transform.position + new Vector3(0f, 1f, 0f);

        // Play effect
        GameObject NewRespawn = Instantiate<GameObject>(Respawn, transform.position, Quaternion.identity);
        NewRespawn.GetComponent<VisualEffect>().SendEvent("TriggerEffect");
        NetworkServer.Spawn(NewRespawn);

        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 1f).SetEase(Ease.InOutBack);
    }

}
