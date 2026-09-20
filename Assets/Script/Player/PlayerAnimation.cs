using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator anim;
    private Rigidbody2D rb;
    private PhysicsCheck physicsCheck;
    private PlayerController playerController;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();
        playerController = GetComponent<PlayerController>();    
    }

    private void Update()//因为每一帧都要执行，所以放在update里
    {
        SetAnimation();
    }
    public void SetAnimation()//执行所有的动画切换
    {
        anim.SetFloat("velocityX", Mathf.Abs(rb.velocity.x));//Mathf.Abs(取绝对值)
        anim.SetFloat("velocityY", rb.velocity.y);
        anim.SetBool("isGround", physicsCheck.isGround);
        anim.SetBool("isDead", playerController.isDead);
        anim.SetBool("isAttack",playerController.isAttack);
        anim.SetInteger("combo", playerController.combo);
    }

    public void PlayHurt()
    {
        anim.SetTrigger("hurt");
    }

    public void PlayAttack()
    {
        anim.SetTrigger("attack");
    }
}
