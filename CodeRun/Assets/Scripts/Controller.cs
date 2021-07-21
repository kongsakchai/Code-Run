using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeRun;

public class Controller : MonoBehaviour
{
    [SerializeField] CodeComponent code;
    void Start()
    {
        code.Add("move", (CodeObject var) => StartCoroutine(Move(var)));
    }

    public IEnumerator Move(CodeObject obj)
    {
        Debug.Log(obj.Count);
        if (obj.Count != 2) yield break;
        code.Pause();
        var i = 0;
        var x = (float)obj[0].n / 60;
        var y = (float)obj[1].n / 60;
        while (i < 60)
        {
            i++;
            transform.position = (Vector2)transform.position + new Vector2(x,y);
            yield return null;
        }
        code.Play();
    }
}
