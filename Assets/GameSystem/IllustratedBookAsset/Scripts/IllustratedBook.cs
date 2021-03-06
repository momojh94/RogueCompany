﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaponAsset;
using UnityEngine.UI;

// https://www.google.com/search?q=%EB%A1%9C%EB%93%9C%EC%98%A4%EB%B8%8C%EB%8D%98%EC%A0%84&source=lnms&tbm=isch&sa=X&ved=0ahUKEwiGu_ea5PbfAhVMZt4KHSxzBPIQ_AUIDigB&biw=767&bih=744&dpr=1.25#imgrc=-i46cM91R3lJUM:
// 선택 할 수 있는 공간 55~65%, 보여주는 쪽 30~35% 정도 크기

// 무기랑 아이템은 고정, 로드오브던전 항목별로 도감 하나씩 있음.
// 무기 도감, 아이템 도감 따로 하면 무기 도감에서 탭을 무기 타입별로 둬도 되긴 하는데
// 무기 타입이 많아서 가로 길이 넘 길어짐.

// 초기의 ? 모양으로 해주고 던전에서 첫 등장시 혹은 처음 사용시 도감에서 ? 해제하여
// 상세내용 보여주기

// info view 쪽
// 이름, 탄약량, 레이팅, 무기 타입, 쿨타임?, 공격력(dps로 보여주는게 나을듯)
// + 부가 설명(없어도 되고 있어도 될 것 같음)

// 도감, 콜렉팅 기능 분리 혹은 도감내에 포함. 아마 분리 시킬듯?
/*
 * TODO : 초기화, 분류(weapon, item)별 다르게 보여주기, 해당 콘텐츠 내용 view, 
 * 해당 분류에서 구분해서 보여주기(S~E rating 별로 하던가, 등급 오름 내린차순 둘중 하나)
 */

public class IllustratedBook : MonoBehaviour
{
    public enum IllustratedBookType { WEAPON, ITEM, MONSTER, BOSS_MONSTER }
    public enum BookSortingType { ALL_RATING, S, A, B, C, D, E }   // 구분 어떻게 보여줄지
    private delegate void SetActiveContents(bool show);

    #region variables
    [SerializeField]
    private GameObject bookUI;
    [SerializeField]
    private Transform contentsParentObj;
    [SerializeField]
    private GameObject contentsPrefab;
    [SerializeField]
    private Sprite questionMarkSprite;

    private IllustratedBookType illustratedBookType;
    private BookSortingType bookSortingType;
    private IllustratedBookContents[] weaponContentsList;
    private List<int>[] weaponIndexbyRating;
    private IllustratedBookContents[] itemContentsList;

    [SerializeField]
    private GameObject sortDropdownObj;
    [SerializeField]
    private Dropdown sortDropdown;

    private SetActiveContents[] setActiveWeaponContents;
    private SetActiveContents[] setActiveItemContents;

    [Header("category tab 변수들")]
    [SerializeField]
    private Image[] tabImages;
    [SerializeField]
    private Text[] tabTexts;

    private int tabLength;

    [SerializeField]
    private Sprite selectedImage;
    [SerializeField]
    private Sprite unselectedImage;

    private int ratingLength;
    #endregion

    public Sprite GetQuestionMarkSprite()
    {
        return questionMarkSprite;
    }

    #region unityfunc
    void Awake()
    {
        InitBook();
    }
    #endregion

    #region func
    private void InitBook()
    {
        illustratedBookType = IllustratedBookType.WEAPON;
        bookSortingType = BookSortingType.ALL_RATING;
        weaponContentsList = new IllustratedBookContents[WeaponsData.Instance.GetWeaponInfosLength()];
        itemContentsList = new IllustratedBookContents[ItemsData.Instance.GetMiscItemInfosLength()];
        ratingLength = (int)Rating.E;

        setActiveWeaponContents = new SetActiveContents[ratingLength];
        setActiveItemContents = new SetActiveContents[ratingLength];

        weaponIndexbyRating = new List<int>[ratingLength];
        
        GameObject createdobj;
        WeaponInfo weaponInfo;
        // weapon contents 생성
        for (int i = WeaponsData.Instance.GetWeaponInfosLength()-1; i >= 0; i--)
        {
            createdobj = Instantiate(contentsPrefab);
            createdobj.name = "weaponContents_" + i;
            createdobj.transform.SetParent(contentsParentObj);
            weaponContentsList[i] = createdobj.GetComponent<IllustratedBookContents>();
            weaponInfo = WeaponsData.Instance.GetWeaponInfo(i, CharacterInfo.OwnerType.PLAYER);
            weaponContentsList[i].Init(weaponInfo);
            setActiveWeaponContents[(int)weaponInfo.rating-1] += weaponContentsList[i].SetActiveContents;
            createdobj.transform.localScale = new Vector3(1, 1, 1);
        }

        UsableItemInfo usableItemInfo;
        // item contents 생성
        for (int i = ItemsData.Instance.GetMiscItemInfosLength() - 1; i >= 0; i--)
        {
            createdobj = Instantiate(contentsPrefab);
            createdobj.name = "itemContents_" + i;
            createdobj.transform.SetParent(contentsParentObj);
            itemContentsList[i] = createdobj.GetComponent<IllustratedBookContents>();
            usableItemInfo = ItemsData.Instance.GetMiscItemInfo(i);
            itemContentsList[i].Init(usableItemInfo);
            setActiveItemContents[(int)usableItemInfo.Rating - 1] += itemContentsList[i].SetActiveContents;
            createdobj.transform.localScale = new Vector3(1, 1, 1);
        }
        bookUI.SetActive(false);

        // category tab 초기화

        tabLength = tabImages.Length;
        ChangeCategory(0);
        ShowSelectedTab(0);
    }

    public void ChangeCategory(int type)
    {
        illustratedBookType = (IllustratedBookType)type;
        bookSortingType = BookSortingType.ALL_RATING;
        sortDropdown.value = 0;
        switch (illustratedBookType)
        {
            case IllustratedBookType.WEAPON:
                sortDropdownObj.SetActive(true);
                SetActiveAllRatingContents(IllustratedBookType.WEAPON, true);
                SetActiveAllRatingContents(IllustratedBookType.ITEM, false);
                break;
            case IllustratedBookType.ITEM:
                sortDropdownObj.SetActive(true);
                SetActiveAllRatingContents(IllustratedBookType.WEAPON, false);
                SetActiveAllRatingContents(IllustratedBookType.ITEM, true);
                break;
            //case IllustratedBookType.MONSTER:
            //    break;
            default:
                sortDropdownObj.SetActive(false);
                break;
        }
        ShowSelectedTab(type);
    }

    public void ChangeContentsDisplay()
    {
        ActiveOnSpecificRatingContent(illustratedBookType, (BookSortingType)sortDropdown.value);
    }

    private void ActiveOnSpecificRatingContent(IllustratedBookType type, BookSortingType sortingType)
    {
        if (BookSortingType.ALL_RATING == sortingType)
        {
            SetActiveAllRatingContents(type, true);
            return;
        }

        SetActiveAllRatingContents(type, false);
        switch (type)
        {
            case IllustratedBookType.WEAPON:
                setActiveWeaponContents[(int)sortingType - 1](true);
                break;
            case IllustratedBookType.ITEM:
                setActiveItemContents[(int)sortingType - 1](true);
                break;
            //case IllustratedBookType.MONSTER:
            //    break;
            default:
                sortDropdownObj.SetActive(false);
                break;
        }
    }

    private void SetActiveAllRatingContents(IllustratedBookType type, bool show)
    {
        switch (type)
        {
            case IllustratedBookType.WEAPON:
                for(int i = 0; i < (int)Rating.E; i++)
                {
                    setActiveWeaponContents[i](show);
                }
                break;
            case IllustratedBookType.ITEM:
                for (int i = 0; i < (int)Rating.E; i++)
                {
                    setActiveItemContents[i](show);
                }
                break;
            //case IllustratedBookType.MONSTER:
            //    break;
            default:
                sortDropdownObj.SetActive(false);
                break;
        }
    }

    public void OpenBook()
    {
        bookUI.SetActive(true);
        AudioManager.Instance.PlaySound(0, SOUNDTYPE.UI);
    }
    public void CloseBook()
    {
        bookUI.SetActive(false);
        AudioManager.Instance.PlaySound(0, SOUNDTYPE.UI);
    }


    private void ShowSelectedTab(int type)
    {
        tabImages[type].sprite = selectedImage;
        tabTexts[type].color = Color.black;
        for (int i = 0; i < tabLength; i++)
        {
            if(i != type)
            {
                tabImages[i].sprite = unselectedImage;
                tabTexts[i].color = Color.white;
            }
        }
    }


    #endregion
}