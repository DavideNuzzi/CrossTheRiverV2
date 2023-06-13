using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrivacyController : MonoBehaviour
{
    public ScrollRect scrollView;
    public Button nextButton;
    bool readText = false;

    private void Start()
    {
        nextButton.interactable = false; 
    }

    private void Update()
    {
        if (scrollView.verticalNormalizedPosition <= 0.05f)
        {
            readText = true;
            nextButton.interactable = true;
        }
    }
    public void NextScene()
    {
        if (readText)
            GameManager.Instance.NextScene();
    }     
}
