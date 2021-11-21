using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using DG.Tweening;

public class scrScoreText : MonoBehaviour
{
    private TextMeshProUGUI ScoreText;
    private int iLocalScore = 0;
    private bool bUpdateScore = false;
    private Vector3 v3BaseLocalScale;

    // Start is called before the first frame update
    void Start()
    {
        // Get text component
        ScoreText = GetComponent<TextMeshProUGUI>();

        // Set local score to game score
        iLocalScore = scrGameManager.GetScore();

        // Update score text
        ScoreText.text = "Score: " + iLocalScore.ToString();

        // Add an event listener
        scrGameManager.ScoreChanged.AddListener(UpdateScoreCallback);

        // Store base scale
        v3BaseLocalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        // See if score text needs to be updated
        if (bUpdateScore)
        {
            int iGameScore = scrGameManager.GetScore();

            // Move local score to match game score
            if (iLocalScore != iGameScore)
            {
                iLocalScore += (int)Mathf.Sign(iGameScore - iLocalScore) * Mathf.Max(1,(int)(Mathf.Abs(iGameScore - iLocalScore)*0.05f));
            }
            // If it matches, stop updating
            else
            {
                bUpdateScore = false;
            }

            // Update text, so it animates going up
            ScoreText.text = "Score: " + iLocalScore.ToString();
        }
    }

    // Listen for score changes
    private void UpdateScoreCallback()
    {
        bUpdateScore = true;

        // Tween FX
        transform.localScale = v3BaseLocalScale * 1.2f;
        transform.DOScale(v3BaseLocalScale, 0.2f);
    }
}
