using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using TMPro;

public class Keyboard : MonoBehaviour
{
    TMP_InputField currentInputField;
    TMP_InputField lastInputField;
    TMP_InputField[] inputFields;

    private void Start()
    {
        inputFields = FindObjectsOfType<TMP_InputField>();
    }

    private void Update()
    {
        currentInputField = FindCurrentInputField();


        lastInputField = currentInputField;
    }

    public void ToggleKeyboardObject()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }

    public void A()
    {
        currentInputField.text += "a";
    }



    TMP_InputField FindCurrentInputField()
    {
        foreach(TMP_InputField ip in inputFields)
        {
            if(ip.isFocused)
            {
                return ip;
            }
        }

        Debug.Log("Vituiks men...");
        return null;
    }
}
