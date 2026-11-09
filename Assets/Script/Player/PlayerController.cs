using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    public PlayerInputControl inputControl;//小驼峰命名
    private Rigidbody2D rb;//获取rigidbody
    public Vector2 inputDirection;
    public PhysicsCheck physicsCheck;
    public PlayerAnimation playerAnimation;
    public CapsuleCollider2D coll;

    [Header("基本参数")]
    public float speed;

    public float jumpForce;

    public float hurtForce;

    public int combo;



    [Header("物理材质")]
    public PhysicsMaterial2D Wall;

    public PhysicsMaterial2D Normal;


    [Header("状态")]
    public bool isHurt;

    public bool isDead;

    public  bool isAttack;

    private bool jumpRequested;
    private bool jumpConsumed;
    private readonly ContactPoint2D[] groundContacts = new ContactPoint2D[8];
    private void Awake()
    {
        inputControl = new PlayerInputControl();
        
        physicsCheck = GetComponent<PhysicsCheck>();

        rb = GetComponent<Rigidbody2D>();

        playerAnimation = GetComponent<PlayerAnimation>();
        //跳跃输入只登记请求，由物理帧统一处理。
        inputControl.Gameplay.Jump.started += Jump;

        //攻击
        inputControl.Gameplay.Attack.started += PlayerAttack;

        coll = GetComponent<CapsuleCollider2D>();

    }

    

    private void OnEnable()
    {
        inputControl.Enable();


    }

    private void OnDisable()
    {
        jumpRequested = false;
        inputControl.Disable();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }
    //public bool JumpCounter()
    //{

    //}


    // Update is called once per frame
    private void Update()//每一帧都会执行,检测键盘输入
    {
        inputDirection = inputControl.Gameplay.Move.ReadValue<Vector2>();//获取键盘输入

        CheckState();
    }

    private void FixedUpdate()
    {
        physicsCheck.Check();
        ProcessJump();

        if (!isHurt && !isAttack) 
            Move();

    }

    //测试
    //private void OnTriggerStay2D(Collider2D collision)
    //{
    //    Debug.Log(collision.name);
    //}


    //任务基本移动
    public void Move()
    {
        rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime, rb.velocity.y);//获取的输入×速度×时间修正

        int faceDirection = (int)/*强制转换*/transform.localScale.x;

        if(inputDirection.x > 0 )
        {
            faceDirection = 1;
        }
        if(inputDirection.x < 0 )
        {
            faceDirection = -1;
        }
        //人物反转
        transform.localScale = new Vector3(faceDirection, 1, 1);
    }
    private void Jump(InputAction.CallbackContext context)
    {
        if (jumpRequested || jumpConsumed || isDead || isHurt || isAttack)
            return;

        physicsCheck.Check();
        if (HasGroundSupport())
            jumpRequested = true;
    }

    private void ProcessJump()
    {
        // 需要实际落地支撑，擦墙、贴顶或仍在上升时都不能恢复跳跃。
        if (HasGroundSupport())
            jumpConsumed = false;

        if (!jumpRequested)
            return;

        jumpRequested = false;
        if (jumpConsumed || isDead || isHurt || isAttack || !HasGroundSupport())
            return;

        jumpConsumed = true;
        rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
        GetComponent<AudioDefination>()?.PlayAudioClip();
    }

    private bool HasGroundSupport()
    {
        if (!physicsCheck.isGround || rb.velocity.y > 0f)
            return false;

        int count = coll.GetContacts(groundContacts);
        for (int i = 0; i < count; i++)
        {
            ContactPoint2D contact = groundContacts[i];
            Collider2D other = contact.collider == coll ? contact.otherCollider : contact.collider;
            if (other != null &&
                (physicsCheck.groundLayer.value & (1 << other.gameObject.layer)) != 0 &&
                contact.normal.y >= 0.5f && contact.separation <= 0.01f)
                return true;
        }

        return false;
    }

    private void PlayerAttack(InputAction.CallbackContext context)
    {
        playerAnimation.PlayAttack();
        isAttack = true;
        //combo++;
        //if (combo >= 3)
        //{
        //    combo = 0;
        //}

        }

    #region UnityEvent
    public void GetHurt(Transform attacker)
    {
        isHurt = true;
        rb.velocity = Vector2.zero;//速度归零
        Vector2 dir = new Vector2((transform.position.x - attacker.position.x), 0).normalized;

        rb.AddForce(dir * hurtForce, ForceMode2D.Impulse);
    }

    public void PlayerDead()
    {
        isDead = true;
        inputControl.Gameplay.Disable();
    }

    #endregion

    public void CheckState()
    {
        coll.sharedMaterial = physicsCheck.isGround ? Normal : Wall;//三元运算符，是则Normal，不是则为Wall
    }
}