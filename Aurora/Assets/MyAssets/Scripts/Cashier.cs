using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;

/// <summary>
/// 收银员：进入场景后自动走到收银台位置并待命。
/// </summary>
public class Cashier : MonoBehaviour
{
    [LabelText("收银员目标位置")]
    private Transform cashierPos;

    [LabelText("导航代理")]
    private NavMeshAgent agent;

    [LabelText("动画控制器")]
    public Animator anim;

    /// <summary>
    /// 初始化导航并让收银员前往收银台。
    /// </summary>
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        cashierPos = FindObjectOfType<BillingDesk>().cashierPos;
        agent.SetDestination(cashierPos.position);
    }

    /// <summary>
    /// 更新收银员移动与到达判断，控制跑步动画。
    /// </summary>
    private void Update()
    {
        if (ReachedDestination())
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);

            Destroy(this);
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            anim.SetBool("Run", false);
        else
            anim.SetBool("Run", true);
    }

    /// <summary>
    /// 检查是否已到达目标点。
    /// </summary>
    private bool ReachedDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    return true;
            }
        }

        return false;
    }
}
