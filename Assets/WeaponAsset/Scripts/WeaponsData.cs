﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponAsset;

// TODO : 나중에 DataStore에서 분리할 예정

namespace WeaponAsset
{
    public delegate float DelGetDirDegree();    // 총구 방향 각도
    public delegate Vector3 DelGetPosition();   // owner position이지만 일단 player position 용도로만 사용.

    public enum WeaponState { Idle, Attack, Reload, Charge, Switch, PickAndDrop }
    /// <summary>
    /// 원거리 : 권총, 산탄총, 기관총, 저격소총, 레이저, 활, 지팡이, 원거리 특수
    /// 근거리 기반 : 창, 몽둥이, 스포츠용품, 검, 청소도구, 주먹장착무기, 근거리 특수
    /// 폭발형? : 폭탄, 접근발동무기
    /// 17개
    /// </summary>
    // END 는 WeaponType 총 갯수를 알리기 위해서 enum 맨 끝에 기입 했음.
    public enum WeaponType
    {
        NULL, PISTOL, SHOTGUN, MACHINEGUN, SNIPER_RIFLE, LASER, BOW,
        SPEAR, CLUB, SPORTING_GOODS, SWORD, CLEANING_TOOL, KNUCKLE,
        BOMB, TRAP,
        WAND, MELEE_SPECIAL, RANGED_SPECIAL, END
    }

    // PISTOL, SHOTGUN, MACHINEGUN, SNIPLER_RIFLE, LASER, BOW
    public enum AttackAniType { None, Blow, Strike, Swing, Punch, Shot }
    public enum AttackType { MELEE, RANGED, TRAP }
    public enum TouchMode { Normal, Charge }
    public enum BulletType { PROJECTILE, LASER, MELEE, NULL, MINE, EXPLOSION }
    public enum BulletPresetType
    {
        None, YELLOW_CIRCLE, RED_CIRCLE, SKYBLUE_BASH, BLUE_BASH, RED_BASH, ORANGE_BASH,
        BLUE_CIRCLE, SKYBLUECIRCLE, PINK_CIRCLE, VIOLET_CIRCLE, EMPTY,
        YELLOW_BULLET, BLUE_BULLET, PINK_BULLET, VIOLET_BULLET, RED_BULLET, GREEN_BULLET,
        YELLOW_BEAM, BLUE_BEAM, PINK_BEAM, VIOLET_BEAM, RED_BEAM, GREEN_BEAM, GREEN_CIRCLE,
        YELLOW_BULLET2, SKYBLUE_BULLET2, BLUE_BULLET2, PINK_BULLET2, VIOLET_BULLET2, RED_BULLET2, GREEN_BULLET2,
        TEST, TEST2, TEST3, TEST4
    };
    /*---*/
    public enum MuzzleParticelType { NONE, BASIC }
    public enum ImpactParticleType { NONE, BASIC }
    public enum TrailParticleType { NONE, BASIC }

    public enum BulletPropertyType { Collision, Update, Delete }
    public enum CollisionPropertyType { BaseNormal, Laser, Undeleted }
    public enum UpdatePropertyType { StraightMove, AccelerationMotion, Laser, Summon, Homing, MineBomb, FixedOwner, Spiral, Rotation, Child, TRIGONOMETRIC }
    public enum DeletePropertyType { BaseDelete, Laser, SummonBullet, SummonPattern }
    public enum BehaviorPropertyType { SpeedControl, Rotate }

    /*---*/

    public enum ColliderType { None, Beam, Box, Circle, SPECIFIC_BOX, SPECIFIC_CIRCLE }

    public enum BulletAnimationType
    {
        NotPlaySpriteAnimation,
        BashAfterImage,
        PowerBullet,
        Wind,
        BashAfterImage2,
        Explosion0,
        BashSkyBlue,
        BashBlue,
        BashRed,
        BashOrange,
        PaperShot
    }

    /*---*/


    // 총알 삭제 함수 델리게이트
    public delegate void DelDestroyBullet();
    // 총알 충돌 함수 델리게이트
    public delegate void DelCollisionBullet(Collider2D coll);
}

public class WeaponsData : MonoBehaviourSingleton<WeaponsData>
{
    #region variables
    [SerializeField]
    private WeaponInfo[] testWeaponInfos;
    [SerializeField]
    //[FormerlySerializedAs("tempWeaponInfos")]
    //이거 선언, 이전 이름, 새로운 변수 명 한 번에 해야됨.
    private WeaponInfo[] mainWeaponInfos;
    //[FormerlySerializedAs("밑에 이전 변수명")] 이전 변수명과 밑에 에는 새로운 변수 명 쳐줘야 안의 값 그대로 이동함. 직렬화 되어있어야 했던가?
    [SerializeField]
    private WeaponInfo[] shapeSampleWeaponInfos;
    [SerializeField]
    private WeaponInfo[] testBossWeaponInfos;
    [SerializeField]
    private WeaponInfo[] enemyWeaponInfos;
    private int ratingLength;

    [SerializeField]
    // 런타임 때 결정되는 정보들.
    private List<WeaponInfo>[] weaponInfoByRating;

    [Header("true하고 실행 시 엑셀 내용으로 무기 초기화")]
    [SerializeField]
    private bool canInputWeaponData;
    public List<Dictionary<string, object>> csvWeaponData;
    #endregion

    #region getter
    public int GetWeaponInfosLength()
    {
        switch (DebugSetting.Instance.weaponModeForDebug)
        {
            case WeaponModeForDebug.Test:
                return testWeaponInfos.Length;
            case WeaponModeForDebug.Main:
                return mainWeaponInfos.Length;
            case WeaponModeForDebug.ShapeSample:
                return shapeSampleWeaponInfos.Length;
            case WeaponModeForDebug.TestBoss:
                return testBossWeaponInfos.Length;
            default:
                break;
        }
        return 0;
    }

    public int GetEnemyWeaponInfosLength()
    {
        return enemyWeaponInfos.Length;
    }

    /// <summary>
    /// Player Weapon 중 해당 Rating에서 랜덤하게 무기 정보 얻기 
    /// </summary>
    /// <param name="rating"></param>
    /// <returns></returns>
    public WeaponInfo GetWeaponInfo(Rating rating)
    {
        if (Rating.NORATING == rating)
            return null;
        int ratingIndex = (int)(rating - 1);
        if (0 >= weaponInfoByRating[ratingIndex].Count)
            return null;
        int weaponIndex = Random.Range(0, weaponInfoByRating[ratingIndex].Count);
        return weaponInfoByRating[ratingIndex][weaponIndex];
    }

    /// <summary>
    /// Owner에 따른 Weapon Data 반환, ownerType 기본 값 Player
    /// </summary>
    /// <param name="id"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    public WeaponInfo GetWeaponInfo(int id, CharacterInfo.OwnerType ownerType)
    {
        // player용 switch 안에 switch 못해서 따로 떼어놓음.
        if (CharacterInfo.OwnerType.PLAYER == ownerType)
        {
            switch (DebugSetting.Instance.weaponModeForDebug)
            {
                case WeaponModeForDebug.Test:
                    return testWeaponInfos[id];
                case WeaponModeForDebug.Main:
                    return mainWeaponInfos[id];
                case WeaponModeForDebug.ShapeSample:
                    return shapeSampleWeaponInfos[id];
                case WeaponModeForDebug.TestBoss:
                    return testBossWeaponInfos[id];
                default:
                    break;
            }
        }
        switch (ownerType)
        {
            case CharacterInfo.OwnerType.ENEMY:
                return enemyWeaponInfos[id];
            case CharacterInfo.OwnerType.OBJECT:
            default:
                break;
        }
        return null;
    }
    #endregion

    #region unityFunc
    void Awake()
    {
        InitWepaonInfo();
    }
    #endregion

    #region func
    /// <summary> 무기 정보 관련 초기화 </summary>
    public void InitWepaonInfo()
    {
        ratingLength = (int)Rating.E;
        weaponInfoByRating = new List<WeaponInfo>[ratingLength];
        for (int i = 0; i < weaponInfoByRating.Length; i++)
        {
            weaponInfoByRating[i] = new List<WeaponInfo>();
        }

        switch (DebugSetting.Instance.weaponModeForDebug)
        {
            case WeaponModeForDebug.Test:
                for (int i = 0; i < testWeaponInfos.Length; i++)
                    testWeaponInfos[i].Init();
                break;
            case WeaponModeForDebug.Main:
                for (int i = 0; i < mainWeaponInfos.Length; i++)
                {
                    mainWeaponInfos[i].Init();
                    mainWeaponInfos[i].SetWeaponId(i);
                    InsertWeaponInfoByRating(mainWeaponInfos[i]);
                }
                break;
            case WeaponModeForDebug.ShapeSample:
                for (int i = 0; i < shapeSampleWeaponInfos.Length; i++)
                    shapeSampleWeaponInfos[i].Init();
                break;
            case WeaponModeForDebug.TestBoss:
                for (int i = 0; i < testBossWeaponInfos.Length; i++)
                    testBossWeaponInfos[i].Init();
                break;
            default:
                break;
        }

        for (int i = 0; i < enemyWeaponInfos.Length; i++)
        {
            enemyWeaponInfos[i].Init();
        }
        InputWeaponData();
    }

    private void InsertWeaponInfoByRating(WeaponInfo weaponInfo)
    {
        if (Rating.NORATING == weaponInfo.rating)
        {
            return;
        }
        int rating = (int)weaponInfo.rating - 1;
        // notRating 일 때 넣지 않음.
        weaponInfoByRating[rating].Add(weaponInfo);
    }

    public void InputWeaponData()
    {
        if (WeaponModeForDebug.Test == DebugSetting.Instance.weaponModeForDebug
            || WeaponModeForDebug.ShapeSample == DebugSetting.Instance.weaponModeForDebug)
            return;

        if (false == canInputWeaponData)
            return;
        csvWeaponData = WeaponDataCSVParser.Read("weaponData");
        //Debug.Log("CSV 데이터 파싱 후 weapon data 입력");

        #region variables
        WeaponType weaponType;
        AttackAniType attackAniType;
        Rating rating;
        float chargeTimeMax;
        float criticalChance;
        float damage;
        int staminaConsumption;
        float cooldown;
        int ammoCapacity;
        float range;
        float bulletSpeed;
        int soundId;
        float scaleX;
        float scaleY;
        float castingTime;
        float addDirVec;
        float addVerticalVec;
        #endregion

        //TODO: soundID, showsMuzzleFalsh, ScaleX, ScaleY, CastingTime, AddDirVec, AddVerticalVec

        int size = csvWeaponData.Count;
        for (int i = 0; i < size; i++)
        {
            mainWeaponInfos[i].weaponName = (string)csvWeaponData[i]["name"];
            //Debug.Log(i + ", name : " + (string)csvWeaponData[i]["name"]);

            weaponType = (WeaponType)System.Enum.Parse(typeof(WeaponType), (string)csvWeaponData[i]["weaponType"]);
            mainWeaponInfos[i].weaponType = weaponType;

            attackAniType = (AttackAniType)System.Enum.Parse(typeof(AttackAniType), (string)csvWeaponData[i]["attackAniType"]);
            mainWeaponInfos[i].attackAniType = attackAniType;
            //Debug.Log(attackAniType);

            rating = (Rating)System.Enum.Parse(typeof(Rating), (string)csvWeaponData[i]["rating"]);
            mainWeaponInfos[i].rating = rating;
            //Debug.Log(rating);

            float.TryParse(csvWeaponData[i]["chargeTimeMax"].ToString(), out chargeTimeMax);
            mainWeaponInfos[i].chargeTimeMax = chargeTimeMax;
            //Debug.Log(chargeTimeMax);
            if (0 == chargeTimeMax)
                mainWeaponInfos[i].touchMode = TouchMode.Normal;
            else
                mainWeaponInfos[i].touchMode = TouchMode.Charge;

            float.TryParse(csvWeaponData[i]["criticalChance"].ToString(), out criticalChance);
            mainWeaponInfos[i].criticalChance = criticalChance;
            //Debug.Log(criticalChance);

            float.TryParse(csvWeaponData[i]["damage"].ToString(), out damage);
            mainWeaponInfos[i].damage = damage;
            //Debug.Log(damage);

            int.TryParse(csvWeaponData[i]["staminaConsumption"].ToString(), out staminaConsumption);
            mainWeaponInfos[i].staminaConsumption = staminaConsumption;
            //Debug.Log(staminaConsumption);

            float.TryParse(csvWeaponData[i]["cooldown"].ToString(), out cooldown);
            mainWeaponInfos[i].cooldown = cooldown;
            //Debug.Log(cooldown);

            int.TryParse(csvWeaponData[i]["ammoCapacity"].ToString(), out ammoCapacity);
            mainWeaponInfos[i].ammoCapacity = ammoCapacity;
            mainWeaponInfos[i].ammo = ammoCapacity;
            //Debug.Log(ammoCapacity);

            float.TryParse(csvWeaponData[i]["range"].ToString(), out range);
            mainWeaponInfos[i].range = range;
            //Debug.Log(range);

            float.TryParse(csvWeaponData[i]["bulletSpeed"].ToString(), out bulletSpeed);
            mainWeaponInfos[i].bulletMoveSpeed = bulletSpeed;
            //Debug.Log(bulletSpeed);

            int.TryParse(csvWeaponData[i]["soundId"].ToString(), out soundId);
            mainWeaponInfos[i].soundId = soundId;

            if ("TRUE" == csvWeaponData[i]["soundId"].ToString())
                mainWeaponInfos[i].showsMuzzleFlash = true;
            else
                mainWeaponInfos[i].showsMuzzleFlash = false;

            float.TryParse(csvWeaponData[i]["scaleX"].ToString(), out scaleX);
            mainWeaponInfos[i].scaleX = scaleX;

            float.TryParse(csvWeaponData[i]["scaleY"].ToString(), out scaleY);
            mainWeaponInfos[i].scaleY = scaleY;

            float.TryParse(csvWeaponData[i]["castingTime"].ToString(), out castingTime);
            mainWeaponInfos[i].castingTime = castingTime;

            float.TryParse(csvWeaponData[i]["addDirVec"].ToString(), out addDirVec);
            mainWeaponInfos[i].addDirVecMagnitude = addDirVec;

            float.TryParse(csvWeaponData[i]["addVerticalVec"].ToString(), out addVerticalVec);
            mainWeaponInfos[i].additionalVerticalPos = addVerticalVec;


            switch (weaponType)
            {
                case WeaponType.PISTOL:
                case WeaponType.SHOTGUN:
                case WeaponType.MACHINEGUN:
                case WeaponType.SNIPER_RIFLE:
                    mainWeaponInfos[i].showsMuzzleFlash = true;
                    break;
                case WeaponType.LASER:
                    //mainWeaponInfos[i].cooldown = 0f;
                    mainWeaponInfos[i].cameraShakeAmount = 0f;
                    mainWeaponInfos[i].cameraShakeTime = 0f;
                    break;
                default:
                    mainWeaponInfos[i].showsMuzzleFlash = false;
                    break;
            }

            switch (weaponType)
            {
                case WeaponType.PISTOL:
                case WeaponType.SHOTGUN:
                case WeaponType.MACHINEGUN:
                case WeaponType.SNIPER_RIFLE:
                case WeaponType.BOW:
                case WeaponType.WAND:
                case WeaponType.RANGED_SPECIAL:
                    mainWeaponInfos[i].cameraShakeAmount = 0.1f;
                    mainWeaponInfos[i].cameraShakeTime = 0.1f;
                    break;
                case WeaponType.SPEAR:
                case WeaponType.CLUB:
                case WeaponType.SPORTING_GOODS:
                case WeaponType.SWORD:
                case WeaponType.CLEANING_TOOL:
                case WeaponType.KNUCKLE:
                    mainWeaponInfos[i].cameraShakeAmount = 0.1f;
                    mainWeaponInfos[i].cameraShakeTime = 0.04f;
                    break;
                case WeaponType.BOMB:
                case WeaponType.TRAP:
                    mainWeaponInfos[i].cameraShakeAmount = 0f;
                    mainWeaponInfos[i].cameraShakeTime = 0f;
                    break;
                default:
                    break;
            }

            switch (weaponType)
            {
                case WeaponType.SPEAR:
                case WeaponType.CLUB:
                case WeaponType.SPORTING_GOODS:
                case WeaponType.SWORD:
                case WeaponType.CLEANING_TOOL:
                    if (addDirVec == 0)
                        mainWeaponInfos[i].addDirVecMagnitude = 1.2f;
                    break;
                case WeaponType.KNUCKLE:
                case WeaponType.SHOTGUN:
                case WeaponType.BOW:
                case WeaponType.WAND:
                case WeaponType.SNIPER_RIFLE:
                    if (addDirVec == 0)
                        mainWeaponInfos[i].addDirVecMagnitude = 0.5f;
                    break;
                case WeaponType.LASER:
                case WeaponType.MACHINEGUN:
                case WeaponType.RANGED_SPECIAL:
                    if (addDirVec == 0)
                        mainWeaponInfos[i].addDirVecMagnitude = 0.3f;
                    break;
                default:
                    break;
            }

            //시전 시간
            switch (mainWeaponInfos[i].attackAniType)
            {
                case AttackAniType.Strike:
                    mainWeaponInfos[i].soundId = 0;
                    mainWeaponInfos[i].castingTime = 0.3f;
                    break;
                case AttackAniType.Blow:
                    mainWeaponInfos[i].castingTime = 0.2f;
                    mainWeaponInfos[i].soundId = 3;
                    break;
                case AttackAniType.Swing:
                    mainWeaponInfos[i].castingTime = 0.3f;
                    mainWeaponInfos[i].soundId = 3;
                    break;
                case AttackAniType.Shot:
                    mainWeaponInfos[i].soundId = 0;
                    break;
                default:
                    break;
            }

            //sound
            switch (weaponType)
            {
                case WeaponType.BOMB:
                case WeaponType.TRAP:
                    mainWeaponInfos[i].soundId = 3;
                    break;
                case WeaponType.SHOTGUN:
                    mainWeaponInfos[i].soundId = 2;
                    break;
                case WeaponType.LASER:
                    mainWeaponInfos[i].soundId = -1;
                    break;
                default:
                    break;
            }
        }

    }
    #endregion
}