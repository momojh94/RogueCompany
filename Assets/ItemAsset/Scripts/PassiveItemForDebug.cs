﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WeaponAsset;

public class PassiveItemForDebug : MonoBehaviour
{
    #region variables
    // 디버깅 관련
    [SerializeField]
    private GameObject passiveDebugObj;
    [SerializeField]
    private GameObject effectTotalViewObj;
    [SerializeField]
    private Image passiveSelectImage;
    [SerializeField]
    private GameObject passiveSlotPrefab;
    [SerializeField]
    private Text EffectTotalNameText;
    [SerializeField]
    private Text EffectTotalValueText;
    [SerializeField]
    private Text SelectPassiveIdText;
    [SerializeField]
    private Text SelectPassiveMemoText;

    [SerializeField]
    private Text viewTypeText;

    // effect Total text 변수 명, 효과
    private string variableNames;
    private List<string> variableValues;

    private int infoCurrentIndex;
    private int totalInfoIndexMax;

    private int currentIndex;
    private int passiveItemIndexMax;

    // 패시브 아이템 창 관련
    [SerializeField]
    private Transform standardPos;
    [SerializeField]
    private Transform passiveSlotsParent;
    [SerializeField]
    private int slotRow;
    [SerializeField]
    private int slotColumn;
    private int slotCountMax;
    [SerializeField]
    private Vector2 intervalPos;
    private PassiveSlot[] passiveSlots;

    private List<int> passiveSlotIds;
    private int passiveSlotIdsLength;
    #endregion

    #region UnityFunc
    // Use this for initialization
    void Start ()
    {
        currentIndex = 0;
        infoCurrentIndex = 0;
        totalInfoIndexMax = (int)WeaponType.END;

        passiveItemIndexMax = DataStore.Instance.GetMiscItemInfosLength();
        slotCountMax = slotRow * slotColumn;
        
        CreatePassiveSlots(standardPos.position);
        UpdatePassiveSelectImage();
        UpdateEffectTotalNameText();
        UpdateEffectTotalValueText();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            if(passiveDebugObj.activeSelf)
            {
                Debug.Log("패시브 아이템 테스트창 off");
            }
            else
            {
                Debug.Log("패시브 아이템 테스트창 on");
            }
            passiveDebugObj.SetActive(!passiveDebugObj.activeSelf);
            effectTotalViewObj.SetActive(passiveDebugObj.activeSelf);
        }
    }
    #endregion

    // 임시로 만든 함수
    public void OnOffPassiveItems()
    {
        passiveDebugObj.SetActive(!passiveDebugObj.activeSelf);
        effectTotalViewObj.SetActive(passiveDebugObj.activeSelf);
    }


    #region PassiveItemSlot

    private void CreatePassiveSlots(Vector3 standardPos)
    {
        passiveSlotIds = new List<int>();
        passiveSlotIdsLength = 0;
        passiveSlots = new PassiveSlot[slotCountMax];
        GameObject createdObj;
        Vector3 currentPos = new Vector3();
        for (int y = 0; y < slotRow; y++)
        {
            for(int x = 0; x < slotColumn; x++)
            {
                currentPos.x = standardPos.x + x * intervalPos.x;
                currentPos.y = standardPos.y - y * intervalPos.y;
                createdObj = Instantiate(passiveSlotPrefab);
                createdObj.name = "패시브 슬룻 " + (y * slotRow + x);
                createdObj.transform.position = currentPos;
                createdObj.transform.SetParent(passiveSlotsParent);
                passiveSlots[y * slotColumn + x] = createdObj.GetComponent<PassiveSlot>();
            }
        }
    }

    public void ApplyPassiveForDebug()
    {
        if (passiveSlotIdsLength >= slotCountMax)
        {
            Debug.Log("패시브 슬룻 꽉참. 아이템 적용 안됨.");
            return;
        }
        Debug.Log(currentIndex + "번 패시브 아이템 사용 for debug");
        passiveSlotIds.Add(currentIndex);
        passiveSlotIdsLength += 1;
        UsableItemInfo passive = DataStore.Instance.GetMiscItemInfo(currentIndex);
        for (int i = 0; i < passive.EffectApplyTypes.Length; i++)
        {
            passive.EffectApplyTypes[i].UseItem();
        }
        UpdatePassiveSlots();
        UpdateEffectTotalValueText();
    }

    public void UpdatePassiveSelectImage()
    {
        SelectPassiveIdText.text = "Id : " + currentIndex;
        SelectPassiveMemoText.text = DataStore.Instance.GetMiscItemInfo(currentIndex).Notes;
        passiveSelectImage.sprite = DataStore.Instance.GetMiscItemInfo(currentIndex).Sprite;
    }

    public void SelectPassiveUp()
    {
        currentIndex = (currentIndex - 1 + passiveItemIndexMax) % passiveItemIndexMax;
        UpdatePassiveSelectImage();
    }

    public void SelectPassiveDown()
    {
        currentIndex = (currentIndex + 1 + passiveItemIndexMax) % passiveItemIndexMax;
        UpdatePassiveSelectImage();
    }

    public void UpdatePassiveSlots()
    {
        Debug.Log("UpdatePassiveSlots : " + currentIndex + ", " + passiveSlotIdsLength);
        for(int i = 0; i < passiveSlotIdsLength; i++)
        {
            //Debug.Log("a : " + i + ", " + passiveSlotIds[i]);
            passiveSlots[i].UpdatePassiveSlot(DataStore.Instance.GetMiscItemInfo(passiveSlotIds[i]).Sprite);
        }
        for (int i = passiveSlotIdsLength; i < slotCountMax; i++)
        {
            //Debug.Log("b : " + i);
            //passiveSlots[i].UpdatePassiveSlot(null);
        }
    }

    #endregion

    #region viewEffectInfo

    private void UpdateEffectTotalNameText()
    {
        variableNames = 
            "1.moveSpeedIncrement\n" +
            "2.rewardOfEndGameIncrement\n" +
            "3.discountRateOfVendingMachineItems\n" +
            "4.discountRateOfCafeteriaItems\n" +
            "5.discountRateAllItems\n" +
            "6.canDrainHp\n" +
            "7.increaseStaminaWhenkillingEnemies\n" +
            "-----\n" +
            "Weapon\n" +
            "1.bulletCountIncrement\n" +
            "2.criticalChanceIncrement\n" +

            "1.damageIncrement\n" +
            "2.knockBackIncrement\n" +
            "3.chargingSpeedIncrement\n" +
            "4.chargingDamageIncrement\n" +
            "5.gettingSkillGaugeIncrement\n" +
            "6.gettingStaminaIncrement\n" +
            "7.skillPowerIncrement\n" +
            "8.bulletScaleIncrement\n" +
            "9.bulletRangeIncrement\n" +
            "10.bulletSpeedIncrement\n" +
            
            "1.decreaseDamageAfterPierceReduction\n" +
            "2.cooldownReduction\n" +
            "3.accuracyIncrement\n" +

            "1.increasePierceCount\n" +
            "2.becomesSpiderMine\n" +
            "3.bounceAble\n" +
            "4.shotgunBulletCanHoming\n" +
            "5.meleeWeaponsCanBlockBullet\n" +
            "6.meleeWeaponsCanReflectBullet\n" +
            "\n" +
            "\n";
        EffectTotalNameText.text = variableNames;
    }

    public void ChangeViewEffectTotal(bool nextType)
    {
        if(nextType)
            infoCurrentIndex = (infoCurrentIndex + 1) % totalInfoIndexMax;
        else
            infoCurrentIndex = (infoCurrentIndex - 1 + totalInfoIndexMax) % totalInfoIndexMax;
        UpdateEffectTotalValueText();
    }

    public void UpdateEffectTotalValueText()
    {
        viewTypeText.text = ((WeaponType)infoCurrentIndex).ToString();
        CharacterTargetEffect characterTotal = PlayerBuffManager.Instance.BuffManager.CharacterTargetEffectTotal;
        WeaponTargetEffect weaponTotal = PlayerBuffManager.Instance.BuffManager.WeaponTargetEffectTotal[infoCurrentIndex];
        string variableValues = 
            characterTotal.moveSpeedIncrement + "\n" +
            characterTotal.rewardOfEndGameIncrement + "\n" +
            characterTotal.discountRateOfVendingMachineItems + "\n" +
            characterTotal.discountRateOfCafeteriaItems + "\n" +
            characterTotal.discountRateAllItems + "\n" +
            characterTotal.canDrainHp + "\n" +
            characterTotal.increaseStaminaWhenkillingEnemies + "\n" +
            "---\n" +
            "Weapon\n" +
            weaponTotal.bulletCountIncrement + "\n" +
            weaponTotal.criticalChanceIncrement + "\n" +

            weaponTotal.damageIncrement + "\n" +
            weaponTotal.knockBackIncrement + "\n" +
            weaponTotal.chargingSpeedIncrement + "\n" +
            weaponTotal.chargingDamageIncrement + "\n" +
            weaponTotal.gettingSkillGaugeIncrement + "\n" +
            weaponTotal.gettingStaminaIncrement + "\n" +
            weaponTotal.skillPowerIncrement + "\n" +
            weaponTotal.bulletScaleIncrement + "\n" +
            weaponTotal.bulletRangeIncrement + "\n" +
            weaponTotal.bulletSpeedIncrement + "\n" +

            weaponTotal.decreaseDamageAfterPierceReduction + "\n" +
            weaponTotal.cooldownReduction + "\n" +
            weaponTotal.accuracyIncrement + "\n" +

            weaponTotal.increasePierceCount + "\n" +
            weaponTotal.becomesSpiderMine + "\n" +
            weaponTotal.bounceAble + "\n" +
            weaponTotal.shotgunBulletCanHoming + "\n" +
            weaponTotal.meleeWeaponsCanBlockBullet + "\n" +
            weaponTotal.meleeWeaponsCanReflectBullet;

        EffectTotalValueText.text = variableValues;
    }
    #endregion
}
