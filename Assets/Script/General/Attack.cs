using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public int damage;//攻击伤害
    public float attackRange;//攻击范围
    public float attackRate;//攻击频率

    private void OnTriggerStay2D(Collider2D collision)//在两个触发器碰撞接触时判定为攻击
    {
        collision.GetComponent<Character>()?.TakeDamage(this);//?相当于不为空的判断!=null，在此判断对面是否有这个函数，如果过没有会报错
    }
}
