﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponAsset;

/*
 * 새 변수 추가시 Clone에도 꼭 추가해줘야 됨.
 */

[CreateAssetMenu(fileName = "BulletInfo", menuName = "GameData/BulletInfo", order = 4)]
public class BulletInfo : ScriptableObject
{
    [Tooltip("적용하는 곳이나, 사용하는 사람이나, 개발시 필요한 정보 등, 기타 등등 메모할 공간")]
    [SerializeField]
    [TextArea(3, 100)] private string memo;

    [Header("Owner 꼭 설정 해주세요")]
    [SerializeField] protected OwnerType ownerType;

    [SerializeField]
    private string bulletName;  // 총알 이름, (메모 용)
    public float damage;
    public float knockBack;
    public float criticalRate;

    public float speed;         // 속력
    public float acceleration;  // 가속도
    public float deltaSpeedTotalLimit;    // 속력이 변화하는 총 값 제한, ex) a = -1, limit = 10, 속력 v = 3-> -7까지만 영향받음. a = +2 limit 8, v = -2 => +6까지만
    public float range;         // 사정거리
    public float scaleX;
    public float scaleY;

    public int pierceCount;
    public int bounceCount;

    [Header("-1이면 적용 X, 0 초과 값이면 lifeTime 만큼 시간 지나고 Delete속성 실행 ")]
    public float lifeTime;
    [Header("-1이면 effect 적용X, 0이상 만 적용")]
    public int effectId;    // 충돌 후 삭제시 생성될 effect

    [Tooltip("총알 생성 시 발생 소리, 0이상 이면 적용, ex: 폭발 총알")]
    public int soundId;

    [Header("Not Play Animation이 아니면 해당 애니메이션 적용")]
    public BulletAnimationType spriteAnimation;
    [Header("spriteAnimation이 Not Play Animation 일 때 사용할 bullet Sprite")]
    public Sprite bulletSprite;

    [Header("scale animation 적용 유무")]
    public bool showsScaleAnimation;
    [Header("rotation animation 적용 유무")]
    public bool showsRotationAnimation;
    [Header("particle 적용 유무")]
    public bool showsParticle;
    [Header("각도(rotation) 고정 유무")]
    public bool isFixedAngle;
    [Header("넉백 방향 True : position 차이 기반, False : Bullet direction 기반")]
    public bool positionBasedKnockBack;


    [Header("DeleteAfterSummonBulletProperty 에서 생성할 bullet id")]
    public int deleteAfterSummonBulletId;
    [Header("DeleteAfterSummonPatternProperty에서 생성할 pattern id")]
    public int deleteAfterSummonPatternId;


    // 튕기는 총알 테스트용, 반사 o / x
    public bool bounceAble;
    

    public CollisionPropertyType[] collisionPropertiesEdit; // 충돌 속성 edit용
    public UpdatePropertyType[] updatePropertiesEdit;       // update 속성 edit용
    public DeletePropertyType[] deletePropertiesEdit;       // 삭제 속성 edit용

    // 실제로 쓰일 속성 정보, 추가 삭제에 용이하게 List<T>
    public List<CollisionProperty> collisionProperties; // 충돌 속성 원본
    public List<UpdateProperty> updateProperties;       // update 속성 원본
    public List<DeleteProperty> deleteProperties;       // 삭제 속성 원본
    [HideInInspector]
    public int collisionPropertiesLength;               // 충돌 속성 길이
    [HideInInspector]
    public int updatePropertiesLength;                  // update 속성 길이
    [HideInInspector]
    public int deletePropertiesLength;                  // 삭제 속성 길이


    [Header("SummonUpdate 속성 전용 매개 변수")]
    // summonUpdate 속성 전용, 소환할 bulletPattern, 생성 주기
    public BulletPatternEditInfo summonBulletPattern;
    [Header("SummonUpdate 속성 전용 매개 변수, 생성 주기")]
    public float creationCycle;

    [Header("DeletAfterSummonPattern 속성 전용 매개 변수")]
    // summonUpdate 속성 전용, 소환할 bulletPattern, 생성 주기
    public BulletPatternEditInfo deleteAfterSummonPattern;


    // 새로운 속성 만들면 clone 추가 무조건 해줘야 됨.
    public BulletInfo()
    {
        scaleX = 1.0f;
        scaleY = 1.0f;

        pierceCount = 1;
        bounceCount = 0;

        lifeTime = -1;
        effectId = -1;
        soundId = -1;

        bulletSprite = null;

        showsScaleAnimation = false;
        showsRotationAnimation = false;
        showsParticle = false;
        isFixedAngle = false;

        bounceAble = false;
    }

    /*
    public BulletInfo(BulletInfo info)
    {
        this.bulletName = info.bulletName;
        this.damage = info.damage;
        this.speed = info.speed;
        this.range = info.range;
        this.scaleX = info.scaleX;
        this.scaleY = info.scaleY;

        collisionPropertiesLength = info.collisionPropertiesLength;
        updatePropertiesLength = info.updatePropertiesLength;
        deletePropertiesLength = info.deletePropertiesLength;

        collisionProperties = new List<CollisionProperty>();
        updateProperties = new List<UpdateProperty>();
        deleteProperties = new List<DeleteProperty>();

        // 총알 충돌 속성 초기화
        for (int i = 0; i < collisionPropertiesLength; i++)
        {
            collisionProperties.Add(info.collisionProperties[i].Clone());
        }
        // 총알 이동 속성 초기화
        for (int i = 0; i < updatePropertiesLength; i++)
        {
            updateProperties.Add(info.updateProperties[i].Clone());
        }
        // 총알 삭제 속성 초기화
        for (int i = 0; i < deletePropertiesLength; i++)
        {
            deleteProperties.Add(info.deleteProperties[i].Clone());
        }
    }
    */

    // 복사 생성자 쓸랬다가 로그 보는 곳에 BulletInfo must be 
    // instantiated using the ScriptableObject.CreateInstance method instead of new BulletInfo.
    // 떠서 Clone으로 새로운 클래스 본떠 만들어 리턴하는 형태로 바꿈.
    public BulletInfo Clone()
    {
        BulletInfo info = CreateInstance<BulletInfo>();

        info.ownerType = ownerType;

        info.bulletName = bulletName;
        info.damage = damage;
        info.knockBack = knockBack;
        info.criticalRate = criticalRate;

        info.speed = speed;
        info.acceleration = acceleration;
        info.deltaSpeedTotalLimit = deltaSpeedTotalLimit;
        info.range = range;
        info.scaleX = scaleX;
        info.scaleY = scaleY;

        info.pierceCount = pierceCount;
        info.bounceCount = bounceCount;

        info.lifeTime = lifeTime;
        info.effectId = effectId;
        info.soundId = soundId;

        info.spriteAnimation = spriteAnimation;
        info.bulletSprite = bulletSprite;

        info.showsScaleAnimation = showsScaleAnimation;
        info.showsRotationAnimation = showsRotationAnimation;
        info.showsParticle = showsParticle;
        info.isFixedAngle = isFixedAngle;
        info.positionBasedKnockBack = positionBasedKnockBack;

        info.deleteAfterSummonBulletId = deleteAfterSummonBulletId;
        info.deleteAfterSummonPatternId = deleteAfterSummonPatternId;
        info.bounceAble = bounceAble;


        /*---*/

        info.collisionPropertiesLength = collisionPropertiesLength;
        info.updatePropertiesLength = updatePropertiesLength;
        info.deletePropertiesLength = deletePropertiesLength;

        info.collisionProperties = new List<CollisionProperty>();
        info.updateProperties = new List<UpdateProperty>();
        info.deleteProperties = new List<DeleteProperty>();

        // 총알 충돌 속성 초기화
        for (int i = 0; i < info.collisionPropertiesLength; i++)
        {
            info.collisionProperties.Add(collisionProperties[i].Clone());
        }
        // 총알 이동 속성 초기화
        for (int i = 0; i < info.updatePropertiesLength; i++)
        {
            info.updateProperties.Add(updateProperties[i].Clone());
        }
        // 총알 삭제 속성 초기화
        for (int i = 0; i < info.deletePropertiesLength; i++)
        {
            info.deleteProperties.Add(deleteProperties[i].Clone());
        }

        return info;
    }

    /// <summary>
    /// edit시 enum으로 처리한 속성들 실제로 collision, update, delete 속성 정보로 만듬
    /// </summary>
    public void Init()
    {
        collisionPropertiesLength = collisionPropertiesEdit.Length;
        updatePropertiesLength = updatePropertiesEdit.Length;
        deletePropertiesLength = deletePropertiesEdit.Length;

        collisionProperties = new List<CollisionProperty>();
        updateProperties = new List<UpdateProperty>();
        deleteProperties = new List<DeleteProperty>();

        for (int i = 0; i < collisionPropertiesLength; i++)
        {
            switch (collisionPropertiesEdit[i])
            {
                case CollisionPropertyType.BaseNormal:
                    collisionProperties.Add(new BaseNormalCollisionProperty());
                    break;
                case CollisionPropertyType.Laser:
                    collisionProperties.Add(new LaserCollisionProperty());
                    break;
                case CollisionPropertyType.Undeleted:
                    collisionProperties.Add(new UndeletedCollisionProperty());
                    break;
                default:
                    break;
            }
        }

        for (int i = 0; i < updatePropertiesLength; i++)
        {
            switch (updatePropertiesEdit[i])
            {
                case UpdatePropertyType.StraightMove:
                    updateProperties.Add(new StraightMoveProperty());
                    break;
                case UpdatePropertyType.AccelerationMotion:
                    updateProperties.Add(new AccelerationMotionProperty());
                    break;
                case UpdatePropertyType.Laser:
                    updateProperties.Add(new LaserUpdateProperty());
                    break;
                case UpdatePropertyType.Summon:
                    BulletPattern argumentBulletPattern;
                    switch (summonBulletPattern.type)
                    {
                        case BulletPatternType.MultiDirPattern:
                            argumentBulletPattern = new MultiDirPattern(summonBulletPattern.id, summonBulletPattern.executionCount, summonBulletPattern.delay, ownerType);
                            break;
                        case BulletPatternType.RowPattern:
                            argumentBulletPattern = new RowPattern(summonBulletPattern.id, summonBulletPattern.executionCount, summonBulletPattern.delay, ownerType);
                            break;
                        case BulletPatternType.LaserPattern:
                            argumentBulletPattern = new LaserPattern(summonBulletPattern.id, ownerType);
                            break;
                        default:
                            argumentBulletPattern = null;
                            break;
                    }
                    updateProperties.Add(new SummonProperty(argumentBulletPattern, creationCycle));
                    break;
                case UpdatePropertyType.Homing:
                    updateProperties.Add(new HomingProperty());
                    break;
                default:
                    break;
            }
        }

        for (int i = 0; i < deletePropertiesLength; i++)
        {
            switch (deletePropertiesEdit[i])
            {
                case DeletePropertyType.BaseDelete:
                    deleteProperties.Add(new BaseDeleteProperty());
                    break;
                case DeletePropertyType.Laser:
                    deleteProperties.Add(new LaserDeleteProperty());
                    break;
                case DeletePropertyType.SummonBullet:
                    deleteProperties.Add(new DeleteAfterSummonBulletProperty());
                    break;
                case DeletePropertyType.SummonPattern:
                    deleteProperties.Add(new DeleteAfterSummonPatternProperty());
                    //
                    break;
                default:
                    break;
            }
        }
    }

}
