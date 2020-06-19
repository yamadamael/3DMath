using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    Vec3 a;
    Vec3 b;
    float c;

    // Start is called before the first frame update
    void Start()
    {



        Debug.Log(new Vector3().normalized);

        a = new Vec3(10000, 10000, 10000);
        b = new Vec3(1, 1, 1);
        c = 2;

        var v1 = a * 3;
        var v2 = 3 * a;

        Debug.Log("start end");
    }

    // Update is called once per frame
    void Update()
    {
        a += b;
        log();
    }

    void log()
    {
        Debug.Log(string.Format($"a=({a.x},{a.y},{a.z}), b=({b.x},{b.y},{b.z})", a, b));
    }
}
