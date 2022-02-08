using System.Collections;
using System;
using UnityEngine;
using CodeRun;

public class Controller : MonoBehaviour
{
    [SerializeField] CodeComponent code;

    void Start()
    {
        code.Add("Move", (CodeObject var, Action next) => StartCoroutine(Move(var, next)));
        code.Add("L", L);
        code.Add("R", R);
    }

    public IEnumerator Move(CodeObject obj, Action next)
    {
        if (obj == null || !obj.IsNumber) yield break;

        var i = 0;
        var x = (float)obj.n / 60;
        while (i < 60)
        {
            i++;
            transform.position = (Vector2)transform.position + (Vector2)transform.up * x;
            yield return null;
        }

        next();
    }

    public void L(CodeObject obj, Action next)
    {
        Debug.Log("L");
        if (obj != null) return;
        transform.Rotate(0, 0, 90);

        next();
    }

    public void R(CodeObject obj, Action next)
    {
        Debug.Log("R" + obj.String);
        if (obj != null) return;
        transform.Rotate(0, 0, -90);

        next();
    }

}
