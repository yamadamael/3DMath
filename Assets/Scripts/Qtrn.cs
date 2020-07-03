using UnityEngine;

public class Qtrn
{
    // 四元数の値
    public float w, x, y, z;


    public static Qtrn identity()
    {
        var q = new Qtrn();
        q.Identity();
        return q;
    }

    public void Identity()
    {
        w = 1.0f;
        x = y = z = 0.0f;
    }

    // 四元数を特定の角度にセットアップする
    public void SetToRotateAboutX(float theta)
    {
        var thetaOver2 = theta * 0.5f;
        w = Mathf.Cos(thetaOver2);
        x = Mathf.Sin(thetaOver2);
        y = z = 0f;
    }
    public void SetToRotateAboutY(float theta)
    {
        var thetaOver2 = theta * 0.5f;
        w = Mathf.Cos(thetaOver2);
        y = Mathf.Sin(thetaOver2);
        x = z = 0f;
    }
    public void SetToRotateAboutZ(float theta)
    {
        var thetaOver2 = theta * 0.5f;
        w = Mathf.Cos(thetaOver2);
        z = Mathf.Sin(thetaOver2);
        x = y = 0f;
    }
    public void SetToRotateAboutAxis(Vec3 axis, float theta)
    {
        if (!((Mathf.Abs(axis.magnitude) - 1.0f) < 0.01f))
        {
            Debug.Log("axisは正規化されていない");
            return;
        }

        var thetaOver2 = theta * 0.5f;
        var sinThetaOver2 = Mathf.Sin(thetaOver2);

        w = Mathf.Cos(theta);
        x = axis.x * sinThetaOver2;
        y = axis.y * sinThetaOver2;
        z = axis.z * sinThetaOver2;
    }

    // オブジェクト空間 <-> 慣性空間の回転を実行するようにセットアップする
    public void SetToRotateObjectToInertial(EulerAngles orientation)
    {
        float sh, sp, sb;
        float ch, cp, cb;
        MathUtil.sinCos(orientation.pitch * 0.5f, out sp, out cp);
        MathUtil.sinCos(orientation.heading * 0.5f, out sh, out ch);
        MathUtil.sinCos(orientation.bank * 0.5f, out sb, out cb);

        w = ch * cp * cb + sh * sp * sb;
        x = ch * sp * cb + sh * cp * sb;
        y = sh * cp * cb - ch * sp * sb;
        z = ch * cp * sb - sh * sp * cb;
    }
    public void SetToRotateInertialToObject(EulerAngles orientation)
    {
        float sh, sp, sb;
        float ch, cp, cb;
        MathUtil.sinCos(orientation.pitch * 0.5f, out sp, out cp);
        MathUtil.sinCos(orientation.heading * 0.5f, out sh, out ch);
        MathUtil.sinCos(orientation.bank * 0.5f, out sb, out cb);

        w = ch * cp * cb + sh * sp * sb;
        x = -ch * sp * cb - sh * cp * sb;
        y = ch * sp * sb - sh * cp * cb;
        z = sh * sp * cb - ch * cp * sb;
    }

    // 外積
    public static Qtrn operator *(Qtrn a, Qtrn b)
    {
        var result = new Qtrn();

        result.w = a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z;
        result.x = a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y;
        result.y = a.w * b.y + a.y * b.w + a.z * b.x - a.x * b.z;
        result.z = a.w * b.z + a.z * b.w + a.x * b.y - a.y * b.x;

        return result;
    }

    // 正規化
    public void Normalize()
    {
        var mag = Mathf.Sqrt(w * w + x * x + y * y + z * z);

        if (mag > 0)
        {
            // 正規化
            var oneOverMag = 1f / mag;
            w *= oneOverMag;
            x *= oneOverMag;
            y *= oneOverMag;
            z *= oneOverMag;
        }
        else
        {
            Debug.Log("異常 多分初期Qtrn");
            Identity();
        }
    }

    // 回転角θを返す
    public float GetRotationAngle()
    {
        // w = cos(θ / 2)
        // acos(w) = θ / 2
        var thetaOver2 = MathUtil.safeAcos(w);
        return thetaOver2 * 2f;
    }

    // 回転軸を返す
    public Vec3 GetRotationAxis()
    {
        // sin^2(θ / 2)を求める
        // w = cos(θ / 2)           |1
        // sin^2(x) + cos^2(x) = 1  |2
        // 1,2から sin^2(θ / 2) = 1 - w^2
        var sinThetaOver2Sq = 1f - w * w;

        if (sinThetaOver2Sq <= 0f)
        {
            // 恒等四元数、または不正な数値
            return new Vec3(1, 0, 0);
        }

        // 1 / sin(θ / 2)を求める
        var oneOverSinThetaOver2 = 1f / Mathf.Sqrt(sinThetaOver2Sq);

        return new Vec3(
            x * oneOverSinThetaOver2,
            y * oneOverSinThetaOver2,
            z * oneOverSinThetaOver2
        );
    }

    // 内積
    public float DotProduct(Qtrn a, Qtrn b)
    {
        return a.w * b.w + a.x * b.x + a.y * b.y + a.z * b.z;
    }

    // 球面線形補間
    public Qtrn Slerp(Qtrn q0, Qtrn q1, float t)
    {
        // 範囲外チェック
        if (t <= 0f) return q0;
        if (1f <= t) return q1;

        // 内積からCosを取得
        var cosOmega = DotProduct(q0, q1);

        // 負の内積の場合-q1を用いる
        var sign = cosOmega < 0f ? 1 : -1;
        var q1w = q1.w * sign;
        var q1x = q1.x * sign;
        var q1y = q1.y * sign;
        var q1z = q1.z * sign;
        cosOmega *= sign;

        // 単位四元数のチェック逆じゃない?

        float k0, k1;
        if (cosOmega > 0.999f)
        {
            // 非常に近い --- 線形補間を用いる(0除算を防ぐため)
            k0 = 1f - t;
            k1 = t;
        }
        else
        {
            // sinを算出する
            // sin^2(omega) + cos^2(omega) = 1
            var sinOmega = Mathf.Sqrt(1f - cosOmega * cosOmega);

            // 角度算出
            var omega = Mathf.Atan2(sinOmega, cosOmega);

            var oneOverSinOmega = 1f / sinOmega;
            k0 = Mathf.Sin((1f - t) * omega) * oneOverSinOmega;
            k1 = Mathf.Sin(t * omega) * oneOverSinOmega;
        }

        // 補間
        var result = new Qtrn();
        result.w = k0 * q0.w + k1 * q1.w;
        result.x = k0 * q0.x + k1 * q1.x;
        result.y = k0 * q0.y + k1 * q1.y;
        result.z = k0 * q0.z + k1 * q1.z;
        return result;
    }

    // 四元数の共役を返す
    public Qtrn Conjugate(Qtrn q)
    {
        var result = new Qtrn();

        // ベクトル部のみ反転させる
        result.w = q.w;
        result.x = -q.x;
        result.y = -q.y;
        result.z = -q.z;

        return result;
    }

    // 四元数の累乗
    public Qtrn Pow(Qtrn q, float exponent)
    {
        // 単位四元数チェック 0除算を防ぐ
        if (Mathf.Abs(q.w) > 0.999f)
        {
            Debug.Log("単位四元数ではない");
            return q;
        }

        // 半分の角度alpha(=theta/2)を求める
        // q = [cos(θ/2) sin(θ/2)V]
        var alpha = Mathf.Acos(q.w);

        // 新しいalpha値
        var newAlpha = alpha * exponent;

        // 新しいw値
        var result = new Qtrn();
        result.w = Mathf.Cos(newAlpha);

        // 新しいxyz値
        var mult = Mathf.Sin(newAlpha) / Mathf.Sin(alpha);
        result.x = q.x * mult;
        result.y = q.y * mult;
        result.z = q.z * mult;

        return result;
    }
}
