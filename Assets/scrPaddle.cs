using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public class scrPaddle : NetworkBehaviour
{
    public static List<scrPaddle> Paddles { get; private set; } = new List<scrPaddle>();

    [SerializeField]
    private float fSpeed = 5f;

    [SerializeField]
    private GameObject BallPrefab;

    private scrBall Ball = null;
    public void SetBall(scrBall _ball) { Ball = _ball; }
    public scrBall GetBall() { return Ball; }

    private Rigidbody RB;
    public Rigidbody GetRigidbody() { return RB; }

    private Vector3 v3BaseLocalScale;
    private Vector3 v3SquishedLocalScale;


    private void Awake()
    {
        // Add self to list of paddles
        Paddles.Add(this);

        // Make sure there are no nulls
        Paddles = Paddles.Where(b => b != null).ToList();
    }

    // Start is called before the first frame update
    void Start()
    {
        RB = GetComponent<Rigidbody>();
        v3BaseLocalScale = transform.localScale;

        // Set up squished scale
        v3SquishedLocalScale = v3BaseLocalScale;
        v3SquishedLocalScale.x *= 0.9f;
        v3SquishedLocalScale.y *= 1.1f;

        if (!isServer) return;

        // Make ball
        GameObject NewBall = Instantiate<GameObject>(BallPrefab, transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity);
        NetworkServer.Spawn(NewBall);
        Ball = NewBall.GetComponent<scrBall>();

        // Set ball's paddle to this
        Ball.SetPaddle(this);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isLocalPlayer) return;

        // Launch ball
        if (Input.GetKeyDown(KeyCode.Space) && !scrGameManager.Instance.GetWonGame())
        {
            if (Ball.GetOnPaddle())
            {
                Ball.Launch();
            }
            else
            {
                Ball.Reset();
            }
        }
    }

    // Physics updates
    void FixedUpdate()
    {
        // Return if not local client
        if (!isLocalPlayer) return;

        // Init new velocity to zero
        Vector3 v3Velocity = Vector3.zero;

        // Add left velocity on key press
        if(Input.GetKey(KeyCode.LeftArrow))
        {
            v3Velocity += new Vector3(-fSpeed, 0f);
        }

        // Add right velocity on key press
        if(Input.GetKey(KeyCode.RightArrow))
        {
            v3Velocity += new Vector3(fSpeed, 0f);
        }

        // Set velocity
        RB.velocity = v3Velocity;

        // squish
        if(v3Velocity == Vector3.zero)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, v3BaseLocalScale, 0.5f);
        }
        else
        {
            transform.localScale = Vector3.Lerp(transform.localScale, v3SquishedLocalScale, 0.5f);
        }
    }

    private void OnDestroy()
    {
        // Failsafe
        if (Ball) Destroy(Ball.gameObject);
    }
}
