using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;
using System.Linq;

public class scrBlock : NetworkBehaviour
{
    private SphereCollider Sphere;
    private Vector3 v3BaseLocalScale;
    private scrExplode ExplosionScript;

    [SyncVar]
    private bool bCanPump = true;

    [SyncVar]
    private bool bEnabled = true;

    public bool GetEnabled() { return bEnabled; }

    public void SetEnabled(bool _bEnabled) 
    { 
        bEnabled = _bEnabled;

        // Re-enabled mesh
        if(bEnabled)
        {
            GetComponent<BoxCollider>().enabled = true;
            GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public static List<scrBlock> lAllBlocks { get; private set; } = new List<scrBlock>();

    private void Awake()
    {
        lAllBlocks = lAllBlocks.Where(b => b != null).ToList();
    }

    // Start is called before the first frame update
    void Start()
    {
        Sphere = GetComponent<SphereCollider>();
        ExplosionScript = GetComponent<scrExplode>();
        v3BaseLocalScale = transform.localScale;

        // Add self to overall list
        lAllBlocks.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, v3BaseLocalScale, 0.5f);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            Sphere.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Do wave
        if (bCanPump)
        {
            bCanPump = false;
            transform.localScale = v3BaseLocalScale * 1.2f;
            StartCoroutine(CycleSphere());
        }
    }

    private IEnumerator CycleSphere()
    {
        // Cycle wave collider
        yield return new WaitForSeconds(0.02f);
        Sphere.enabled = true;
        yield return new WaitForSeconds(0.2f);
        Sphere.enabled = false;
        bCanPump = true;
    }

    private void OnDestroy()
    {
        lAllBlocks.Remove(this);
    }
}
