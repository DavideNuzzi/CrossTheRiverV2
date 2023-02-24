using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIStart : MonoBehaviour
{
    public TMP_InputField firstTextfield;
    public TMP_InputField secondTextfield;
    public TMP_Dropdown genderDropdown;

    public TMP_Text firstWarning;
    public TMP_Text secondWarning;
    public TMP_Text thirdWarning;

    public Button playButton;

    bool firstFieldGood;
    bool secondFieldGood;
    bool genderDropdownGood;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        firstFieldGood = CheckFirstField();
        secondFieldGood = CheckSecondField();
        genderDropdownGood = CheckGender();

        if (firstFieldGood && secondFieldGood && genderDropdownGood)
        {
            playButton.interactable = true;
        }
        else
        {
            playButton.interactable = false;
        }
    }

    bool CheckFirstField()
    {
        if (firstTextfield.text.Length > 0)
        {
            firstWarning.gameObject.SetActive(false);
            return true;
        }
        else
        {
            firstWarning.text = "Name can't be empty";
            firstWarning.gameObject.SetActive(true);
            return false;
        }
    }

    bool CheckSecondField()
    {
        int age;

        if (secondTextfield.text.Length == 0)
        {
            secondWarning.text = "Can't be empty";
            secondWarning.gameObject.SetActive(true);
            return false;
        }
        else if (!int.TryParse(secondTextfield.text, out age))
        {
            secondWarning.text = "Number format is invalid";
            secondWarning.gameObject.SetActive(true);
            return false;
        }
        else if (age < 18 || age > 100)
        {
            secondWarning.text = "Age should be in the range (18,100)";
            secondWarning.gameObject.SetActive(true);
            return false;
        }
        else
        {
            secondWarning.gameObject.SetActive(false);
            return true;

        }
        /*
        if (secondTextfield.text.Length > 0 && int.TryParse)
        {
            secondWarning.gameObject.SetActive(false);
            return true;
        }
        else
        {
            secondWarning.text = "Invalid ID";
            secondWarning.gameObject.SetActive(true);
            return false;
        }
        */
    }

    bool CheckGender()
    {
        if (genderDropdown.value == 0)
        {
            thirdWarning.text = "Please select a valid option";
            thirdWarning.gameObject.SetActive(true);
            return false;
        }
        else
        {
            thirdWarning.gameObject.SetActive(false);
            return true;
        }
    }


    public void StartGame()
    {
        string gender = "";
        if (genderDropdown.value == 1) gender = "Male";
        if (genderDropdown.value == 2) gender = "Female";
        if (genderDropdown.value == 3) gender = "Other";
        if (genderDropdown.value == 3) gender = "Undisclosed";

        GameManager.Instance.StartGame(firstTextfield.text, secondTextfield.text, gender);
        // StartCoroutine(GameManager.Instance.StartGame(firstTextfield.text, secondTextfield.text));
    }
}
