using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeRun;

public class Controller : MonoBehaviour
{
    [SerializeField] CodeComponent code;
    void Start()
    {
        code.Add("Move", (CodeObject var) => StartCoroutine(Move(var)));
        code.Add("L", L);
        code.Add("R", R);
    }

    public IEnumerator Move(CodeObject obj)
    {
        if (obj==null || !obj.IsNumber) yield break;
        code.Stop();
        var i = 0;
        var x = (float)obj.n / 60;
        while (i < 60)
        {
            i++;
            transform.position = (Vector2)transform.position + (Vector2)transform.up * x;
            yield return null;
        }
        code.Play();
    }

    public void L(CodeObject obj)
    {
        if (obj != null) return;
        code.Stop();
        transform.Rotate(0,0,90);
        code.Play();
    }

    public void R(CodeObject obj)
    {
        if (obj != null) return;
        code.Stop();
        transform.Rotate(0,0,-90);
        code.Play();
    }

}
