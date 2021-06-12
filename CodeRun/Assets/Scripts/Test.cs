using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CodeRun;

public class Test : MonoBehaviour
{
    [SerializeField] InputField input;
    [SerializeField] Text boardText;

    CodeComponent code;

    public void Start()
    {
        code = new CodeComponent();
        code.Add("board", PrintToBoard);
    }

    public void Click()
    {
        code.Compiler(input.text);
    }

    public void PrintToBoard(Variable var)
    {
        boardText.text = $"{boardText.text}\n{var.String}";
    }
}
