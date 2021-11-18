using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class scrScoreText : MonoBehaviour
{
    private TextMeshProUGUI ScoreText;
    private int iLocalScore = 0;
    private bool bUpdateScore = false;

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
                iLocalScore += (int)Mathf.Sign(iGameScore - iLocalScore);
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
    }
}
