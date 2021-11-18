using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class scrPaddle : MonoBehaviour
{
    public static scrPaddle Instance { get; private set; } = null;


    [SerializeField]
    private float fSpeed = 5f;

    private Rigidbody RB;
    public Rigidbody GetRigidbody() { return RB; }

    private Vector3 v3BaseLocalScale;
    private Vector3 v3SquishedLocalScale;

    // Singleton
    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);
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
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    // Physics updates
    void FixedUpdate()
    {
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
}
