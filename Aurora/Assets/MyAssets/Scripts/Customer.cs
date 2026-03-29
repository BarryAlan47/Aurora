using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using DG.Tweening;
using Sirenix.OdinInspector;

/// <summary>
/// 顾客逻辑：选择货架、取货、排队结账并支付金币。
/// </summary>
public class Customer : MonoBehaviour
{
    [LabelText("可选货架数组")]
    private GameObject[] availableShelfs;

    [LabelText("手推车上食物位置数组")]
    public Transform[] trollyPoses;

    [LabelText("已收集食物列表")]
    public List<Food> collectedFoods;

    [LabelText("本次计划购买食物数量 / 已使用位置计数")]
    public int buyFoodCapacity, trollyPoseCount = 0;

    [LabelText("是否前往收银台 / 是否还能继续取货")]
    private bool goToBillingCounter, canCollect = true;

    [HideInInspector]
    [LabelText("是否抵达后朝向收银台标记")]
    public bool counterLook;

    [LabelText("收银台引用")]
    private BillingDesk billingDesk;

    [LabelText("手部挂载点 / 队列目标点")]
    public Transform handPos, target;

    [HideInInspector]
    [LabelText("出口 Transform")]
    public Transform exitTransform;

    [LabelText("金币预制体 / 手推车对象")]
    public GameObject moneyPrefab, trolly;

    [LabelText("帽子 MeshFilter")]
    public MeshFilter hat;

    [LabelText("顾客皮肤渲染器")]
    public SkinnedMeshRenderer skin;

    [HideInInspector]
    [LabelText("导航代理")]
    public NavMeshAgent agent;

    [LabelText("动画控制器")]
    public Animator anim;

    [LabelText("当前目标货架位置")]
    Vector3 targetShelfPos;

    [LabelText("当前占用的顾客站位点")]
    private CustomerPoints _CustomerPoints;

    /// <summary>本次目标货架上的食物名（用于头顶订单图标映射）。</summary>
    private string _orderFoodName;

    /// <summary>
    /// 初始化外观、寻路与目标货架。
    /// </summary>
    private void Start()
    {
        GameManager _GameManager = FindObjectOfType<GameManager>();
        skin.material.color = _GameManager.customerColors[Random.Range(0, _GameManager.customerColors.Length)];
        hat.mesh = _GameManager.customerHats[Random.Range(0, _GameManager.customerHats.Length)];

        billingDesk = FindObjectOfType<BillingDesk>();

        buyFoodCapacity = Random.Range(1, 4);

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = true;

        availableShelfs = GameObject.FindGameObjectsWithTag("Shelf");

        targetShelfPos = FindShelf();

        agent.SetDestination(targetShelfPos);

        // 头顶订单 UI（每人一份气泡，见 CustomerOrderInfoService）
        if (CustomerOrderInfoService.Instance != null)
            CustomerOrderInfoService.Instance.ShowForCustomer(transform, buyFoodCapacity, _orderFoodName);
    }

    /// <summary>
    /// 随机选择一个有空位的货架顾客点并返回其位置。
    /// </summary>
    private Vector3 FindShelf()
    {
        int randVal = Random.Range(0, availableShelfs.Length);

        FoodPlaceManager shelf = availableShelfs[randVal].GetComponent<FoodPlaceManager>();
        foreach (CustomerPoints customerPoint in shelf.customerPoints)
        {
            if (!customerPoint.fill)
            {
                customerPoint.fill = true;
                _CustomerPoints = customerPoint;
                _orderFoodName = shelf != null ? shelf.shelfFoodName : null;
                return customerPoint.transform.position;
            }
        }

        return FindShelf();
    }

    /// <summary>
    /// 触发出口或顾客站位点时的逻辑。
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Exit"))
        {
            other.GetComponentInParent<CustomerSpawner>().SpawnCustomer();
            Destroy(this.gameObject);
        }

        if (other.CompareTag("CustomerPoint") && !goToBillingCounter)
        {
            if (other.gameObject == _CustomerPoints.gameObject)
            {
                agent.updateRotation = false;
                transform.rotation = other.transform.rotation;
            }
        }
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("CustomerPoint"))
    //    {
    //        print("Name  " + other.gameObject);

    //        if (other.gameObject == _CustomerPoints.gameObject)
    //            _CustomerPoints.fill = false;
    //    }
    //}

    /// <summary>
    /// 前往收银台排队。
    /// </summary>
    public void GoToBillingCounter()
    {
        if (CustomerOrderInfoService.Instance != null)
            CustomerOrderInfoService.Instance.HideForCustomer(transform);

        goToBillingCounter = true;
        agent.updateRotation = true;
        _CustomerPoints.fill = false;
        agent.isStopped = false;
        billingDesk.customersForBilling.Add(this);
        billingDesk.ArrangeCustomersInQue();
    }

    /// <summary>
    /// 前往出口并从收银队列中移除。
    /// </summary>
    public void GoToExit()
    {
        
        billingDesk.customersForBilling.Remove(this);
        agent.SetDestination(exitTransform.position);
        billingDesk.ArrangeCustomersInQue();
    }

    /// <summary>
    /// 生成金币飞向收银台，作为付款效果。
    /// </summary>
    public void PayMoney()
    {
        int val = buyFoodCapacity * 2;

        for (int i = 0; i < val; i++) 
        {
            int index = billingDesk.moneyPosCount;

            GameObject money = Instantiate(moneyPrefab, transform.position, transform.rotation);

            money.transform.DOJump(billingDesk.moneyPos[index].position, 4, 1, .4f)
            .OnComplete(delegate ()
            {
                billingDesk.money.Add(money);
            });

            if (billingDesk.moneyPosCount == 9) 
            {
                billingDesk.moneyPosCount = 0;

                Vector3 vec = billingDesk.moneyPosParent.position;
                vec.y = vec.y+1;

                billingDesk.moneyPosParent.position = vec;
            }
            else
                billingDesk.moneyPosCount++;
        }
    }

    /// <summary>
    /// 在货架触发区域内时，从货架取走食物放入推车。
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Shelf") && ReachedDestinationOrGaveUp())
        {
            FoodPlaceManager shelf = other.GetComponent<FoodPlaceManager>();

            if (shelf.collectedFoods.Count > 0 && canCollect)
            {
                Food food = shelf.collectedFoods[shelf.collectedFoods.Count - 1];
                // 补货跳跃到货架未完成时勿取走，否则会打断 DOJump，看起来「直接飞向购物车」
                if (!food.CanCustomerPickupFromShelf())
                    return;

                shelf.collectedFoods.Remove(food);
                shelf.MoveShelfTopTransform();

                food.GotoCustomer(trollyPoses[trollyPoseCount], this);
                collectedFoods.Add(food);
                
                trollyPoseCount++;
                canCollect = false;
            }
        }
    }

    /// <summary>
    /// 食物收集完成回调：数量达到上限则前往收银台。
    /// </summary>
    public void FoodColected()
    {
       if (collectedFoods.Count == buyFoodCapacity)
       {
           if (CustomerOrderInfoService.Instance != null)
               CustomerOrderInfoService.Instance.HideForCustomer(transform);
           Invoke("GoToBillingCounter", 0.5f);
           return;
       }

       int remaining = buyFoodCapacity - collectedFoods.Count;
       if (CustomerOrderInfoService.Instance != null)
           CustomerOrderInfoService.Instance.ShowForCustomer(transform, remaining, _orderFoodName);

       canCollect = true;       
    }

    private void OnDestroy()
    {
        if (CustomerOrderInfoService.Instance != null)
            CustomerOrderInfoService.Instance.HideForCustomer(transform);
    }

    /// <summary>
    /// 更新顾客朝向与跑步动画。
    /// </summary>
    private void Update()
    {
        if (counterLook)
        {
            if (ReachedDestinationOrGaveUp())
            {
                transform.rotation = target.rotation;
                counterLook = false;
            }
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
            anim.SetBool("Run", false);
        else
            anim.SetBool("Run", true);
    }

    /// <summary>
    /// 检查是否已到达导航目标或放弃路径。
    /// </summary>
    private bool ReachedDestinationOrGaveUp()
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
