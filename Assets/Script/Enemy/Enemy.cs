using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected Rigidbody2D rb;//protected保护，内部和子类可访问，外部不可访问
    [HideInInspector] public Animator anim;//[HideInInspector]用于隐藏
    [HideInInspector] public PhysicsCheck physicsCheck;

    [Header("基本参数")]
    public float normalSpeed;//基本移动参数
    public float chaseSpeed;//追击速度
    [HideInInspector] public float currentSpeed;//当前速度
    public Vector3 faceDir;
    public Transform attacker;
    public float hurtForce;

    [Header("检测")]
    public Vector2 centerOffset;//
    public Vector2 checkSize;//检测盒子的尺寸
    public LayerMask attackLayer;//检测的距离
    public float checkDistance;//图层

    [Header("计时器")]
    public float waitTime;
    public float waitTimeCounter;//记录当前的值
    public bool wait;
    public float lostTime;
    public float lostTimeCounter;

    [Header("状态")]
    public bool isHurt;
    public bool isDead;

    private BaseState currentState;
    protected BaseState patrolState;//巡逻
    protected BaseState chaseState;//追击

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        physicsCheck = GetComponent<PhysicsCheck>();

        currentSpeed = normalSpeed;
        //waitTimeCounter = waitTime; 

    }

    private void OnEnable()//周期函数
    {
        currentState = patrolState;//巡逻变追击
        currentState.OnEnter(this);
    }
    private void Update()
    {
        faceDir = new Vector3 (-transform.localScale.x, 0, 0);//
        //if (physicsCheck.touchLeftWall && faceDir.x < 0 || physicsCheck.touchRightWall && faceDir.x > 0)
        //{
        //    wait = true;
        //}
        //if (physicsCheck.touchLeftWall)
        //    transform.localScale = new Vector3(-1, 1, 1);//当接触左墙的时候向右
        //if (physicsCheck.touchRightWall)
        //    transform.localScale = new Vector3(1, 1, 1);//当接触右墙的时候向左
        currentState.LogicUpdate();
        Timecounter();

    }

    private void FixedUpdate()
    {
        if (!isHurt && !isDead && !wait) 
        Move();
        currentState.PhysicsUpdate();
    }

    private void OnDisable()//人物关闭，场景消失时执行一次
    {
        currentState.OnExit(); 
    }
    public virtual void Move()//virtual代表虚拟的，子类可以修改
    {
        rb.velocity = new Vector2 (currentSpeed*faceDir.x*Time.deltaTime, /*rb.velocity.y*/0);
    }
    /// <summary>
    /// 计时器
    /// </summary>
    public void Timecounter()//计时器
    {
        if(wait)
        {
            waitTimeCounter-= Time.deltaTime;
            if(waitTimeCounter <= 0)
            {
                wait = false;
                waitTimeCounter = waitTime;
                transform.localScale = new Vector3(faceDir.x, 1, 1);
            }
            
        }
        if (!FoundPlayer() && lostTimeCounter > 0)
        {
            lostTimeCounter -= Time.deltaTime;
        }

        else if (FoundPlayer()) 
        {
            lostTimeCounter = lostTime;//如果再次发现重置时间
        }
    }

    public bool FoundPlayer()//发射一个方形的判断器
    {
        return Physics2D.BoxCast(transform.position + (Vector3)centerOffset, checkSize, 0, faceDir, checkDistance, attackLayer);//可以用var temp检测返回值
 
    }

    public void SwitchState(NPCState state)//切换状态
    {
        var newState = state switch//一个语法糖写法
        {
            NPCState.Patrol => patrolState,
            NPCState.Chase => chaseState,
            _ => null
        };

        currentState.OnExit();//执行退出
        currentState = newState;
        currentState.OnEnter(this);
    }
    #region 事件执行方法
    public void OnTakeDamage(Transform attackTrans)
    {
        attacker = attackTrans;
        //转身
        if (attackTrans.position.x - transform.position.x > 0)
            transform.localScale = new Vector3(-1, 1, 1);
        if (attackTrans.position.x - transform.position.x < 0)
            transform.localScale = new Vector3(1, 1, 1);    
        //受伤被击退
        isHurt = true;
        anim.SetTrigger("hurt");
        Vector2 dir = new Vector2(transform.position.x - attackTrans.position.x, 0).normalized;
        rb.velocity = new Vector2(0, rb.velocity.y);
        StartCoroutine(OnHurt(dir));//启动携程
    }

    private IEnumerator OnHurt(Vector2 dir)//IEnumerator迭代器
    {
        rb.AddForce(dir * hurtForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(0.5f);//语法糖内容
        isHurt= false; 
    }

    public void OnDie()
    {
        gameObject.layer = 2;//死亡后改变图层防止发生碰撞
        anim.SetTrigger("dead");
        isDead = true;
    }

    public void DestroyAfterAnimation()//死亡后销毁
    {
        Destroy(this.gameObject);
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position + (Vector3)centerOffset+new Vector3(checkDistance*-transform.localScale.x,0), 0.2f);
    }
}

