using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class scrGameManager : MonoBehaviour
{
    public static scrGameManager Instance { get; private set; } = null;


    private static int iScore = 0;
    public static int GetScore() { return iScore; }
    public static void AddScore(int _iScore){iScore += _iScore; ScoreChanged.Invoke(); }

    public static UnityEvent ScoreChanged { get; private set; } = new UnityEvent();

    // Singleton
    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);

        iScore = 0;
        ScoreChanged = new UnityEvent();
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
