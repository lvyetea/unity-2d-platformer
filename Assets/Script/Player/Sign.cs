using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.iOS;
using UnityEngine.tvOS;

public class Sign : MonoBehaviour
{
    private PlayerInputControl playerInput;

    private Animator anim;

    public Transform playerTrans;

    public GameObject signSprite;

    private bool canPress;

    private Iinteractable targetItem;
    

    private void Awake()
    {
        //anim = GetComponentInChildren<Animator>();两种方法都可，由于一开始sign隐藏，所以用第二种。
        anim = signSprite.GetComponent<Animator>();

        playerInput = new PlayerInputControl();
        playerInput.Enable();
    }
    private void OnEnable()
    {
        InputSystem.onActionChange += OnActionChange;
        playerInput.Gameplay.Confirm.started += OnConfirm;
    }

    private void OnDisable()
    {
        canPress = false;
    }


    private void Update()
    {
        signSprite.GetComponent<SpriteRenderer>().enabled = canPress;
        signSprite.transform.localScale = playerTrans.localScale;
    }
    /// <summary>
    /// 切换设备同时切换动画
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="actionChange"></param>
    private void OnActionChange(object obj, InputActionChange actionChange)
    {
        if(actionChange == InputActionChange.ActionStarted)
        {
            var d = ((InputAction)obj).activeControl.device;

            switch (d.device)
            {
                case Keyboard:
                    anim.Play("KeyBoard");
                    break;
                
            }
        }
        
    }

    private void onActionChange(object obj, InputActionChange actionChange)
    {
        if (actionChange == InputActionChange.ActionStarted)
        {
            Debug.Log((InputAction)obj);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("interactable"))
        {
            canPress = true;
            targetItem = collision.GetComponent<Iinteractable>();
        }
    }

    private void OnConfirm(InputAction.CallbackContext context)
    {
        if (canPress) 
        {
            targetItem.TriggerAction();
            GetComponent<AudioDefination>()?.PlayAudioClip();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        canPress = false;
    }
}
