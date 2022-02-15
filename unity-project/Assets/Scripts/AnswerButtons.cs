using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButtons : MonoBehaviour
{

    public GameObject answerLoginButton;
    public GameObject answerLogoutButton;
    public GameObject usernameInput;
    public GameObject passwordInput;

    public void LoginButtonFunction()
    {
        if (ButtonGenerate.actualAnswer  == "A")
        {
            usernameInput.SetActive(false);
            passwordInput.SetActive(false);
            answerLoginButton.SetActive(false);
            answerLogoutButton.SetActive(true);
        }
    }

    public void LogoutButtonFunction()
    {
        if (ButtonGenerate.actualAnswer  == "A")
        {
            usernameInput.SetActive(true);
            passwordInput.SetActive(true);
            answerLoginButton.SetActive(true);
            answerLogoutButton.SetActive(false);
        }
    }

}
