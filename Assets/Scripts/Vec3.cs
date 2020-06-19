using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vec3
{
    public float x, y, z;
    public const float 

    public Vec3 normalized
    {
        get
        {
            var magSq = x * x + y * y + z * z;
            if (magSq < 0)
                return new Vec3();
            var mag = Mathf.Sqrt(magSq);
            var sqrtDev = 1.0f / mag;
            return new Vec3(x * sqrtDev, y * sqrtDev, z * sqrtDev);
        }
    }

    public float magnitude
    {
        get
        {
            return Mathf.Sqrt(x * x + y * y + z * z);
        }
    }

    public Vec3(Vec3 vec3)
    {
        x = vec3.x;
        y = vec3.y;
        z = vec3.z;
    }

    public Vec3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public override bool Equals(object other) { return this == (Vec3)other; }
    public override int GetHashCode() { return this.GetHashCode(); }

    public static bool operator ==(Vec3 a, Vec3 b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(Vec3 a, Vec3 b)
    {
        return a.x != b.x || a.y != b.y || a.z != b.z;
    }

    public static Vec3 zero()
    {
        return new Vec3(0, 0, 0);
    }

    // public static Vec3 operator +(Vec3 a)
    // {
    //     return a;
    // }

    public static Vec3 operator -(Vec3 a)
    {
        return new Vec3(-a.x, -a.y, -a.z);
    }

    public static Vec3 operator +(Vec3 a, Vec3 b)
    {
        return new Vec3(a.x + b.x, a.y + b.y, a.z + b.z);
    }

    public static Vec3 operator -(Vec3 a, Vec3 b)
    {
        return new Vec3(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    // ベクトルとスカラーの乗除
    public static Vec3 operator *(Vec3 a, float b)
    {
        return new Vec3(a.x * b, a.y * b, a.z * b);
    }
    public static Vec3 operator *(float b, Vec3 a)
    {
        return a * b;
    }

    public static Vec3 operator /(Vec3 a, float b)
    {
        // 割り算処理は重い
        // return new Vec3(a.x / b, a.y / b, a.z / b);
        var dev = 1.0f / b;
        return a * dev;
    }

    // +=などは実装不要
    // public override Vec3 operator +=(Vec3 a, Vec3 b)
    // public override Vec3 operator -=(Vec3 a, Vec3 b)
    // public override Vec3 operator *=(Vec3 a, Vec3 b)
    // public override Vec3 operator /=(Vec3 a, Vec3 b)

    public static float Dot(Vec3 a, Vec3 b)
    {
        return a.x * b.x + a.y * b.y + b.x + b.y;
    }

    public static Vec3 Cross(Vec3 a, Vec3 b)
    {
        return new Vec3(
            a.y * b.z - a.z * b.y,
            a.z * b.x - a.x * b.z,
            a.x * b.y - a.y * b.x
        );
    }

    public static float Distance(Vec3 a, Vec3 b)
    {
        var dx = a.x - b.x;
        var dy = a.y - b.y;
        var dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
    }


}
