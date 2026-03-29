using System.Collections;
using UnityEngine;
using RDG;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine.UI;

/// <summary>
/// 购买点：持续消耗玩家金币以解锁物体（货架、区域等）。
/// 用 <see cref="unlockDurationSeconds"/> 与 <see cref="spendInterval"/> 控制节奏：金币充足时约在总时长内扣完「本次进圈时」的剩余应付（线性匀速，非按剩余指数衰减）。
/// </summary>
public class BuyPoint : MonoBehaviour
{
    [FoldoutGroup("基础", expanded: true)]
    [LabelText("序号（存档 Key）")]
    public int srNo;

    [FoldoutGroup("基础")]
    [LabelText("解锁价格")]
    public int purchaseAmount;

    [LabelText("游戏管理器引用")]
    private GameManager _GameManager;

    [FoldoutGroup("解锁与扣款节奏")]
    [InspectorName("解锁总时长（秒）")]
    [LabelText("解锁总时长（秒）")]
    [Tooltip("玩家金币足够时，从当前剩余应付金额扣到 0 的预计总时间。与「扣款间隔」一起决定每次扣多少金币。")]
    [Min(0.01f)]
    public float unlockDurationSeconds = 3f;

    [FoldoutGroup("解锁与扣款节奏")]
    [InspectorName("扣款触发间隔（秒）")]
    [LabelText("扣款触发间隔（秒）")]
    [Tooltip("每隔多少秒触发一次扣款；每次扣款数量按「进圈时剩余应付」线性均摊，≈ 快照 × 间隔 ÷ 解锁总时长（至少 1）。")]
    [Min(0.01f)]
    public float spendInterval = 0.05f;

    [FoldoutGroup("进入区域后的等待")]
    [InspectorName("开始扣款前等待时间（秒）")]
    [Tooltip("玩家站在圈内先等待该时间，避免一进入就扣款；为 0 则立即开始扣款。")]
    [Min(0f)]
    [LabelText("开始扣款前等待时间（秒）")]
    public float waitBeforeSpendSeconds = 1f;

    [SerializeField]
    [FoldoutGroup("进入区域后的等待")]
    [LabelText("等待进度条（Fill Image）")]
    [Tooltip("子物体上挂 Image，Image Type 设为 Filled。进入区域后从 0 填到 1，满圈后保持；仅离开区域时重置为 0。与「解锁进度」分开。")]
    private Image waitProgressFill;

    [FoldoutGroup("表现与动画")]
    [LabelText("解锁缩放动画时长（秒）")]
    private float animDuration = 0.5f;

    [FoldoutGroup("界面引用")]
    [LabelText("剩余金额文本（TextMeshPro UI）")]
    private TextMeshProUGUI moneyAmountText;

    [SerializeField]
    [FoldoutGroup("界面引用")]
    [LabelText("解锁付款进度条（可选）")]
    [Tooltip("显示已付/总价的 Fill Image，与「等待进度条」可不同物体。")]
    private Image progressFill;

    [SerializeField]
    [FoldoutGroup("界面引用")]
    [LabelText("价格标签文本（可选）")]
    private TextMeshProUGUI priceLabel;

    [SerializeField]
    [FoldoutGroup("表现与动画")]
    [LabelText("飞币动画预制体（可选）")]
    [Tooltip("不填则不播放金币飞向购买点的动画。")]
    private GameObject moneyFlyPrefab;

    [SerializeField]
    [FoldoutGroup("表现与动画")]
    [LabelText("飞币落点（可选）")]
    [Tooltip("为空则落到本购买点 transform.position；可拖到购买点中心子物体以对齐 UI。")]
    private Transform moneyFlyTarget;

    [SerializeField]
    [FoldoutGroup("表现与动画")]
    [LabelText("在角色碰撞体顶部之上的额外高度")]
    [Tooltip("基于 PlayerController 的 CharacterController.bounds 顶部，再沿世界 Y 轴加该值；改此数值会明显改变生成高度。")]
    private float moneySpawnHeight = 1.2f;

    [SerializeField]
    [FoldoutGroup("表现与动画")]
    [LabelText("DOJump 跳跃高度 / 时长")]
    private float moneyJumpPower = 3f;

    [SerializeField]
    [FoldoutGroup("表现与动画")]
    [LabelText("DOJump 时长（秒）")]
    private float moneyJumpDuration = 0.5f;

    [SerializeField]
    [FoldoutGroup("表现与动画")]
    [LabelText("水平散开半径")]
    [Tooltip("多枚飞币时在玩家头顶附近随机偏移，避免完全重叠。")]
    private float moneySpawnSpread = 0.35f;

    [SerializeField]
    [FoldoutGroup("表现与动画")]
    [LabelText("单次扣款最多飞出几枚（表现上限）")]
    [Tooltip("一次扣很多金币时仍只生成至多 N 枚，避免 Instantiate 过多；FastFood 是每小段扣款一枚。")]
    private int maxVisualCoinsPerSpend = 10;

    [SerializeField]
    [FoldoutGroup("表现与动画")]
    [LabelText("多枚飞币间隔（秒）")]
    private float visualCoinStagger = 0.03f;

    [SerializeField]
    [FoldoutGroup("表现与动画")]
    [LabelText("飞币缓动曲线")]
    private Ease moneyJumpEase = Ease.OutQuad;

    [SerializeField]
    [FoldoutGroup("表现与动画")]
    [LabelText("飞出时缩放弹出")]
    private bool moneySpawnPopScale = true;

    [SerializeField]
    [FoldoutGroup("表现与动画")]
    [LabelText("飞币短音效名（可选）")]
    [Tooltip("填 AudioClip 的 name，用 PlayOneShot 叠加播放；留空则不在每枚币上播。主扣款仍播 BuyPoint。")]
    private string coinFlySoundName = "";

    [SerializeField]
    [FoldoutGroup("表现与动画")]
    [LabelText("仅第一枚飞币播短音效")]
    private bool coinFlySoundFirstOnly = true;

    /// <summary>离开区域时递增，用于取消尚未执行的错开飞币回调。</summary>
    private int _moneyFlySessionId;

    [LabelText("初始价格（用于进度计算）")]
    private int initialPurchaseAmount;

    [FoldoutGroup("基础")]
    [LabelText("要解锁的物体")]
    public GameObject objectToUnlock;

    [LabelText("玩家控制器引用")]
    private PlayerController _PlayerController;

    /// <summary>等待结束后再开始 <see cref="InvokeRepeating"/> 扣款的协程。</summary>
    private Coroutine _waitBeforeSpendRoutine;

    /// <summary>
    /// 本次 <see cref="BeginSpendingInvoke"/> 开始时锁定的「剩余应付」快照。
    /// 若用「当前剩余」每帧算 chunk，会变成指数衰减，总时长会远大于 <see cref="unlockDurationSeconds"/>（大额时可达数倍～十倍）。
    /// </summary>
    private int _spendSessionRemainingSnapshot;

    /// <summary>
    /// 初始化购买点状态，检查是否已解锁并加载剩余金额。
    /// </summary>
    private void Awake()
    {
        if (PlayerPrefs.HasKey(srNo + "Unlocked"))
        {
            if (objectToUnlock.GetComponent<FoodPlaceManager>())
                objectToUnlock.GetComponent<BoxCollider>().enabled = true;

            UnlockObject();
        }

        _PlayerController = ResolvePlayerController();
        _GameManager = FindObjectOfType<GameManager>();

        // 记录初始价格用于进度条显示，首次使用脚本时写入，之后从存档恢复
        string initialKey = srNo + "InitialPurchaseAmount";
        if (PlayerPrefs.HasKey(initialKey))
        {
            initialPurchaseAmount = PlayerPrefs.GetInt(initialKey, purchaseAmount);
        }
        else
        {
            initialPurchaseAmount = purchaseAmount;
            PlayerPrefs.SetInt(initialKey, initialPurchaseAmount);
        }

        purchaseAmount = PlayerPrefs.GetInt(srNo + "PurchaseAmount", purchaseAmount);

        moneyAmountText = GetComponentInChildren<TextMeshProUGUI>();

        if (waitProgressFill != null)
            waitProgressFill.fillAmount = 0f;

        ShowPurchaseAmount();
    }

    /// <summary>
    /// 显示当前剩余购买金额。
    /// </summary>
    private void ShowPurchaseAmount()
    {
        if (moneyAmountText != null)
            moneyAmountText.text = purchaseAmount.ToString();

        if (priceLabel != null)
            priceLabel.text = "$" + purchaseAmount.ToString();

        if (progressFill != null && initialPurchaseAmount > 0)
        {
            float paid = initialPurchaseAmount - purchaseAmount;
            progressFill.fillAmount = Mathf.Clamp01(paid / initialPurchaseAmount);
        }
    }

    /// <summary>
    /// 进入购买区域：先按 <see cref="waitBeforeSpendSeconds"/> 等待（可选同步 <see cref="waitProgressFill"/>），再开始持续扣款。
    /// </summary>
    public void StartSpend()
    {
        StopWaitRoutine();
        CancelInvoke(nameof(Spend));

        if (waitProgressFill != null)
            waitProgressFill.fillAmount = 0f;

        float wait = Mathf.Max(0f, waitBeforeSpendSeconds);
        if (wait <= 0f)
        {
            BeginSpendingInvoke();
            return;
        }

        _waitBeforeSpendRoutine = StartCoroutine(WaitThenBeginSpendingRoutine(wait));
    }

    void StopWaitRoutine()
    {
        if (_waitBeforeSpendRoutine != null)
        {
            StopCoroutine(_waitBeforeSpendRoutine);
            _waitBeforeSpendRoutine = null;
        }
    }

    IEnumerator WaitThenBeginSpendingRoutine(float wait)
    {
        float elapsed = 0f;
        while (elapsed < wait)
        {
            elapsed += Time.deltaTime;
            if (waitProgressFill != null)
                waitProgressFill.fillAmount = Mathf.Clamp01(elapsed / wait);
            yield return null;
        }

        _waitBeforeSpendRoutine = null;

        if (waitProgressFill != null)
            waitProgressFill.fillAmount = 1f;

        BeginSpendingInvoke();
        // 等待条保持满圈，直到玩家离开区域（见 StopSpend）
    }

    /// <summary>
    /// 开始按间隔重复扣款。
    /// </summary>
    void BeginSpendingInvoke()
    {
        // 进圈开始扣款瞬间的剩余应付：整段线性匀速都按此快照算 chunk，总时长才 ≈ unlockDurationSeconds
        _spendSessionRemainingSnapshot = Mathf.Max(1, purchaseAmount);

        float interval = Mathf.Max(0.01f, spendInterval);
        InvokeRepeating(nameof(Spend), interval, interval);
    }

    /// <summary>
    /// 单次应扣金币数：由「解锁总时长」与「扣款间隔」推算，使在金币充足时约在该时长内扣完「本次会话快照」对应的剩余。
    /// 公式：ceil(快照 × 间隔 ÷ 总时长)，至少为 1，且不超过玩家金币与当前剩余应付。
    /// </summary>
    private int GetCoinsToSpendThisTick()
    {
        int playerMoney = _GameManager != null ? _GameManager.collectedMoney : 0;
        if (purchaseAmount <= 0)
            return 0;

        float interval = Mathf.Max(0.01f, spendInterval);
        // 总时长至少不小于一个间隔，避免除零或单次扣光
        float duration = Mathf.Max(interval, unlockDurationSeconds);

        int basis = Mathf.Max(1, _spendSessionRemainingSnapshot);
        int chunk = Mathf.Max(1, Mathf.CeilToInt(basis * interval / duration));
        return Mathf.Max(0, Mathf.Min(chunk, Mathf.Min(playerMoney, purchaseAmount)));
    }

    /// <summary>
    /// 单次扣费逻辑：扣玩家金币、更新文本，金额为 0 时完成解锁。
    /// </summary>
    private void Spend()
    {
        int chunk = GetCoinsToSpendThisTick();

        if (chunk > 0 && _GameManager.collectedMoney > 0)
        {       
            AudioManager.Instance.Play("BuyPoint");

            Vibration.Vibrate(30);
            purchaseAmount -= chunk;
            PlayerPrefs.SetInt(srNo + "PurchaseAmount", purchaseAmount);

            _GameManager.LessMoneyinBulk(chunk);
            ShowPurchaseAmount();

            PlayMoneyFlyAnimations(chunk);

            if (purchaseAmount == 0)
            {
                PlayerPrefs.SetString(srNo + "Unlocked", "True");

                ResolvePlayerController()?.SidePos();
                objectToUnlock.transform.DOPunchScale(new Vector3(0.1f, 1, 0.1f), animDuration, 7).OnComplete(() => Destroy(this.gameObject)); ;
                UnlockObject();

                CustomerSpawner[] custSawners = FindObjectsOfType<CustomerSpawner>();
                custSawners[Random.Range(0, custSawners.Length)].SpawnCustomer();

                AudioManager.Instance.Play("Unlock");
                ParticleSystem particle = GetComponentInChildren<ParticleSystem>();
                particle.transform.parent = null;
                particle.Play();
            }
        }
        else
        {
            CancelInvoke(nameof(Spend));
        }
    }

    /// <summary>
    /// 解锁目标物体并销毁购买点。
    /// </summary>
    private void UnlockObject()
    {
        objectToUnlock.SetActive(true);
        DOTween.Kill(this.gameObject);      
        Destroy(this.gameObject);
    }

    /// <summary>
    /// 离开购买区域：取消等待与扣款，并将等待进度条归零。
    /// </summary>
    public void StopSpend()
    {
        _moneyFlySessionId++;
        StopWaitRoutine();
        CancelInvoke(nameof(Spend));

        if (waitProgressFill != null)
            waitProgressFill.fillAmount = 0f;
    }

    /// <summary>若 Awake 时尚未存在玩家（或执行顺序导致未找到），在飞币时再解析一次。</summary>
    PlayerController ResolvePlayerController()
    {
        if (_PlayerController == null)
            _PlayerController = FindObjectOfType<PlayerController>();
        return _PlayerController;
    }

    /// <summary>金币生成点：角色 CharacterController 包围盒顶部世界坐标，再沿 Y 加 <see cref="moneySpawnHeight"/>；水平位置取角色 pivot 的 XZ。</summary>
    Vector3 GetMoneySpawnBaseWorld()
    {
        PlayerController pc = ResolvePlayerController();
        if (pc == null)
            return transform.position;

        Vector3 pivot = pc.transform.position;
        CharacterController cc = pc.controller;
        if (cc != null)
            return new Vector3(pivot.x, cc.bounds.max.y + moneySpawnHeight, pivot.z);

        return pivot + Vector3.up * moneySpawnHeight;
    }

    /// <summary>
    /// 参考 FastFoodRush <c>UnlockableBuyer.PlayMoneyAnimation</c>：玩家头顶生成金币，DOJump 落到购买点；大额扣款用多枚错开模拟连续付款。
    /// </summary>
    void PlayMoneyFlyAnimations(int chunkAmount)
    {
        if (moneyFlyPrefab == null || ResolvePlayerController() == null)
            return;

        int count = Mathf.Clamp(chunkAmount, 1, Mathf.Max(1, maxVisualCoinsPerSpend));
        Vector3 targetPos = moneyFlyTarget != null ? moneyFlyTarget.position : transform.position;
        Vector3 playerBase = GetMoneySpawnBaseWorld();

        int session = _moneyFlySessionId;
        float stagger = Mathf.Max(0f, visualCoinStagger);

        for (int i = 0; i < count; i++)
        {
            int index = i;
            float delay = i * stagger;

            if (delay <= 0f)
                SpawnSingleMoneyFly(playerBase, targetPos, session, index == 0);
            else
                DOVirtual.DelayedCall(delay, () =>
                {
                    if (session != _moneyFlySessionId)
                        return;
                    bool playSfx = !coinFlySoundFirstOnly || index == 0;
                    SpawnSingleMoneyFly(playerBase, targetPos, session, playSfx);
                });
        }
    }

    void SpawnSingleMoneyFly(Vector3 playerBase, Vector3 targetPos, int session, bool playCoinSfx)
    {
        if (session != _moneyFlySessionId)
            return;

        Vector2 disk = Random.insideUnitCircle * moneySpawnSpread;
        Vector3 spawnPos = playerBase + new Vector3(disk.x, 0f, disk.y);

        GameObject moneyObj = Instantiate(moneyFlyPrefab);
        moneyObj.transform.position = spawnPos;

        if (moneySpawnPopScale)
        {
            Vector3 endScale = moneyObj.transform.localScale;
            moneyObj.transform.localScale = Vector3.zero;
            moneyObj.transform.DOScale(endScale, 0.08f).SetEase(Ease.OutBack);
        }

        moneyObj.transform
            .DOJump(targetPos, moneyJumpPower, 1, moneyJumpDuration)
            .SetEase(moneyJumpEase)
            .OnComplete(() => Destroy(moneyObj));

        if (playCoinSfx && !string.IsNullOrEmpty(coinFlySoundName) && AudioManager.Instance != null)
            AudioManager.Instance.PlayOneShot(coinFlySoundName, 0.65f);
    }

    void OnDestroy()
    {
        _moneyFlySessionId++;
    }
}
