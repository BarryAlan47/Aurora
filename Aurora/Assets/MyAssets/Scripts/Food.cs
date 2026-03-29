using UnityEngine;
using RDG;
using DG.Tweening;
using System.Collections.Generic;
using Sirenix.OdinInspector;

/// <summary>
/// 食物逻辑：负责被玩家/Helper/顾客拾取及在场景中的跳跃移动。
/// </summary>
public class Food : MonoBehaviour
{
    [LabelText("当前堆叠位置 Transform")]
    private Transform foodCollectPos;

    [LabelText("每个食物在玩家身上的高度增量")]
    private float foodCollectPlayerYVal = 1f;

    [LabelText("是否仍可飞向玩家")]
    private bool goToPlayer = true;

    [HideInInspector]
    [LabelText("是否正在飞向顾客")]
    public bool goToCustomer;

    [LabelText("是否不自动重新生成")]
    public bool notSpawnAuto;

    [LabelText("跳跃速度与高度")]
    public float speed, jumpPower;

    [LabelText("跳跃目标位置")]
    private Transform targetPose;

    [LabelText("食物名称")]
    public string foodName;

    [LabelText("收银台引用")]
    private BillingDesk _BillingDesk;

    [HideInInspector]
    [LabelText("玩家最大可携带数量快照")]
    public int maxFoodPlayerCarry;

    /// <summary>
    /// 玩家/Helper 补货时，食物正在 DOJump 到货架顶；完成前顾客不应取走，否则会打断动画直接飞向购物车。
    /// </summary>
    private bool _shelfPlacementTweenActive;

    /// <summary>
    /// 货架上的这份食物是否允许被顾客取走（已落位到货架上的跳跃结束）。
    /// </summary>
    public bool CanCustomerPickupFromShelf() => !_shelfPlacementTweenActive;

    /// <summary>
    /// 初始化收银台与玩家容量引用。
    /// </summary>
    private void Start()
    {
        _BillingDesk = FindObjectOfType<BillingDesk>();
        maxFoodPlayerCarry = FindObjectOfType<PlayerManager>().maxFoodPlayerCarry;
    }

    /// <summary>
    /// 被玩家或 Helper 碰撞时尝试加入其背包。
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (goToPlayer)
            {
                if (other.gameObject.GetComponent<PlayerManager>())
                {
                    PlayerManager _PlayerManager = other.gameObject.GetComponent<PlayerManager>();

                    if (other.gameObject.GetComponent<Helper>())
                    {
                        if(foodName != _PlayerManager.currentFoodName)
                        {
                            return;
                        }
                    }

                    if (_PlayerManager.collectedFood.Count < _PlayerManager.maxFoodPlayerCarry)
                    {
                        if(notSpawnAuto/*foodName != "Egg" || foodName != "Sauce"*/)
                            transform.GetComponentInParent<FoodSpawner>().foodObj = null;
                        else
                            transform.GetComponentInParent<FoodSpawner>().SpawnFood();


                        Vibration.Vibrate(30);

                        if(other.gameObject.layer == 7)
                           AudioManager.Instance.Play("FoodCollect");

                        transform.parent = other.transform;
                        _PlayerManager.collectedFood.Add(this);
                    }
                    else
                        return;
                }

                foodCollectPos = other.transform.GetChild(1).transform;
                targetPose = foodCollectPos;

                transform.DOLocalJump(targetPose.localPosition, jumpPower, 1, speed)
                .OnComplete(delegate ()
                {
                    this.transform.localPosition = foodCollectPos.localPosition;
                    this.transform.localEulerAngles = Vector3.zero;

                    foodCollectPos.position = new Vector3(foodCollectPos.transform.position.x, foodCollectPos.transform.position.y + foodCollectPlayerYVal, foodCollectPos.transform.position.z);
                });

                goToPlayer = false;
            }
        }
    }

    /// <summary>
    /// 将食物从玩家身上放回到货架指定位置。
    /// </summary>
    public void PlaceFood(Transform targetPos)
    {
        if (transform.parent)
            transform.parent = null;

        targetPose = targetPos;

        transform.DOKill();
        _shelfPlacementTweenActive = true;

        transform.DOJump(targetPose.position, 4, 1, .4f)
            .OnComplete(() =>
            {
                _shelfPlacementTweenActive = false;
            });

        if (foodCollectPos != null)
        {
            foodCollectPos.position = new Vector3(
                foodCollectPos.transform.position.x,
                foodCollectPos.transform.position.y - foodCollectPlayerYVal,
                foodCollectPos.transform.position.z);
        }
    }

    /// <summary>
    /// 跳跃到顾客手推车上的目标位置。
    /// </summary>
    public void GotoCustomer(Transform target, Customer customer)
    {
        goToPlayer = false;
        _shelfPlacementTweenActive = false;

        transform.DOKill();

        transform.DOJump(target.position, 4, 1, .4f)
        .OnComplete(delegate ()
        {
            goToCustomer = false;
            transform.parent = target;
            transform.position = target.position;
            customer.FoodColected();

        });
    }

    /// <summary>
    /// 跳跃到收银台包装箱，完成后通知收银台继续取食物。
    /// </summary>
    public void GotoBillingCounterBox(Transform target)
    {
        transform.DOJump(target.position, 4, 1, .4f)
        .OnComplete(delegate ()
        {
            _BillingDesk.CollectFoodFromCustomer();
            Destroy(this.gameObject);
        });
    }

    /// <summary>
    /// 跳跃到垃圾桶并销毁自身。
    /// </summary>
    public void GotoTrashBin(Transform target)
    {
        transform.DOJump(target.position, 4, 1, .4f)
        .OnComplete(delegate ()
        {
            Destroy(this.gameObject);
        });
    }
}
