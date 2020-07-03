using UnityEngine;
public class EulerAngles
{
    public float heading;
    public float pitch;
    public float bank;

    public EulerAngles(float h, float p, float b)
    {
        heading = h;
        pitch = p;
        bank = b;
    }

    // 恒等オイラー角を返す
    public static EulerAngles identity()
    {
        return new EulerAngles(0, 0, 0);
    }

    // 恒等化
    public void Identity()
    {
        heading = pitch = bank = 0.0f;
    }

    // オイラー角を正準値に設定する
    public void Canonize()
    {
        // pitchを-pi~piにラップする
        pitch = MathUtil.wrapPi(pitch);

        // pitchの裏側チェック
        if (pitch < -MathUtil.kPiOver2)
        {
            pitch = -MathUtil.kPi - pitch;
            heading += MathUtil.kPi;
            bank += MathUtil.kPi;
        }
        else if (MathUtil.kPiOver2 < pitch)
        {
            pitch = MathUtil.kPi - pitch;
            heading += MathUtil.kPi;
            bank += MathUtil.kPi;
        }

        // ジンバルロックのチェックをする(許容範囲を少しもつ)
        if (Mathf.Abs(pitch) > MathUtil.kPiOver2 - 1e-4)
        {
            // ジンバルロック内にいる
            // 垂直軸に関する全ての回転をヘディングに割り当てる
            heading += bank;
            bank = 0;
        }
        else
        {
            // ジンバルロック内にいない
            // バンクを正準範囲にラップする
            bank = MathUtil.wrapPi(bank);
        }

        // ヘディングを正準範囲にラップする
        heading = MathUtil.wrapPi(heading);
    }

    // オブジェクト空間->慣性空間の回転を実行
    public void FromObjectToInertialQuaternion(Qtrn q)
    {
        // sin(pitch)を取り出す
        var sp = -2.0f * (q.y * q.z - q.w * q.x);

        // ジンバルロックチェック
        if (Mathf.Abs(sp) > 0.999f)
        {
            // 真上か真下を向いている
            pitch = MathUtil.kPiOver2 * sp;

            // ヘディングを計算し、バンクを0に設定する
            heading = Mathf.Atan2(-q.x * q.z + q.w * q.y, 0.5f - q.y * q.y - q.z * q.z);
            bank = 0f;
        }
        else
        {
            // ジンバルロックなし
            pitch = Mathf.Asin(sp);
            heading = Mathf.Atan2(q.x * q.z + q.w * q.y, 0.5f - q.x * q.x - q.y * q.y);
            bank = Mathf.Atan2(q.x * q.y + q.w * q.z, 0.5f - q.x * q.x - q.z * q.z);
        }
    }

    // 慣性空間->オブジェクト空間の回転を実行
    public void FromInertialToObjectQuaternion(Qtrn q)
    {
        // sin(pitch)を取り出す
        var sp = -2.0f * (q.y * q.z + q.w * q.x);

        // ジンバルロックチェック
        if (Mathf.Abs(sp) > 0.999f)
        {
            // 真上か真下を向いている
            pitch = MathUtil.kPiOver2 * sp;

            // ヘディングを計算し、バンクを0に設定する
            heading = Mathf.Atan2(-q.x * q.z - q.w * q.y, 0.5f - q.y * q.y - q.z * q.z);
            bank = 0f;
        }
        else
        {
            // ジンバルロックなし
            pitch = Mathf.Asin(sp);
            heading = Mathf.Atan2(q.x * q.z - q.w * q.y, 0.5f - q.y * q.y - q.z * q.z);
            bank = Mathf.Atan2(q.x * q.y - q.w * q.z, 0.5f - q.x * q.x - q.z * q.z);
        }
    }

    // オブジェクト空間からワールド空間へ座標変換を実行する
    public void FromObjectToWorldMatrix(Matrix4x3 m)
    {
        var sp = -m.m32;

        // ジンバルロックチェック
        if (Mathf.Abs(sp) > 0.999f)
        {
            // 真上か真下を向いている
            pitch = MathUtil.kPiOver2 * sp;

            // ヘディングを計算し、バンクを0に設定する
            heading = Mathf.Atan2(-m.m23, m.m11);
            bank = 0f;
        }
        else
        {
            // ジンバルロックなし
            pitch = Mathf.Asin(sp);
            heading = Mathf.Atan2(m.m31, m.m33);
            bank = Mathf.Atan2(m.m12, m.m22);
        }
    }

    // ワールド空間からオブジェクト空間へ座標変換を実行する
    public void FromWorldToObjectMatrix(Matrix4x3 m)
    {
        var sp = -m.m23;

        // ジンバルロックチェック
        if (Mathf.Abs(sp) > 0.999f)
        {
            // 真上か真下を向いている
            pitch = MathUtil.kPiOver2 * sp;

            // ヘディングを計算し、バンクを0に設定する
            heading = Mathf.Atan2(-m.m31, m.m11);
            bank = 0f;
        }
        else
        {
            // ジンバルロックなし
            pitch = Mathf.Asin(sp);
            heading = Mathf.Atan2(m.m13, m.m33);
            bank = Mathf.Atan2(m.m21, m.m22);
        }
    }

    // 回転行列をオイラー角に変換する
    // FromWorldToObjectMatrix()と同じ
    public void FromRotationMatrix(RotationMatrix m)
    {
        var sp = -m.m23;

        // ジンバルロックチェック
        if (Mathf.Abs(sp) > 0.999f)
        {
            // 真上か真下を向いている
            pitch = MathUtil.kPiOver2 * sp;

            // ヘディングを計算し、バンクを0に設定する
            heading = Mathf.Atan2(-m.m31, m.m11);
            bank = 0f;
        }
        else
        {
            // ジンバルロックなし
            pitch = Mathf.Asin(sp);
            heading = Mathf.Atan2(m.m13, m.m33);
            bank = Mathf.Atan2(m.m21, m.m22);
        }
    }
}