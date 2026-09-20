using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{


    private CapsuleCollider2D coll;
    [Header("检测参数")]
    public bool manual;//手动

    public Vector2 bottomOffset;//脚底的位移差值

    public Vector2 leftOffset;//左位置偏移

    public Vector2 rightOffset;

    public float checkRadius;//检测范围

    public LayerMask groundLayer;

 
    [Header("状态")]
    public bool isGround;//创建变量表示人物状态

    public bool touchLeftWall;//检测左边有没有墙

    public bool touchRightWall;//检测右墙

    private void Awake()
    {
        coll = GetComponent<CapsuleCollider2D>();

        if(!manual)
        {
            rightOffset = new Vector2((coll.bounds.size.x + coll.offset.x) / 2, coll.bounds.size.y / 2);
            leftOffset = new Vector2(-rightOffset.x, rightOffset.y);
        }
    }
    private void Update()
    {
        Check();//需要持续不断检查
    }
    public void Check()//创建一个函数方法
    {
        //检测地面
        isGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, checkRadius, groundLayer);
        //if(isGround) 
        //    //PlayerController.jumpCounter = true;
        //墙体判断
        touchLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, checkRadius, groundLayer);
        touchRightWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, checkRadius, groundLayer);

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, checkRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, checkRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, checkRadius);
    }
}
