using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;//调用事件

public class Character : MonoBehaviour
{
    [Header("基本属性")]
    public float maxHealth;//最大血量
    public float currentHealth;//当前血量


    [Header("受伤无敌")]
    public float invulnerableDurantion;//无敌时间
    [HideInInspector]private float invulnerableCounter;//计时器
    public bool invulnerable;

    public UnityEvent<Character> OnHealthChange;

    public UnityEvent<Transform> OnTakeDamage;
    public UnityEvent OnDie;

    private void Start()
    {
        currentHealth = maxHealth;//每次新游戏开始时血量回到最大
        OnHealthChange?.Invoke(this);
    }

    private void Update()
    {
        if (invulnerable)
        {
            invulnerableCounter -= Time.deltaTime/*完成上一帧所用的时间*/;
            if (invulnerableCounter <= 0)
            {
                invulnerable = false;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if(collision.CompareTag("Water"))
        {
            currentHealth = 0;
            OnHealthChange?.Invoke(this);
            OnDie?.Invoke();
        }
    }
    public void TakeDamage(Attack attacker)//接受伤害
    {
        if (invulnerable)//如果无敌则返回
        {
            return;
        }

        if (currentHealth - attacker.damage > 0)
        {
            currentHealth -= attacker.damage;
            TriggerInvulnerable();
            //执行受伤动画
            OnTakeDamage?.Invoke(attacker.transform);
            TriggerInvulnerable();

        }
        else//确保不会出现负值
        {
            currentHealth = 0;
            //触发死亡
            OnDie?.Invoke();
        }
        OnHealthChange?.Invoke(this);
        
    }

    private void TriggerInvulnerable()//触发无敌
    {
        if (!invulnerable)
        {
            invulnerable = true;//触发时如果不是无敌改为无敌
            invulnerableCounter = invulnerableDurantion;
        }
    }

}
