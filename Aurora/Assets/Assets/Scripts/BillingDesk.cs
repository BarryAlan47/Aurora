using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Sirenix.OdinInspector;

/// <summary>
/// 收银台：负责打包顾客商品、生成金币以及排队管理。
/// </summary>
public class BillingDesk : MonoBehaviour
{
    [LabelText("是否有顾客在台前")]
    private bool customer, player, isCounterEmpty = true;

    [LabelText("当前结账顾客")]
    private Customer currentCustomer;

    [LabelText("包装箱生成位置")]
    public Transform packageBoxPos;

    [LabelText("金币堆叠父物体")]
    public Transform moneyPosParent;

    [LabelText("收银员站位")]
    public Transform cashierPos;

    [LabelText("包装箱预制体")]
    public GameObject packageBoxPrefab;

    [LabelText("当前包装箱实例")]
    private GameObject packageBox;

    [HideInInspector]
    [LabelText("等待结账的顾客列表")]
    public List<Customer> customersForBilling;

    [LabelText("排队队列位置数组")]
    public Transform[] billingQue;

    [HideInInspector]
    [LabelText("已生成的金币列表")]
    public List<GameObject> money;

    [LabelText("金币堆叠位置列表")]
    public List<Transform> moneyPos;

    [HideInInspector]
    [LabelText("当前金币堆叠索引")]
    public int moneyPosCount;

    /// <summary>
    /// 顾客进入收银台触发器时，开始结账流程。
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Customer") && isCounterEmpty)
        {
            currentCustomer = other.gameObject.GetComponent<Customer>();

            if (currentCustomer.collectedFoods.Count > 0)
            {
                isCounterEmpty = false;
                customer = true;
                CheckPackaging();
            }
        }
    }

    /// <summary>
    /// 玩家停留在收银台触发器中时，标记玩家已就位。
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && !player)
        {
            player = true;
            CheckPackaging();
        }
    }

    /// <summary>
    /// 检查顾客与玩家是否都在台前，若是则开始打包流程。
    /// </summary>
    private void CheckPackaging()
    {
        if (customer && player)
        {
            if (packageBox == null)
                packageBox = Instantiate(packageBoxPrefab, packageBoxPos.position, packageBoxPos.rotation);

            CollectFoodFromCustomer();
        }
    }

    /// <summary>
    /// 顾客或玩家离开收银台触发器时重置标记。
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Customer"))
            customer = false;

        if (other.CompareTag("Player")) 
            player = false;
    }

    /// <summary>
    /// 从当前顾客购物车中依次取出食物并放入包装箱，完成后生成包裹。
    /// </summary>
    public void CollectFoodFromCustomer()
    {
        int foodCount = currentCustomer.collectedFoods.Count;
        Food food;

        if (foodCount > 0)
        {
            food = currentCustomer.collectedFoods[foodCount - 1];
            currentCustomer.collectedFoods.Remove(food);
            food.GotoBillingCounterBox(packageBoxPos);
        }
        else
        {
            packageBox.GetComponent<Animator>().SetTrigger("StartProduction");

            Invoke("DeliverBox", .6f);          
        }
    }

    /// <summary>
    /// 将打包好的箱子抛给顾客手中，并触发支付与离场。
    /// </summary>
    private void DeliverBox()
    {
        if (currentCustomer != null)
        {
            Destroy(currentCustomer.trolly);

            if (packageBox)
            {

                print(packageBox.gameObject.name);
                print(currentCustomer.gameObject.name);
                print(currentCustomer.handPos.gameObject.name);


                packageBox.transform.DOJump(currentCustomer.handPos.position, 4, 1, .3f)
                .OnComplete(delegate ()
                {
                    packageBox.transform.position = currentCustomer.handPos.position;
                    packageBox.transform.rotation = currentCustomer.handPos.rotation;
                    packageBox.transform.parent = currentCustomer.transform;

                    packageBox = null;
                    currentCustomer.PayMoney();
                    GetComponent<AudioSource>().Play();
                    customer = false;
                    Invoke("GotoMyExit", .4f);
                });
            }
        }
    }

    /// <summary>
    /// 让当前顾客前往出口并重置收银台状态。
    /// </summary>
    private void GotoMyExit()
    {
        currentCustomer.GoToExit();
        currentCustomer = null;
        isCounterEmpty = true;
    }

    /// <summary>
    /// 根据队列位置重新排列等待结账的顾客。
    /// </summary>
    public void ArrangeCustomersInQue()
    {
        for(int i = 0; i< customersForBilling.Count; i++)
        {
            if (customersForBilling[i].target == null || customersForBilling[i].target != billingQue[i])
            {
                customersForBilling[i].agent.SetDestination(billingQue[i].position);
                customersForBilling[i].target = billingQue[i];
                customersForBilling[i].counterLook = true;
            }
        }
    }
}
