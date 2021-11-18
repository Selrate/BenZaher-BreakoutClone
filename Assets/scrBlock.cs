using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class scrBlock : MonoBehaviour
{
    private SphereCollider Sphere;
    private Vector3 v3BaseLocalScale;

    // Start is called before the first frame update
    void Start()
    {
        Sphere = GetComponent<SphereCollider>();
        v3BaseLocalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            transform.localScale = v3BaseLocalScale * 1.1f;
            transform.DOScale(v3BaseLocalScale, 0.2f);
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        Sphere.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        transform.localScale = v3BaseLocalScale * 1.1f;
        transform.DOScale(v3BaseLocalScale, 0.2f);

        StartCoroutine(CycleSphere());
    }

    private IEnumerator CycleSphere()
    {
        Sphere.enabled = true;
        yield return new WaitForSeconds(0.1f);
        Sphere.enabled = false;
    }
}
