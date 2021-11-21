using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class scrFadeOut : MonoBehaviour
{
    [SerializeField]
    private float fDespawnTime = 5f;

    private void Start()
    {
        StartCoroutine(FadeDie());
    }

    private IEnumerator FadeDie()
    {
        yield return new WaitForSeconds(fDespawnTime);
        Destroy(gameObject);
    }

}
