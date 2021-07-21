using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeRun;

public class Test : MonoBehaviour
{
    [SerializeField] CodeComponent code;
    [SerializeField] InputField inputText;

    void Start()
    {

    }


    void Update()
    {
    }

    public void RunCode()
    {
        //if(code.active)return;
        code.Compiler(inputText.text);
    }

}
