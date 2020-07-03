using UnityEngine;

public class Matrix4x3
{
    public float m11, m12, m13;
    public float m21, m22, m23;
    public float m31, m32, m33;
    public float tx, ty, tz;

    // 恒等行列をセットする
    public void Identity()
    {
        m11 = 1f; m12 = 0f; m13 = 0f;
        m21 = 0f; m22 = 1f; m23 = 0f;
        m31 = 0f; m32 = 0f; m33 = 1f;
        tx = ty = tz = 0f;
    }

    // ４行目を0にする(平行移動を無効にする)
    public void ZeroTranslation()
    {
        tx = ty = tz = 0f;
    }

    // ベクトル形式で行列の平行移動部分を設定する
    public void SetTranslation(Vec3 d)
    {
        tx = d.x; ty = d.y; tz = d.z;
    }

    // ベクトル形式で行列の平行移動部分を設定する
    public void SetupTanslation(Vec3 d)
    {
        // 線形変換部分に恒等行列を設定する
        m11 = 1f; m12 = 0f; m13 = 0f;
        m21 = 0f; m22 = 1f; m23 = 0f;
        m31 = 0f; m32 = 0f; m33 = 1f;
        // 平行移動部分を設定する
        tx = d.x; ty = d.y; tz = d.z;
    }

    // 親の基準座標系内のローカルの基準座標系の位置と方向を指定し、
    // ローカル->親への座標変換を実行する行列をセットアップする
    public void SetupLocalToParent(Vec3 pos, EulerAngles orient)
    {
        // 回転行列を作成する
        var orientMatrix = new RotationMatrix();
        orientMatrix.Setup(orient);

        // 4x3行列をセットアップする
        // SetupLocalToParent(Vec3, RotationMatrix)を呼ぶことで簡略化されるが処理速度は最速ではない
        SetupLocalToParent(pos, orientMatrix);
    }

    public void SetupLocalToParent(Vec3 pos, RotationMatrix orient)
    {
        // 行列の回転部分をコピーする
        // 回転行列は通常、慣性空間->オブジェクト空間への行列であり、親->ローカルである
        // ここではローカル->親が欲しいのでコピー中に転置している
        m11 = orient.m11; m12 = orient.m21; m13 = orient.m31;
        m21 = orient.m12; m22 = orient.m22; m23 = orient.m32;
        m31 = orient.m13; m32 = orient.m23; m33 = orient.m33;

        // 平行移動部分を設定する
        tx = pos.x; ty = pos.y; tz = pos.z;
    }

    // 親の基準座標系内のローカルの基準座標系の位置と方向を指定し、
    // 親->ローカルへの座標変換を実行する行列をセットアップする
    public void SetupParentToLocal(Vec3 pos, EulerAngles orient)
    {
        // 回転行列を作成する
        var orientMatrix = new RotationMatrix();
        orientMatrix.Setup(orient);

        // 4x3行列をセットアップする
        // SetupParentToLocal(Vec3, RotationMatrix)を呼ぶことで簡略化されるが処理速度は最速ではない
        SetupParentToLocal(pos, orientMatrix);
    }

    public void SetupParentToLocal(Vec3 pos, RotationMatrix orient)
    {
        // 行列の回転部分をコピーする
        m11 = orient.m11; m12 = orient.m12; m13 = orient.m13;
        m21 = orient.m21; m22 = orient.m22; m23 = orient.m23;
        m31 = orient.m31; m32 = orient.m32; m33 = orient.m33;

        // 平行移動部分を設定する
        // posだけ逆に平行移動し、ワールド空間から慣性空間へ平行移動する
        // しかし、回転->平行移動の順に座標変換は行われる
        // つまり平行移動部分を回転する必要がある
        // これは-posだけ平行移動する行列Tと回転行列Rを作成し、
        // 連結したTR行列を作成することと同じである
        tx = -(pos.x * m11 + pos.y * m21 + pos.z * m31);
        ty = -(pos.x * m12 + pos.y * m22 + pos.z * m32);
        tz = -(pos.x * m13 + pos.y * m23 + pos.z * m33);
    }

    // 基本軸の周りの回転を実行する行列をセットアップする
    // axisは以下に対応している
    // 1 => x軸に関する回転
    // 2 => y軸に関する回転
    // 3 => z軸に関する回転
    // 
    // thetaは回転の量(ラジアン)。左手の回転ルールを定義する
    // 平行移動部分はリセットされる
    public void SetupRotate(int axis, float theta)
    {
        float s, c;
        MathUtil.sinCos(theta, out s, out c);

        switch (axis)
        {
            case 1:
                m11 = 1; m12 = 0; m13 = 0;
                m21 = 0; m22 = c; m23 = s;
                m31 = 0; m32 = -s; m33 = c;
                break;
            case 2:
                m11 = c; m12 = 0; m13 = -s;
                m21 = 0; m22 = 1; m23 = 0;
                m31 = s; m32 = 0; m33 = c;
                break;
            case 3:
                m11 = c; m12 = s; m13 = 0;
                m21 = -s; m22 = c; m23 = 0;
                m31 = 0; m32 = 0; m33 = 1;
                break;
            default:
                Debug.Log("axis不正値");
                return;
        }

        tx = ty = tz = 0f;
    }

    // 任意の軸の周りの回転を実行する行列をセットアップする
    // 回転の軸は原点を通らなければならない

    // axisは回転の軸を定義し、単位ベクトルでなければならない
    // thetaは回転の量(ラジアン)。左手の回転ルールを定義する
    // 平行移動部分はリセットされる
    public void SetupRotate(Vec3 axis, float theta)
    {
        // 単位ベクトルかチェック
        if (Mathf.Abs(Vec3.Dot(axis, axis) - 1f) < 0.01f)
        {
            Debug.Log("単位ベクトルではない");
            return;
        }

        float s, c;
        MathUtil.sinCos(theta, out s, out c);

        // 1-cos(theta)と共通して用いる副次式を求める
        var a = 1f - c;
        var ax = a * axis.x;
        var ay = a * axis.y;
        var az = a * axis.z;

        m11 = ax * axis.x + c;
        m12 = ax * axis.y + c * axis.z;
        m13 = ax * axis.z - c * axis.y;

        m21 = ay * axis.x - c * axis.z;
        m22 = ay * axis.y + c;
        m23 = ay * axis.z + c * axis.x;

        m31 = az * axis.x + c * axis.y;
        m32 = az * axis.y - c * axis.x;
        m33 = az * axis.z + c;

        tx = ty = tz = 0f;
    }

    // 角変位を四元数形式で与え、回転を実行する行列をセットアップする
    // 平行移動部分はリセットされる
    public void FromQuaternion(Qtrn q)
    {
        // 共通して用いる副次式を最適化するために値を計算する
        var ww = 2f * q.w;
        var xx = 2f * q.x;
        var yy = 2f * q.y;
        var zz = 2f * q.z;

        // 行列の要素を設定する
        m11 = 1f - yy * q.y - zz * q.z;
        m12 = xx * q.y + ww * q.z;
        m13 = xx * q.z - ww * q.y;

        m21 = yy * q.x - ww * q.z;
        m22 = 1f - xx * q.x - zz * q.z;
        m23 = yy * q.z + ww * q.x;

        m31 = zz * q.x + ww * q.y;
        m32 = zz * q.y - ww * q.x;
        m33 = 1f - xx * q.x - yy * q.y;

        tx = ty = tz = 0f;
    }

    // 各軸でスケーリングを実行する行列をセットアップする
    // kだけ均等スケーリングをする場合は、Vec3(k,k,k)を用いる
    // 平行移動部分はリセットされる
    public void SetupScale(Vec3 s)
    {
        // 行列の要素を設定する
        m11 = s.x; m12 = 0f; m13 = 0f;
        m21 = 0f; m22 = s.y; m23 = 0f;
        m31 = 0f; m32 = 0f; m33 = s.z;
        tx = ty = tz = 0f;
    }

    // 任意の軸でスケーリングを実行する行列をセットアップする
    // axisは単位ベクトル
    // 平行移動部分はリセットされる
    public void SetupScaleAlongAxis(Vec3 axis, float k)
    {
        // 単位ベクトルかチェック
        if (Mathf.Abs(Vec3.Dot(axis, axis) - 1f) < 0.01f)
        {
            Debug.Log("単位ベクトルではない");
            return;
        }

        // k-1と共通して用いる副次式を計算する
        var a = k - 1f;
        var ax = a * axis.x;
        var ay = a * axis.y;
        var az = a * axis.z;

        // 行列の要素を埋める
        // 対角線上で反対の行列の要素が等しいため計算を省略する
        m11 = 1f + ax * axis.x;
        m22 = 1f + ay * axis.y;
        m33 = 1f + az * axis.z;

        m12 = m21 = ax * axis.y;
        m23 = m32 = ay * axis.z;
        m13 = m31 = az * axis.x;

        tx = ty = tz = 0f;
    }

    // せん断を実行する行列をセットアップする
    // axisは以下に対応している
    // 1 => y += s*x, z += t*x
    // 2 => x += s*y, z += t*y
    // 3 => x += s*z, y += t*z
    // 平行移動部分はリセットされる
    public void SetupShear(int axis, float s, float t)
    {
        switch (axis)
        {
            case 1:
                m11 = 1f; m12 = s; m13 = t;
                m21 = 0f; m22 = 1f; m23 = 0f;
                m31 = 0f; m32 = 0f; m33 = 1f;
                break;
            case 2:
                m11 = 1f; m12 = 0f; m13 = 0f;
                m21 = s; m22 = 1f; m23 = t;
                m31 = 0f; m32 = 0f; m33 = 1f;
                break;
            case 3:
                m11 = 1f; m12 = 0f; m13 = 0f;
                m21 = 0f; m22 = 1f; m23 = 0f;
                m31 = s; m32 = t; m33 = 1f;
                break;
            default:
                Debug.Log("axis不正値");
                return;
        }

        tx = ty = tz = 0f;
    }

    // 原点を通る面上への投影を実行する行列をセットアップする
    // この面は、単位ベクトルnに垂直である
    public void SetupProject(Vec3 n)
    {
        // 単位ベクトルかチェック
        if (Mathf.Abs(Vec3.Dot(n, n) - 1f) < 0.01f)
        {
            Debug.Log("単位ベクトルではない");
            return;
        }

        // 行列の要素を埋める
        // 共通して用いる副次式を最適化するために値を計算する
        // 対角線上で反対の行列の要素が等しいため計算を省略する
        m11 = 1f - n.x * n.x;
        m22 = 1f - n.y * n.y;
        m33 = 1f - n.z * n.z;

        m12 = m21 = -n.x * n.y;
        m13 = m31 = -n.x * n.z;
        m23 = m32 = -n.y * n.z;

        tx = ty = tz = 0f;
    }

    // 基本軸に平行な面に関するリフレクションを実行する行列をセットアップする
    // axisは以下に対応している
    // 1 => 面x=kに関するリフレクション
    // 2 => 面y=kに関するリフレクション
    // 3 => 面z=kに関するリフレクション
    // 平行移動は適切に設定される(平行移動はk!=0の場合にだけ起きる)
    public void SetupReflect(int axis, float k = 0f)
    {
        switch (axis)
        {
            case 1:
                m11 = -1f; m12 = 0f; m13 = 0f;
                m21 = 0f; m22 = 1f; m23 = 0f;
                m31 = 0f; m32 = 0f; m33 = 1f;
                tx = 2f * k;
                ty = 0;
                tz = 0;
                break;
            case 2:
                m11 = 1f; m12 = 0f; m13 = 0f;
                m21 = 0f; m22 = -1f; m23 = 0f;
                m31 = 0f; m32 = 0f; m33 = 1f;
                tx = 0;
                ty = 2f * k;
                tz = 0;
                break;
            case 3:
                m11 = 1f; m12 = 0f; m13 = 0f;
                m21 = 0f; m22 = 1f; m23 = 0f;
                m31 = 0f; m32 = 0f; m33 = -1f;
                tx = 0;
                ty = 0;
                tz = 2f * k;
                break;
            default:
                break;
        }
    }

    // 原点を通る任意の平面に関するリフレクションを実行する行列をセットアップする
    // この面は、単位ベクトルnに垂直である
    // 平行移動部分はリセットされる
    public void SetupReflect(Vec3 n)
    {
        // 単位ベクトルかチェック
        if (Mathf.Abs(Vec3.Dot(n, n) - 1f) < 0.01f)
        {
            Debug.Log("単位ベクトルではない");
            return;
        }

        // 共通して用いる副次式を計算する
        var ax = -2f * n.x;
        var ay = -2f * n.y;
        var az = -2f * n.z;

        // 行列の要素を埋める
        m11 = 1f + ax * n.x;
        m22 = 1f + ay * n.y;
        m33 = 1f + az * n.z;

        m12 = m21 = ax * n.y;
        m23 = m32 = ay * n.z;
        m13 = m31 = az * n.x;

        tx = ty = tz = 0f;
    }

    // 点を座標変換する
    public static Vec3 operator *(Vec3 p, Matrix4x3 m)
    {
        return new Vec3(
            p.x * m.m11 + p.y * m.m12 + p.z * m.m13,
            p.x * m.m21 + p.y * m.m22 + p.z * m.m23,
            p.x * m.m31 + p.y * m.m32 + p.z * m.m33);
    }

    // 行列の連結
    public static Matrix4x3 operator *(Matrix4x3 a, Matrix4x3 b)
    {
        var r = new Matrix4x3();

        r.m11 = a.m11 * b.m11 + a.m12 * b.m21 + a.m13 * b.m31;
        r.m12 = a.m11 * b.m12 + a.m12 * b.m22 + a.m13 * b.m32;
        r.m13 = a.m11 * b.m13 + a.m12 * b.m23 + a.m13 * b.m33;

        r.m21 = a.m21 * b.m11 + a.m22 * b.m21 + a.m23 * b.m31;
        r.m22 = a.m21 * b.m12 + a.m22 * b.m22 + a.m23 * b.m32;
        r.m23 = a.m21 * b.m13 + a.m22 * b.m23 + a.m23 * b.m33;

        r.m31 = a.m31 * b.m11 + a.m32 * b.m21 + a.m33 * b.m31;
        r.m32 = a.m31 * b.m12 + a.m32 * b.m22 + a.m33 * b.m32;
        r.m33 = a.m31 * b.m13 + a.m32 * b.m23 + a.m33 * b.m33;

        return r;
    }

    // 行列の3x3部分の行列式を計算する
    public float determinant
    {
        get
        {
            return
                m11 * (m22 * m33 - m23 * m32) +
                m12 * (m23 * m13 - m21 * m33) +
                m13 * (m21 * m32 - m22 * m31);
        }
    }

    // 逆行列を計算する
    // determinantで除算された随伴行列を用いる
    public Matrix4x3 inverse
    {
        get
        {
            // 行列式を計算する
            var det = determinant;

            // 特異行列の場合、行列式は0になり逆行列は存在しない
            if (Mathf.Abs(det) < 0.0001f)
            {
                Debug.Log("特異行列のため逆行列は存在しない");
                return new Matrix4x3();
            }

            // 1 / 行列式
            var oneOverDet = 1f / det;

            // 逆行列3x3部分を計算する
            var r = new Matrix4x3();
            r.m11 = (m22 * m33 - m23 * m32) * oneOverDet;
            r.m12 = (m21 * m33 - m23 * m31) * oneOverDet;
            r.m13 = (m21 * m32 - m22 * m31) * oneOverDet;

            r.m21 = (m12 * m33 - m13 * m32) * oneOverDet;
            r.m22 = (m11 * m33 - m13 * m31) * oneOverDet;
            r.m23 = (m11 * m32 - m12 * m31) * oneOverDet;

            r.m31 = (m12 * m23 - m13 * m22) * oneOverDet;
            r.m32 = (m11 * m23 - m13 * m21) * oneOverDet;
            r.m33 = (m11 * m22 - m12 * m21) * oneOverDet;

            // 逆行列の平行移動部分を計算する
            r.tx = tx * r.m11 + ty * r.m12 + tz * r.m13;
            r.ty = tx * r.m21 + ty * r.m22 + tz * r.m23;
            r.tz = tx * r.m31 + ty * r.m32 + tz * r.m33;

            return r;
        }
    }

    // 平行移動の行をベクトルで返す
    public Vec3 getTranslation
    {
        get
        {
            return new Vec3(tx, ty, tz);
        }
    }

    // 親->ローカル座標変換行列(ワールド行列->オブジェクト行列など)を与え、
    // オブジェクトの位置を取り出す
    public Vec3 getPositionFromParentToLocalMatrix
    {
        get
        {
            // 3x3部分の転置と負の平行移動値を乗算する
            return new Vec3(
                -(tx * m11 + ty * m12 + tz * m13),
                -(ty * m21 + ty * m22 + tz * m23),
                -(tz * m31 + ty * m32 + tz * m33));
        }
    }

    // ローカル->親の座標変換行列(オブジェクト行列->ワールド行列など)を与え、
    // オブジェクトの位置を取り出す
    public Vec3 getPositionFromLocalToParentMatrix
    {
        get
        {
            // 単なる平行移動部分
            return new Vec3(tx, ty, tz);
        }
    }
}