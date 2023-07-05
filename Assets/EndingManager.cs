using Proyecto26;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;

public class EndingManager : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text scoreText;
    public TMP_Text codeText;

    string code;
    int totalScore;

    void Start()
    {
        // Genero il codice
        code = GenerateCode();

        codeText.text += "\n" + code;
        scoreText.text += totalScore + " stars";

        ExperimentResults results = new ExperimentResults()
        {
            code = code,
            score = totalScore,
            totalTime = GameManager.Instance.totalExperimentTime
        };

        GameManager.Instance.SaveFinalData(results);
    }

    string GenerateCode()
    {
        totalScore = GameManager.Instance.starsCollected;
        string hexValue = totalScore.ToString("X");
        return hexValue + "_42";

    }
}


[Serializable]
public struct ExperimentResults
{
    public int score;
    public string code;
    public float totalTime;
}

