
public class RotationMatrix
{
    public float m11, m12, m13;
    public float m21, m22, m23;
    public float m31, m32, m33;


    // 慣性行列を設定
    public void Identity()
    {
        m11 = 1f; m12 = 0f; m13 = 0f;
        m21 = 0f; m22 = 1f; m23 = 0f;
        m31 = 0f; m32 = 0f; m33 = 1f;
    }

    // 特定の向きを持つ行列をセットアップする
    public void Setup(EulerAngles orientation)
    {
        float sh, sp, sb, ch, cp, cb;
        MathUtil.sinCos(orientation.heading, out sh, out ch);
        MathUtil.sinCos(orientation.pitch, out sp, out cp);
        MathUtil.sinCos(orientation.bank, out sb, out cb);

        m11 = ch * cb + sh * sp * sb;
        m12 = -ch * sb + sh * sp * cb;
        m13 = sh * cp;

        m21 = sb * cp;
        m22 = cb * cp;
        m23 = -sp;

        m31 = -sh * cb + ch * sp * sb;
        m32 = sb * sh + ch * sp * cb;
        m33 = ch * cp;
    }

    // 四元数から行列をセットアップする
    // 引数の四元数は指定された座標変換の向きで回転を実行するものとする
    public void FromInertialToObjectQuaternion(Qtrn q)
    {
        m11 = 1f - 2f * (q.y * q.y - q.z * q.z);
        m12 = 2f * (q.x * q.y + q.w * q.z);
        m13 = 2f * (q.x * q.z - q.w * q.y);

        m21 = 2f * (q.x * q.y - q.w * q.z);
        m22 = 1f - 2f * (q.x * q.x - q.z * q.z);
        m23 = 2f * (q.y * q.z + q.w * q.x);

        m31 = 2f * (q.x * q.z + q.w * q.y);
        m32 = 2f * (q.y * q.z - q.w * q.x);
        m33 = 1f - 2 * (q.x * q.x - q.y * q.y);
    }

    public void FromObjectToInertialQuaternion(Qtrn q)
    {
        m11 = 1f - 2f * (q.y * q.y - q.z * q.z);
        m12 = 2f * (q.x * q.y - q.w * q.z);
        m13 = 2f * (q.x * q.z + q.w * q.y);

        m21 = 2f * (q.x * q.y + q.w * q.z);
        m22 = 1f - 2f * (q.x * q.x - q.z * q.z);
        m23 = 2f * (q.y * q.z - q.w * q.x);

        m31 = 2f * (q.x * q.z - q.w * q.y);
        m32 = 2f * (q.y * q.z + q.w * q.x);
        m33 = 1f - 2 * (q.x * q.x - q.y * q.y);
    }

    // 回転を実行する
    public Vec3 InertialToObject(Vec3 v)
    {
        var result = new Vec3();
        result.x = v.x * m11 + v.y * m21 + v.z * m31;
        result.y = v.x * m12 + v.y * m22 + v.z * m32;
        result.z = v.x * m13 + v.y * m23 + v.z * m33;
        return result;
    }

    // 回転を実行する
    public Vec3 ObjectToInertial(Vec3 v)
    {
        // 行列は逆行列を利用する
        var result = new Vec3();
        result.x = v.x * m11 + v.y * m12 + v.z * m13;
        result.y = v.x * m21 + v.y * m22 + v.z * m23;
        result.z = v.x * m31 + v.y * m32 + v.z * m33;
        return result;
    }
}