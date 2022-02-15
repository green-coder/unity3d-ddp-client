using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonGenerate : MonoBehaviour
{
    public static string actualAnswer;
    public static bool displayingQuestion = false;
    
 
    // Update is called once per frame
    void Update()
    {
         if(displayingQuestion == false){
             displayingQuestion = true;
             ButtonDisplay.newLoginButton = "Login";
             ButtonDisplay.newCreateAccountButton = "Create Account";
             actualAnswer = "A";
         }
    }
}
