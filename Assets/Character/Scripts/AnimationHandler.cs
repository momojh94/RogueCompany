﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimationHandler : MonoBehaviour {

    Animator animator;

    public void Init(RuntimeAnimatorController  animatorController)
    {
        this.animator = GetComponent<Animator>();
        this.animator.runtimeAnimatorController = animatorController;
        Play();
        Idle();
    }

    public void Idle()
    {
        animator.SetTrigger("idle");
        animator.SetInteger("skill", -1);
    }

    public void Attack()
    {
        animator.SetTrigger("attack");
    }

    public void Skill(int i)
    {
        animator.SetInteger("skill", i);
    }

    public void Attacked()
    {
        animator.SetTrigger("attaked");
    }

    public void Walk()
    {
        animator.SetTrigger("walk");
    }

    public void Run()
    {
        animator.SetTrigger("run");
    }

    public void Play()
    {
        animator.enabled = true;
    }

    public void Stop()
    {
        animator.enabled = false;
    }

    private void EndAnimation()
    {

    }
}
