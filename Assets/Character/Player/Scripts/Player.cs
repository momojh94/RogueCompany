﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponAsset;

/*
#region variables
#endregion
#region getter
#endregion
#region setter
#endregion
#region UnityFunction
#endregion
#region Function
#endregion
*/

/// <summary>
/// Player Class
/// </summary>
public class Player : Character
{
    #region variables
    public enum PlayerType { SOCCER, MUSIC, FISH, ARMY }
    //public Joystick joystick;

    [SerializeField]
    private PlayerController controller;    // 플레이어 컨트롤 관련 클래스
    
    private Transform objTransform;
    /// <summary> player 크기 </summary>
    private float playerScale;

    private RaycastHit2D hit;
    private List<RaycasthitEnemy> raycastHitEnemies;
    private RaycasthitEnemy raycasthitEnemyInfo;
    private int layerMask;  // autoAim을 위한 layerMask
    private int killedEnemyCount;

    [SerializeField] private PlayerHPUI playerHpUi;
    [SerializeField] private WeaponSwitchButton weaponSwitchButton;
    private PlayerData playerData;
    private PlayerData originPlayerData;    // 아이템 효과 적용시 기준이 되는 정보

    // 0717 임시 스킬 게이지
    private int skillGauge;
    private ActiveSkill activeSkill;
    #endregion

    #region property
    public PlayerData PlayerData
    {
        get
        {
            return playerData;
        }
        set
        {
            playerData = value;
        }
    }
    public int KilledEnemyCount
    {
        get
        {
            return killedEnemyCount;
        }
    }
    #endregion

    #region getter
    public PlayerController PlayerController { get { return controller; } }
    public Vector3 GetInputVector () { return controller.GetInputVector(); }

    public WeaponSwitchButton GetWeaponSwitchButton() { return weaponSwitchButton; }
    #endregion


    #region UnityFunc
    void Awake()
    {
        //pState = PlayerState.IDLE;
        objTransform = GetComponent<Transform>();
        playerScale = 1f;
        scaleVector = new Vector3(1f, 1f, 1f);
        isRightDirection = true;
        raycastHitEnemies = new List<RaycasthitEnemy>();
        raycasthitEnemyInfo = new RaycasthitEnemy();
        layerMask = 1 << LayerMask.NameToLayer("Wall");
        Physics2D.IgnoreLayerCollision(16, 13); // enemy 본체랑 충돌 무시

        // 임시로 배경음악 시작
        // AudioManager.Instance.PlayMusic(0);
    }

    //for debug
    bool canAutoAim = false;
    bool updateAutoAim = true;

    // bool e = false;
    // Update is called once per frame
    void Update()
    {
        /*
        if(false == e && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("무기 장착");
            e = true;
            // weaponManager 초기화, 바라보는 방향 각도, 방향 벡터함수 넘기기 위해서 해줘야됨
            weaponManager.Init(this, OwnerType.Player);
        }
        */

        if (Input.GetKeyDown(KeyCode.B))
        {
            updateAutoAim = !updateAutoAim;
            Debug.Log("updateAutoAim : " + updateAutoAim);
        }
        if (updateAutoAim)
        {
            SetAim();
        }
        else
        {
            directionVector = controller.GetRecenteNormalInputVector();
            directionDegree = directionVector.GetDegFromVector();
        }

        // 총구 방향(각도)에 따른 player 우측 혹은 좌측 바라 볼 때 반전되어야 할 object(sprite는 여기서, weaponManager는 스스로 함) scale 조정
        if (-90 <= directionDegree && directionDegree < 90)
        {
            isRightDirection = true;
            scaleVector.x = 1f;
            spriteTransform.localScale = scaleVector;
        }
        else
        {
            isRightDirection = false;
            scaleVector.x = -1f;
            spriteTransform.localScale = scaleVector;
        }
        spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100);
    }

    void FixedUpdate()
    {
        Move();
    }
    #endregion

    #region function
    public override void Init()
    {
        base.Init();
        pState = CharacterInfo.State.ALIVE;

        skillGauge = 100;

        // Player class 정보가 필요한 UI class에게 Player class 넘기거나, Player에게 필요한 UI 찾기
        GameObject.Find("AttackButton").GetComponent<AttackButton>().SetPlayer(this);
        GameObject.Find("ActiveSkillButton").GetComponent<ActiveSkillButton>().SetPlayer(this);
        weaponSwitchButton = GameObject.Find("WeaponSwitchButton").GetComponent<WeaponSwitchButton>();
        weaponSwitchButton.SetPlayer(this);
        controller = new PlayerController(GameObject.Find("VirtualJoystick").GetComponent<Joystick>());
        playerHpUi = GameObject.Find("HPGroup").GetComponent<PlayerHPUI>();
        buffManager = PlayerBuffManager.Instance.BuffManager;
        buffManager.SetOwner(this);
        activeSkill = GetComponentInChildren<ActiveSkill>();
        activeSkill.Init(this);

        // weaponManager 초기화, 바라보는 방향 각도, 방향 벡터함수 넘기기 위해서 해줘야됨
        weaponManager.Init(this, CharacterInfo.OwnerType.Player);

    }

    public void InitPlayerData(PlayerData playerData)
    {
        Debug.Log("InitPlayerData hp : " + playerData.Hp);
        this.playerData = playerData;
        originPlayerData = playerData;
        UpdatePlayerData();
        playerHpUi.UpdateHPUI(playerData.Hp);
    }

    protected override void Die()
    {
        GameStateManager.Instance.GameOver();
        UIManager.Instance.GameOverUI();
    }

    public override float Attacked(TransferBulletInfo transferredBulletInfo)
    {
        playerData.Hp -= transferredBulletInfo.damage;
        playerHpUi.UpdateHPUI(playerData.Hp);
        if (playerData.Hp <= 0) Die();
        return transferredBulletInfo.damage;
    }

    public override float Attacked(Vector2 _dir, Vector2 bulletPos, float damage, float knockBack, float criticalChance = 0, bool positionBasedKnockBack = false)
    {
        if (CharacterInfo.State.ALIVE != pState)
            return 0;
        float criticalCheck = Random.Range(0f, 1f);
        // 크리티컬 공격
        playerData.Hp -= damage;

        if (knockBack > 0)
            isKnockBack = true;

        // 넉백 총알 방향 : 총알 이동 방향 or 몬스터-총알 방향 벡터
        rgbody.velocity = Vector3.zero;

        // bullet과 충돌 Object 위치 차이 기반의 넉백  
        if (positionBasedKnockBack)
        {
            rgbody.AddForce(knockBack * ((Vector2)transform.position - bulletPos).normalized);
        }
        // bullet 방향 기반의 넉백
        else
        {
            rgbody.AddForce(knockBack * _dir);
        }
        playerHpUi.UpdateHPUI(playerData.Hp);

        StopCoroutine(KnockBackCheck());
        StartCoroutine(KnockBackCheck());

        if (playerData.Hp <= 0) Die();

        return damage;
    }

    public void ActiveSkill()
    {
        if(100 == skillGauge)
        {
            Debug.Log("Player 스킬 활성화");
            //skillGauge = 0;
        }
    }
    public CustomObject Interact()
    {
        float bestDistance = interactiveCollider2D.radius * 10;
        Collider2D bestCollider = null;

        Collider2D[] collider2D = Physics2D.OverlapCircleAll(transform.position, interactiveCollider2D.radius);

        for (int i = 0; i < collider2D.Length; i++)
        {
            if (null == collider2D[i].GetComponent<CustomObject>())
                continue;
            if (!collider2D[i].GetComponent<CustomObject>().GetAvailable())
                continue;
            float distance = Vector2.Distance(transform.position, collider2D[i].transform.position);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestCollider = collider2D[i];
            }
        }

        if (null == bestCollider)
            return null;

        return bestCollider.GetComponent<CustomObject>();
    }
    /// <summary>
    /// 캐릭터 이동, 디버그 용으로 WASD Key로 이동 가능  
    /// </summary>
    private void Move()
    {
        // 조이스틱 방향으로 이동하되 입력 거리에 따른 이동속도 차이가 생김.
        objTransform.Translate(controller.GetInputVector() * playerData.MoveSpeed * Time.deltaTime);

        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector2.up * playerData.MoveSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(Vector2.down * playerData.MoveSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector2.right * playerData.MoveSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(Vector2.left * playerData.MoveSpeed * Time.deltaTime);
        }
    }
    /// <summary> 공격 가능 여부 리턴 </summary>
    public bool AttackAble()
    {
        if (pState == CharacterInfo.State.ALIVE)
            return true;
        else return false;
    }

    public void CountKilledEnemy()
    {
        if (false == buffManager.CharacterTargetEffectTotal.canDrainHp) return;
        killedEnemyCount += 1;
        if(killedEnemyCount == 5)
        {
            RecoverHp(0.5f);
            killedEnemyCount = 0;
        }
    }

    public bool RecoverHp(float recoveryHp)
    {
        if (playerData.Hp + recoveryHp <= playerData.HpMax)
        {
            playerData.Hp += recoveryHp;
            return true;
        }
        else
            return false;
    }

    // player 에임 조정, 몬스터 자동 조준 or 조이스틱 방향 
    public void SetAim()
    {       
        int enemyTotal = EnemyManager.Instance.GetAliveEnemyTotal();
        
        if (0 == enemyTotal)
        {
            directionVector = controller.GetRecenteNormalInputVector();
            directionDegree = directionVector.GetDegFromVector();
            //Debug.Log("enemyTotal = 0, 오토 에임 풀림");
            return;
        }
        else
        {
            List<Enemy> enemyList = EnemyManager.Instance.GetEnemyList;
            
            raycastHitEnemies.Clear();
            int raycasthitEnemyNum = 0;
            float minDistance = 10000f;
            int proximateEnemyIndex = -1;

            // Debug.Log("Total : " + enemyTotal);

            // raycast로 player와 enemy 사이에 장애물이 없는 enmey 방향만 찾아낸다.
            for (int i = 0; i < enemyTotal; i++)
            {
                raycasthitEnemyInfo.index = i;
                raycasthitEnemyInfo.distance = Vector2.Distance(enemyList[i].transform.position, objTransform.position);
                // Debug.Log(raycasthitEnemyInfo.distance);
                hit = Physics2D.Raycast(objTransform.position, enemyList[i].transform.position - objTransform.position, raycasthitEnemyInfo.distance, layerMask);
                if(hit.collider == null)
                {
                    raycastHitEnemies.Add(raycasthitEnemyInfo);
                    raycasthitEnemyNum += 1;
                }
            }

            if (raycasthitEnemyNum == 0)
            {
                directionVector = controller.GetRecenteNormalInputVector();
                directionDegree = directionVector.GetDegFromVector();
                //Debug.Log("raycasthitEnemyNum = 0, player와 enemy사이에 장애물 존재, 오토 에임 풀림");
                return;
            }


           // Debug.Log("raycasthitEnemyNum : " + raycasthitEnemyNum + " 조준 가능한 enemy 존재");
            // 위에서 찾은 enmey들 중 distance가 가장 작은 값인 enemy쪽 방향으로 조준한다.
            for (int j = 0; j < raycasthitEnemyNum; j++)
            {
                if (raycastHitEnemies[j].distance <= minDistance)
                {
                    minDistance = raycastHitEnemies[j].distance;
                    proximateEnemyIndex = j;
                }
            } 

            directionVector = (enemyList[raycastHitEnemies[proximateEnemyIndex].index].transform.position - objTransform.position);
            directionVector.z = 0;
            directionVector.Normalize();
            //Debug.Log(directionVector.magnitude);
            directionDegree = directionVector.GetDegFromVector();
        }
    }


    // item Player 대상 효과 적용
    public override void ApplyItemEffect(CharacterTargetEffect itemUseEffect)
    {
        // 주로 즉시 효과 볼 내용들이 적용되서 체력, 허기 회복 두개만 쓸 것 같음.

        Debug.Log("소모품 아이템 플레이어 대상 효과 적용");
        if (itemUseEffect.recoveryHp != 0)
        {
            playerData.Hp += itemUseEffect.recoveryHp;
        }
        if (itemUseEffect.recoveryHunger != 0)
        {
            playerData.Hunger += itemUseEffect.recoveryHunger;
        }
        /*
        이런건 버프, 패시브 효과 쪽이 어울림
        moveSpeedIncrease, hungerMaxIncrease, armorIncrease, criticalChanceIncrease
        */
        playerData.HungerMax = originPlayerData.HungerMax * itemUseEffect.hungerMaxIncrease;
        playerData.MoveSpeed = originPlayerData.MoveSpeed * itemUseEffect.moveSpeedIncrease;
        playerData.Armor = originPlayerData.Armor * itemUseEffect.armorIncrease;
        playerData.CriticalChance = originPlayerData.CriticalChance * itemUseEffect.criticalChanceIncrease;
    }

    public override void ApplyStatusEffect(StatusEffectInfo statusEffectInfo)
    {
    }

    // 안 쓸거 같음.
    public void UpdatePlayerData()
    {
        // playerData. = originPlayerData. * buffManager.PlayerTargetEffectTotal.
        playerData.HungerMax = originPlayerData.HungerMax * buffManager.CharacterTargetEffectTotal.hungerMaxIncrease;
        playerData.MoveSpeed = originPlayerData.MoveSpeed * buffManager.CharacterTargetEffectTotal.moveSpeedIncrease;
        playerData.Armor = originPlayerData.Armor * buffManager.CharacterTargetEffectTotal.armorIncrease;
        playerData.CriticalChance = originPlayerData.CriticalChance * buffManager.CharacterTargetEffectTotal.criticalChanceIncrease;
    }
    #endregion
}

/// <summary> Player 조작 관련 Class </summary>
[System.Serializable]
public class PlayerController
{
    [SerializeField]
    private Joystick joystick; // 조이스틱 스크립트

    // 조이스틱 방향
    private Vector3 inputVector;

    
    public PlayerController(Joystick joystick)
    {
        this.joystick = joystick;
    }

    /// <summary>
    /// 조이스틱이 현재 바라보는 방향의 벡터  
    /// </summary> 
    public Vector3 GetInputVector()
    {    
        float h = joystick.GetHorizontalValue();
        float v = joystick.GetVerticalValue();

        // 조이스틱 일정 거리 이상 움직였을 때 부터 조작 적용. => 적용 미적용 미정
        //if (h * h + v * v > 0.01)
        //{
        inputVector = new Vector3(h, v, 0).normalized;
        //}
        //else inputVector = Vector3.zero;

        return inputVector;
    }

    /// <summary>
    /// 입력한 조이스틱의 가장 최근 Input vector의 normal vector 반환 
    /// </summary>
    public Vector3 GetRecenteNormalInputVector()
    {
        //Debug.Log(joystick.GetRecenteNormalInputVector().magnitude);
        return joystick.GetRecenteNormalInputVector();
    }
    
}