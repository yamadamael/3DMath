using UnityEngine;

public class MathUtil
{
    const float kPi = 3.14159265f;
    const float k2Pi = kPi * 2.0f;
    const float kPiOver2 = kPi / 2.0f;
    const float k1OverPi = 1.0f / kPi;
    const float k1Over2Pi = 1.0f / k2Pi;

    public float wrapPi(float theta)
    {
        theta += kPi;
        theta -= Mathf.Floor(theta * k1Over2Pi) * k2Pi;
        theta -= kPi;
        return theta;
    }

    public float safeAcos(float x)
    {
        if (1 <= x)
        {
            return 0;
        }
        else if (x <= -1)
        {
            return kPi;
        }
        return Mathf.Acos(x);
    }

    public void sinCos(float theta, out float returnSin, out float returnCos)
    {
        returnSin = Mathf.Sin(theta);
        returnCos = Mathf.Cos(theta);
    }
}