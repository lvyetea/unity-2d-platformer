using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boar : Enemy//继承父类Enemy
{
    //public override void Move()//覆写
    //{
    //    base.Move();
    //    anim.SetBool("walk", true);//播放指定动画Enemy-walk
    //}

    protected override void Awake()
    {
        base.Awake();
        patrolState = new BoarPatrolState();
        chaseState = new BoarChaseState();
    }
}
 