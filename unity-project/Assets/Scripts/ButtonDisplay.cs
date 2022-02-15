using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonDisplay : MonoBehaviour
{
    public GameObject loginButton;
    public GameObject createAccountButton;
    public static string newLoginButton;
    public static string newCreateAccountButton;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PushTextOnScreen());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator PushTextOnScreen()
    {
        yield return new WaitForSeconds(0.25f);
        loginButton.GetComponent<Text>().text = newLoginButton;
        createAccountButton.GetComponent<Text>().text = newCreateAccountButton;
    }
}
