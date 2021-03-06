﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WeaponAsset;
// BulletProperty.cs

[System.Serializable]
public abstract class BulletProperty
{
    protected Bullet bullet;
    protected GameObject bulletObj;
    protected GameObject colliderObj;
    protected Transform bulletTransform;
    protected DelDestroyBullet delDestroyBullet;
    protected BuffManager ownerBuff;
    protected TransferBulletInfo transferBulletInfo;
    protected StatusEffectInfo statusEffectInfo;
    protected DelCollisionBullet delCollisionBullet;

    /// <summary> bullet class에 정보를 받아와서 속성에 맞는 초기화 </summary>
    /// <param name="bullet">해당 Property가 포함된 Bullet</param>
    public virtual void Init(Bullet bullet)
    {
        this.bullet = bullet;
        bulletObj = bullet.gameObject;
        colliderObj = bullet.GetColliderObj();
        bulletTransform = bullet.objTransform;
        delDestroyBullet = bullet.DestroyBullet;
        ownerBuff = bullet.GetOwnerBuff();
        transferBulletInfo = bullet.GetTransferBulletInfo();
        statusEffectInfo = bullet.GetStatusEffectInfo();
    }
    //protected WeaponState.Owner owner;
}