using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// 相机跟随：平滑跟随目标角色。
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [LabelText("跟随距离")]
    public float distance;

    [LabelText("高度偏移")]
    public float height;

    [LabelText("平滑系数")]
    public float smoothness;

    [LabelText("跟随目标")]
    public Transform camTarget;

    [LabelText("额外偏移")]
    public Vector3 offset;

    [LabelText("当前平滑速度向量")]
    Vector3 velocity;

    /// <summary>
    /// 在 LateUpdate 中更新相机位置，保证先更新完角色再跟随。
    /// </summary>
    void LateUpdate()
    {
        if (!camTarget)
            return;

        Vector3 pos = Vector3.zero;
        pos.x = camTarget.position.x;
        pos.y = camTarget.position.y + height;
        pos.z = camTarget.position.z - distance;

        transform.position = Vector3.SmoothDamp(transform.position, pos+offset, ref velocity, smoothness);
    }

    //public Material m;
    //public float TransparencyLevel;

    //private void Awake()
    //{
    //    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
    //    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
    //    m.SetInt("_ZWrite", 0);
    //    m.DisableKeyword("_ALPHATEST_ON");
    //    m.DisableKeyword("_ALPHABLEND_ON");
    //    m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
    //    m.renderQueue = 3000;

    //}
}
