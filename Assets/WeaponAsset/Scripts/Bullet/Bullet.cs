﻿using System.Collections;
using UnityEngine;
using WeaponAsset;
using CharacterInfo;

/// <summary>
/// Bullet Class
/// 
/// bullet 하나에 몰아서 만들고 if문으로 총알을 나눌 수도 있고 아니면 나중에 총알 종류마다 클래스 나누어서
/// bullet 상속받고 나뉠수도 있음, 일반 총알, 레이저 총알 따로 만드는 식으로
/// </summary>

public class Bullet : MonoBehaviour
{
    #region variables
    public BulletInfo info;
    public Transform objTransform;
    public BoxCollider2D boxCollider;
    public CircleCollider2D circleCollider;
    public Rigidbody2D objRigidbody;

    // 레이저용 lineRenderer
    [SerializeField] private LineRenderer lineRenderer;

    // spirte, 애니메이션 용 sprite 포함 object
    [SerializeField] private Transform viewTransform;
    [SerializeField] private GameObject spriteAnimatorObj;
    [SerializeField] private SpriteRenderer spriteAniRenderer;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Animator animator;
    [SerializeField] private GameObject paticleObj;


    [SerializeField] private GameObject laserViewObj;
    [SerializeField] private Transform laserStartPoint;
    [SerializeField] private Animator laserStartPointAnimator;
    [SerializeField] private Transform laserEndPoint;
    [SerializeField] private Animator laserEndPointAnimator;

    private Coroutine bulletUpdate;
    private Coroutine rotationAnimation;
    private Coroutine scaleAnimation;
    private Coroutine deleteOnlifeTime;
    private Coroutine setColliderSize;

    private Vector3 dirVector; // 총알 방향 벡터
    private float dirDegree;   // 총알 방향 각도.

    private OwnerType ownerType;
    private DelGetPosition ownerPos;
    private DelGetPosition ownerDirVec;
    private DelGetDirDegree ownerDirDegree;
    private BuffManager ownerBuff;
    private TransferBulletInfo transferBulletInfo;
    private float addDirVecMagnitude;

    // 코루틴 deltaTime
    private float coroutineDeltaTime = 0.016f;

    private bool active;
    #endregion

    #region getter / setter
    public Transform LaserStartPoint { get { return laserStartPoint; } set { laserStartPoint = value; } }
    public Transform LaserEndPoint { get { return laserEndPoint; } set { laserEndPoint = value; } }

    public LineRenderer GetLineRenderer() { return lineRenderer; }
    public OwnerType GetOwnerType() { return ownerType; }
    public DelGetPosition GetOwnerPos() { return ownerPos; }
    public DelGetPosition GetOwnerDirVec() { return ownerDirVec; }
    public DelGetDirDegree GetOwnerDirDegree() { return ownerDirDegree; }
    public BuffManager GetOwnerBuff() { return ownerBuff; }
    public TransferBulletInfo GetTransferBulletInfo() { return transferBulletInfo; }
    public StatusEffectInfo GetStatusEffectInfo() { return info.statusEffectInfo; }

    // 현재 바라보는 방향의 euler z 각도 반환
    public Vector3 GetPosition() { return objTransform.position; }
    public float GetAddDirVecMagnitude() { return addDirVecMagnitude; }

    public float GetDirDegree() { return dirDegree; }
    // 현재 바라보는 방향의 vector 반환
    public Vector3 GetDirVector() { return dirVector; }

    public void SetOwnerType(OwnerType ownerType)
    {
        this.ownerType = ownerType;
    }

    public float DecreaseDamageAfterPierce { get; private set; }
    #endregion

    #region unityFunction
    void Awake()
    {
        active = false;
        //gameObject.hideFlags = HideFlags.HideInHierarchy;
        objTransform = GetComponent<Transform>();
        boxCollider = GetComponent<BoxCollider2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        objRigidbody = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        animator = GetComponentInChildren<Animator>();
        // 총알 끼리 무시, 총알 레이어 무시, 현재 임시로 Enemy 13, Wall 14, Bullet 15, Player 16번 쓰고 있음.
        // Physics2D.IgnoreLayerCollision(15, 15);
        // edit -> project settings -> physics2D 에서 레이어별 충돌 무시 설정 가능, 거기서 일단 설정했음
        // PlayerBullet, Player 충돌 무시
        // EnemyBullet, Enemy 충돌 무시
    }

    private void FixedUpdate()
    {
        /* 애니메이션 경우 trigger 설정 하고 그 다음 프레임에서 실행
         * 프레임 너무 떨어짐
        if(BulletAnimationType.NotPlaySpriteAnimation!= info.spriteAnimation)
        {
            Debug.Log("T : " + Time.time + ", spriteAniRenderer : " + spriteAniRenderer.sprite);
            boxCollider.size = spriteAniRenderer.sprite.bounds.size;
        }*/
        for (int i = 0; i < info.updatePropertiesLength; i++)
        {
            info.updateProperties[i].Update();
        }
    }


    // 총알 전체 회수할 때 onDisable로 회수 처리 하려함.
    void OnDisable()
    {
        if(true == active)
        {
            CommonDelete();
        }
    }
    #endregion

    #region initialization
    /// <summary>일반(투사체) 총알 초기화 - position이랑 direction만 받음, DeleteAfterSummonBulletProperty 전용 초기화</summary>
    public void Init(BulletInfo bulletInfo, BuffManager ownerBuff, TransferBulletInfo transferBulletInfo, OwnerType ownerType, Vector3 pos, float direction = 0)
    {
        active = true;
        info = bulletInfo;
        this.transferBulletInfo = new TransferBulletInfo(transferBulletInfo);

        UpdateTransferBulletInfo();
        ApplyWeaponBuff();
        // 투사체 총알 속성 초기화
        InitProjectileProperty();

        // Owner 정보 초기화
        InitOwnerInfo(ownerType);

        // 총알 속성들 초기화
        InitPropertyClass();

        // 처음 위치 설정
        objTransform.position = pos;
        objTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
        objTransform.localScale = new Vector3(info.scaleX, info.scaleY, 1f);
        dirDegree = 0f;
        dirVector = Vector3.right;
        SetDirection(direction);
    }

    // 일반(투사체) 총알 초기화
    public void Init(BulletInfo bulletInfo, BuffManager ownerBuff, OwnerType ownerType, Vector3 pos, float direction, TransferBulletInfo transferBulletInfo)
    {
        active = true;
        info = bulletInfo;
        this.transferBulletInfo = new TransferBulletInfo(transferBulletInfo);
        this.ownerBuff = ownerBuff;

        UpdateTransferBulletInfo();

        // Owner 정보 초기화
        InitOwnerInfo(ownerType);

        // Owner buff 적용
        ApplyWeaponBuff();

        // 투사체 총알 속성 초기화
        InitProjectileProperty();

        // 총알 속성들 초기화
        InitPropertyClass();

        // 처음 위치 설정
        objTransform.position = pos;
        objTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
        objTransform.localScale = new Vector3(info.scaleX, info.scaleY, 1f);
        dirDegree = 0f;
        dirVector = Vector3.right;
        SetDirection(direction);
    }

    // 레이저 총알 초기화
    public void Init(BulletInfo bulletInfo, BuffManager ownerBuff, OwnerType ownerType, float addDirVecMagnitude, DelGetPosition ownerPos, DelGetPosition ownerDirVec, DelGetDirDegree ownerDirDegree,TransferBulletInfo transferBulletInfo)
    {
        active = true;
        info = bulletInfo;
        this.transferBulletInfo = new TransferBulletInfo(transferBulletInfo);
        this.ownerBuff = ownerBuff;

        UpdateTransferBulletInfo();

        // component on/off
        boxCollider.enabled = false;
        circleCollider.enabled = false;
        lineRenderer.enabled = true;

        paticleObj.SetActive(false);
        lineRenderer.startColor = Color.blue;
        lineRenderer.endColor = Color.cyan;
        lineRenderer.startWidth = 0.4f;
        lineRenderer.endWidth = 0.4f;
        lineRenderer.SetPosition(0, Vector2.zero);
        lineRenderer.SetPosition(1, Vector2.zero);

        // Owner 정보 초기화
        InitOwnerInfo(ownerType);
        // Owner buff 적용, InitPropertyClass 이전에 실행 해야됨.
        ApplyWeaponBuff();

        spriteAnimatorObj.SetActive(false);
        spriteRenderer.sprite = null;
        laserViewObj.SetActive(true);

        // 0529 레이저 임시, 파란색 레이저
        laserStartPointAnimator.SetTrigger("BlueLaser");
        laserEndPointAnimator.SetTrigger("BlueLaser");
        objTransform.localScale = new Vector3(info.scaleX, info.scaleY, 1f);
        this.ownerPos = ownerPos;
        this.ownerDirVec = ownerDirVec;
        this.ownerDirDegree = ownerDirDegree;
        this.addDirVecMagnitude = addDirVecMagnitude;
        objTransform.position = ownerPos();

        InitPropertyClass();
        //bulletUpdate = StartCoroutine("BulletUpdate");
    }

    /// <summary>FixedOwenrPattern 전용 초기화</summary> 
    public void Init(BulletInfo bulletInfo, BuffManager ownerBuff, OwnerType ownerType, float addDirVecMagnitude, Vector3 pos, DelGetPosition ownerPos, DelGetPosition ownerDirVec, DelGetDirDegree ownerDirDegree, TransferBulletInfo transferBulletInfo)
    {
        active = true;
        info = bulletInfo;
        this.transferBulletInfo = new TransferBulletInfo(transferBulletInfo);
        this.ownerBuff = ownerBuff;

        UpdateTransferBulletInfo();

        // Owner 정보 초기화
        InitOwnerInfo(ownerType);

        // Owner buff 적용
        ApplyWeaponBuff();

        // 투사체 총알 속성 초기화
        InitProjectileProperty();

        // InitPropertyClass() 할 때 정보 넘겨야 되서 미리 선언
        this.ownerPos = ownerPos;
        this.ownerDirVec = ownerDirVec;
        this.ownerDirDegree = ownerDirDegree;
        this.addDirVecMagnitude = addDirVecMagnitude;

        // 총알 속성들 초기화
        InitPropertyClass();

        // 처음 위치 설정
        objTransform.position = pos;
        objTransform.rotation = Quaternion.Euler(0f, 0f, 0f);
        objTransform.localScale = new Vector3(info.scaleX, info.scaleY, 1f);
        dirDegree = 0f;
        dirVector = Vector3.right;        
        SetDirection(ownerDirDegree());
    }

    private void InitOwnerInfo(OwnerType ownerType)
    {
        this.ownerType = ownerType;
        // Enemy 13, Wall 14, Bullet 15, Player 16번
        switch (ownerType)
        {
            case OwnerType.Enemy:
                gameObject.layer = LayerMask.NameToLayer("EnemyBullet");
                break;
            case OwnerType.Player:
                gameObject.layer = LayerMask.NameToLayer("PlayerBullet");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Projectile 총알 collision, update, delete 속성 Class이외의
    /// 기타 bullet 고유 속성들 초기화 및 적용
    /// </summary>
    private void InitProjectileProperty()
    {
        // sprite 애니메이션 적용
        if (BulletAnimationType.NotPlaySpriteAnimation != info.spriteAnimation)
        {
            spriteAnimatorObj.SetActive(true);
            animator.SetTrigger(info.spriteAnimation.ToString());
            spriteRenderer.sprite = null;
            //boxCollider.size = new Vector2(0.1f, 0.1f);
            circleCollider.radius = 0.1f;
            setColliderSize = StartCoroutine("SetColliderSize");
        }
        // sprite 애니메이션 미 적용
        else
        {
            spriteAnimatorObj.SetActive(false);
            spriteRenderer.sprite = info.bulletSprite;
            float sizeX = spriteRenderer.sprite.bounds.size.x;
            float sizeY = spriteRenderer.sprite.bounds.size.y;
            float size = (sizeX > sizeY) ? sizeY : sizeX;
            circleCollider.radius = size * 0.3f;
            if (OwnerType.Enemy == ownerType)
            {
                circleCollider.radius = 0.05f;
            }
            //boxCollider.size = spriteRenderer.sprite.bounds.size;
            //Debug.Log("spriteRenderer : " + spriteRenderer.sprite.bounds.size);
        }

        // rotate 360도 계속 회전하는 애니메이션 적용
        if (true == info.showsRotationAnimation)
        {
            rotationAnimation = StartCoroutine("RotationAnimation");
        }
        // scale이 커지고 작아지는 애니메이션 적용
        if (true == info.showsScaleAnimation)
        {
            scaleAnimation = StartCoroutine("ScaleAnimation");
        }

        // lifeTime이 0 초과되는 값을 가지면 시간이 lifeTime이 지나면 delete 속성 실행
        if (info.lifeTime > 0)
        {
            deleteOnlifeTime = StartCoroutine("DeleteOnLifeTime");
        }

        if (info.soundId >= 0)
        {
            AudioManager.Instance.PlaySound(info.soundId);
        }

        // 파티클이 포함되어있는 오브젝트 on/ off
        paticleObj.SetActive(info.showsParticle);

        laserViewObj.SetActive(false);

        // component on/off
        circleCollider.enabled = true;
        lineRenderer.enabled = false;
        //lineRenderer.positionCount = 0;

        // 튕기는 총알 테스트 용, 일단 컬라이더 임시로 박스만 쓰는 중
        if (true == info.bounceAble)
        {
            circleCollider.isTrigger = false;
        }
        else
        {
            circleCollider.isTrigger = true;
        }

        if (true == info.canBlockBullet)
        {
            if (OwnerType.Player == ownerType)
                objTransform.gameObject.layer = LayerMask.NameToLayer("PlayerCanBlockBullet");
            if (OwnerType.Enemy == ownerType)
                objTransform.gameObject.layer = LayerMask.NameToLayer("EnemyCanBlockBullet");
        }
        if (true == info.canReflectBullet)
        {
            if (OwnerType.Player == ownerType)
                objTransform.gameObject.layer = LayerMask.NameToLayer("PlayerCanReflectBullet");
            if (OwnerType.Enemy == ownerType)
                objTransform.gameObject.layer = LayerMask.NameToLayer("EnmeyCanReflectBullet");
        }
        else
        {
            objTransform.tag = "Bullet";
        }
    }

    /// <summary> collision, update, delete 속성 Class들 초기화  </summary>
    private void InitPropertyClass()
    {
        // 총알 충돌 속성 초기화
        for (int i = 0; i < info.collisionPropertiesLength; i++)
        {
            info.collisionProperties[i].Init(this);
        }
        // 총알 이동 속성 초기화
        for (int i = 0; i < info.updatePropertiesLength; i++)
        {
            info.updateProperties[i].Init(this);
        }
        // 총알 삭제 속성 초기화
        for (int i = 0; i < info.deletePropertiesLength; i++)
        {
            info.deleteProperties[i].Init(this);
        }
    }
    #endregion

    #region function
    /// <summary> 해당 Vector 방향으로 총알을 회전하고 속도를 설정한다. </summary>
    public void SetDirection(Vector3 dirVector)
    {
        this.dirVector = dirVector;
        this.dirDegree = dirVector.GetDegFromVector();
        if (info.isFixedAngle == false)
        {
            objTransform.rotation = Quaternion.Euler(0, 0, this.dirDegree);
        }
        objRigidbody.velocity = info.speed * dirVector;
    }

    /// <summary> 해당 각도로 rotation.z 값을 설정하고 속도를 지정한다. </summary>
    public void SetDirection(float dirDegree)
    {
        this.dirDegree = dirDegree;
        dirVector = MathCalculator.VectorRotate(dirVector, dirDegree);
        if (info.isFixedAngle == false)
        {
            objTransform.rotation = Quaternion.Euler(0, 0, this.dirDegree);
        }
        objRigidbody.velocity = info.speed * dirVector;
    }

    /// <summary> 현재 방향의 각도와 방향벡터에서 매개변수로 받은 각도만큼 회전 및 속도를 지정한다. </summary>
    public void RotateDirection(float dirDegree)
    {
        this.dirDegree += dirDegree;
        dirVector = MathCalculator.VectorRotate(dirVector, dirDegree);
        if (info.isFixedAngle == false)
        {
            objTransform.rotation = Quaternion.Euler(0, 0, this.dirDegree);
        }
        objRigidbody.velocity = info.speed * dirVector;
    }

    // velocity 바꾸는 함수는 계속 구조 개선 및 수정될 예정. 

    /// <summary> 속력만 바뀌는데 속력 0미만 되면 방향 원래 벡터의 반대 방향으로 바꿈 </summary>
    public void SetVelocity(float speed)
    {
        if(speed >= 0)
        {
            info.speed = speed;
            RotateDirection(0);
        }
        else
        {
            info.speed = -speed;
        }
    }

    /// <summary> 충돌 처리 Collision </summary>
    public void OnCollisionEnter2D(Collision2D coll)
    {
        CollisionBullet(coll);
    }

    /// <summary> 충돌 처리 Trigger </summary>
    public void OnTriggerEnter2D(Collider2D coll)
    {
        CollisionBullet(coll);
    }

    // TODO : collisionPropery안에서도 또 layer 체크하는데 봐서 간소화 시킬 수 있으면 간소화 시킬 예정
    /// <summary> 충돌 속성 실행 Collision </summary>
    public void CollisionBullet(Collision2D coll)
    {
        int length = info.collisionPropertiesLength;
        if (OwnerType.Player == ownerType)
        {
            // TransparentFx 1, enemy 13, wall 14, EnemyCanBlockBullet 20, EnemyCanReflectBullet 21
            if (UtilityClass.CheckLayer(coll.gameObject.layer, 1, 13, 14, 20, 21))
            {
                for (int i = 0; i < length; i++)
                {
                    info.collisionProperties[i].Collision(ref coll);
                }
            }
        }
        else if(OwnerType.Enemy == ownerType)
        {
            // TransparentFx 1, wall 14, player 16, PlayerCanBlockBullet 18, PlayerCanReflectBullet 19
            if (UtilityClass.CheckLayer(coll.gameObject.layer, 1, 14, 16, 18, 19))
            {
                for (int i = 0; i < length; i++)
                {
                    info.collisionProperties[i].Collision(ref coll);
                }
            }
        }
    }
    
    /// <summary> 충돌 속성 실행 Trigger </summary>
    public void CollisionBullet(Collider2D coll)
    {
        int length = info.collisionPropertiesLength;
        if (OwnerType.Player == ownerType)
        {
            // TransparentFx 1, enemy 13, wall 14, EnemyCanBlockBullet 20, EnemyCanReflectBullet 21
            if (UtilityClass.CheckLayer(coll.gameObject.layer, 1, 13, 14, 20, 21))
            {
                for (int i = 0; i < length; i++)
                {
                    info.collisionProperties[i].Collision(ref coll);
                }
            }
        }
        else if (OwnerType.Enemy == ownerType)
        {
            // TransparentFx 1, wall 14, player 16, PlayerCanBlockBullet 18, PlayerCanReflectBullet 19
            if (UtilityClass.CheckLayer(coll.gameObject.layer, 1, 14, 16, 18, 19))
            {
                for (int i = 0; i < length; i++)
                {
                    info.collisionProperties[i].Collision(ref coll);
                }
            }
        }
    }

    /// <summary> 삭제 속성 실행 </summary>
    public void DestroyBullet()
    {
        CommonDelete();
        // 삭제 속성 모두 실행
        for (int i = 0; i < info.deletePropertiesLength; i++)
        {
            info.deleteProperties[i].DestroyBullet();
        }
    }

    /// <summary> 충돌로 인한 삭제, 총알 전체 회수로 인한 삭제시 공통적인 삭제 내용 실행</summary>
    private void CommonDelete()
    {
        active = false;

        // 실행 중인 코루틴이 있으면 코루틴 멈춤
        if (null != bulletUpdate)
        {
            StopCoroutine(bulletUpdate);
        }
        if (null != rotationAnimation)
        {
            StopCoroutine(rotationAnimation);
        }
        if (null != scaleAnimation)
        {
            StopCoroutine(scaleAnimation);
        }
        if (null != deleteOnlifeTime)
        {
            StopCoroutine(deleteOnlifeTime);
        }
        if (null != setColliderSize)
        {
            StopCoroutine(setColliderSize);
        }

        viewTransform.localRotation = Quaternion.Euler(0, 0, 0);
        viewTransform.localScale = new Vector3(1f, 1f, 1f);

        // 0813 모, 애니메이터로 인해서 sprite가 null이 아니게 되어서 초기화 하기 위해서 넣음
        if (BulletAnimationType.NotPlaySpriteAnimation != info.spriteAnimation)
        {
            animator.Rebind();
        }
    }

    /// <summary> weapon -> bulletPattern으로 넘어온 정보 최신화, 이후 enemy에 정보 넘김. </summary>
    private void UpdateTransferBulletInfo()
    {
        // pattern에서 넘어온 정보의 값이 존재 할 경우(0이 아닐 경우) bulletInfo 값에 덮어쓰고 0일 경우 기존 bulletInfo값 그대로 씀.
        if (0 != transferBulletInfo.bulletMoveSpeed)
            info.speed = transferBulletInfo.bulletMoveSpeed;
        if (0 != transferBulletInfo.range)
            info.range = transferBulletInfo.range;
        if (0 != transferBulletInfo.damage)
            info.damage = transferBulletInfo.damage;        
        if (0 != transferBulletInfo.criticalChance)
            info.criticalChance = transferBulletInfo.criticalChance;
    }

    /// <summary> 매개변수 weaponType와 동일 타입인지 확인 후 bool 반환</summary>
    /// <param name="weaponType">확인할 weapontype</param>
    public bool CheckEqualWeaponType(WeaponType weaponType)
    {
        if (weaponType == transferBulletInfo.weaponType)
            return true;
        else
            return false;
    }

    /// <summary>bullet waeponEffect 적용</summary>
    public void ApplyWeaponBuff()
    {
        int type = (int)transferBulletInfo.weaponType;
        //Debug.Log(info.memo + ", " + (WeaponType)type + ", " + info.bulletType);
        //Debug.Log(name + " zzz : " + ownerBuff);

        if (BulletType.EXPLOSION == info.bulletType) return;
        
        WeaponTargetEffect totalInfo = ownerBuff.WeaponTargetEffectTotal[0];
        WeaponTargetEffect effectInfo = ownerBuff.WeaponTargetEffectTotal[type];

        // 1. 원거리 무기 관통 횟수 +2 증가
        if (effectInfo.increasePierceCount)
        {
            info.pierceCount += 2;
        }

        // 5. 샷건 총알 비유도 총알 방식에서 발사 n초 후 유도총알로 바뀌기
        if (CheckEqualWeaponType(WeaponType.SHOTGUN) && effectInfo.shotgunBulletCanHoming)
        {
            info.homingStartTime = 0.1f;
            info.homingEndTime = 1.5f;
            info.range += 15f;      // 기존의 샷건이 사정거리가 짧은데 유도 총알로 바뀌면서 사거리 증가 시켜야 될 것 같음. 수치는 봐서 조절

            //HomingProperty 중복 생성 방지
            if (false == HasIncludedUpdateProperty(BulletPropertyType.Update, typeof(HomingProperty)))
            {
                info.updateProperties.Add(new HomingProperty());
                info.updatePropertiesLength += 1;
            }
        }

        
        // 6. 모든 근거리 무기 상대 총알 막기
        if (effectInfo.meleeWeaponsCanBlockBullet)
        {
            info.canBlockBullet = true;
            if (OwnerType.Player == ownerType)
            {
                objTransform.gameObject.layer = LayerMask.NameToLayer("PlayerCanBlockBullet");
            }
            if (OwnerType.Enemy == ownerType)
            {
                objTransform.gameObject.layer = LayerMask.NameToLayer("EnemyCanBlockBullet");
            }
        }

        // 7. 모든 근거리 무기 상대 총알 반사
        if (effectInfo.meleeWeaponsCanReflectBullet)
        {
            info.canReflectBullet = true;
            if (OwnerType.Player == ownerType)
            {
                objTransform.gameObject.layer = LayerMask.NameToLayer("PlayerCanReflectBullet");
            }
            if (OwnerType.Enemy == ownerType)
            {
                objTransform.gameObject.layer = LayerMask.NameToLayer("EnmeyCanReflectBullet");
            }
        }

        // 8. 총알이 벽에 1회 튕겨집니다. 
        if (effectInfo.bounceAble)
        {
            info.bounceAble = true;
            info.bounceCount = 1;
        }

        // 10. 폭탄 무기 마인화, 일정 거리 근접 시 자동 추적 후 폭발
        if (effectInfo.becomesSpiderMine && BulletType.MINE == info.bulletType)
        {
            info.becomeSpiderMine = true;
            info.lifeTime += 3f;
            //MineBombProperty 중복 생성 방지
            if (false == HasIncludedUpdateProperty(BulletPropertyType.Update, typeof(MineBombProperty)))
            {
                info.updateProperties.Add(new MineBombProperty());
                info.updatePropertiesLength += 1;
            }
        }

        // bulletInfo 수치들 공식 최종 적용
        
        // 0.모든 무기 공격력 n % 증가, n 미정
        info.damage = info.damage * (totalInfo.damageIncrement + effectInfo.damageIncrement + transferBulletInfo.chargedDamageIncrement);
        
        // 1.모든 무기 치명타 확률 n% 증가, 합 옵션
        info.criticalChance = info.criticalChance + totalInfo.criticalChanceIncrement + effectInfo.damageIncrement;

        info.speed = info.speed * (totalInfo.bulletSpeedIncrement + effectInfo.bulletSpeedIncrement);    // 총알 이동속도 변화

        if (OwnerType.Player == ownerType)
        {
            DecreaseDamageAfterPierce = 0.3f * (totalInfo.decreaseDamageAfterPierceReduction * effectInfo.decreaseDamageAfterPierceReduction);
        }
    }

    /// <summary> bullet Properties 안에 해당 property가 포함 되어 있는지 여부 확인</summary>
    public bool HasIncludedUpdateProperty(BulletPropertyType bulletPropertyType, System.Type type)
    {
        switch(bulletPropertyType)
        {
            case BulletPropertyType.Collision:
                for (int i = 0; i < info.collisionPropertiesLength; i++)
                {
                    if (type == info.collisionProperties[i].GetType())
                    {
                        return true;
                    }
                }
                break;
            case BulletPropertyType.Update:
                for (int i = 0; i < info.updatePropertiesLength; i++)
                {
                    if (type == info.updateProperties[i].GetType())
                    {
                        return true;
                    }
                }
                break;
            case BulletPropertyType.Delete:
                for (int i = 0; i < info.deletePropertiesLength; i++)
                {
                    if (type == info.deleteProperties[i].GetType())
                    {
                        return true;
                    }
                }
                break;                
            default:
                break;
        }
        return false;
    }
    #endregion

    #region coroutine

    // 안쓸 듯
    // 총알 Update 코루틴
    private IEnumerator BulletUpdate()
    {
        while (true)
        {
            // 총알 update 속성 실행
            for (int i = 0; i < info.updatePropertiesLength; i++)
            {
                info.updateProperties[i].Update();
            }
            yield return YieldInstructionCache.WaitForSeconds(0.016f);  // 일단은 약 60 fps 정도로 실행
        }
    }


    // FIXME: init 쪽에서 바로 sprite.bounds 체크하려니 안됨(애니메이션 트리거 작동 특성상 setTrigger 이후 한 박자 뒤에 바뀌나봄)일정 텀 주고
    // sprite.bounds 측정해야되서 일단 코루틴으로 0.01초 미뤄서 체크, 애니메이션 없는 sprite는 처음에 init 때 sprite 크기 측정해도 되는데 애니메이션 있는 sprite는 연구좀 해야됨. 
    // fixedUpdate에서 매번 돌리면 프레임 많이 떨어짐.(애니메이션 있는 sprite는 sprite 크기가 매번 달라서 이런 식으로 해야 될 수도 있긴 함.)

    // 땜빵용 코드
    private IEnumerator SetColliderSize()
    {
        while(true)
        {
            if (null != spriteAniRenderer.sprite)
                break;
            //Debug.Log(spriteAniRenderer.sprite);
            yield return YieldInstructionCache.WaitForSeconds(Time.fixedDeltaTime);
        }
        //Debug.Log("size : " + spriteAniRenderer.sprite.bounds.size);
        float sizeX = spriteAniRenderer.sprite.bounds.size.x;
        float sizeY = spriteAniRenderer.sprite.bounds.size.y;
        float size = (sizeX > sizeY) ? sizeY : sizeX;
        circleCollider.radius = size;
        // boxCollider.size = spriteRenderer.sprite.bounds.size;
        //Debug.Log("t : " + Time.time + ", " + spriteAniRenderer.sprite.bounds.size +", colider Size : " + boxCollider.size);
    }


    /// <summary>rotation Z 360도 회전하는 코루틴</summary>
    private IEnumerator RotationAnimation()
    {
        float eulerAngleZ = 0f;
        while (true)
        {
            eulerAngleZ += 12f;
            viewTransform.localRotation = Quaternion.Euler(0f, 0f, eulerAngleZ);
            yield return YieldInstructionCache.WaitForSeconds(coroutineDeltaTime);  // 일단은 약 60 fps 정도로 실행
        }
    }

    /// <summary> scale 1.0 ~ 2.0 커졌다 작아졌다 하는 코루틴 </summary>
    private IEnumerator ScaleAnimation()
    {
        float deltaScale = 0;
        float scale = 0;
        float sign = 1;
        while (true)
        {
            deltaScale += 0.05f * sign;
            scale = 1 + deltaScale;
            viewTransform.localScale = new Vector3(scale, scale, 1f);
            if (scale <= 1.0f)
            {
                sign = 1;
            }
            else if (scale >= 2.0f)
            {
                sign = -1;
            }
            yield return YieldInstructionCache.WaitForSeconds(coroutineDeltaTime);  // 일단은 약 60 fps 정도로 실행
        }
    }


    // invoke로 하면 메모리풀에서 on / off시 남아있어서 오류가 생겨서 코루틴으로 바꿈

    /// <summary> 시간이 lifeTime 되면 삭제 </summary>
    private IEnumerator DeleteOnLifeTime()
    {
        float time = 0;
        while(true)
        {
            if(time >= info.lifeTime)
            {
                DestroyBullet();
            }
            time += coroutineDeltaTime;
            yield return YieldInstructionCache.WaitForSeconds(coroutineDeltaTime);
        }
    }
    #endregion

}

/*
// 기본 총알
public class BaseBullet : Bullet
{

}*/
