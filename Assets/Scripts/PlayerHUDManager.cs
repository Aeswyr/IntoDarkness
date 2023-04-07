using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDManager : Singleton<PlayerHUDManager>
{
    [Header("Bars")]
    [SerializeField] private GameObject resourcePrefab;
    [SerializeField] private Image staminaBar;

    [SerializeField] private Sprite healthEmptySprite;
    [SerializeField] private Sprite healthFullSprite;

    [SerializeField] private Sprite focusEmptySprite;
    [SerializeField] private Sprite focusFullSprite;

    [SerializeField] private GameObject healthHolder;
    [SerializeField] private GameObject focusHolder;
    [SerializeField] private GameObject staminaHolder;

    [SerializeField] private float maxStaminaMod;

    [Header("Resource Cards")]
    [SerializeField] private GameObject resourceHolder;
    [SerializeField] private GameObject cardHolder;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] List<Sprite> resourceSprites;

    List<Image> healthUnits = new List<Image>();
    List<Image> focusUnits = new List<Image>();

    public void RefreshHealthBar(int maxHealth, int currentHealth) {
        if (healthUnits.Count != maxHealth) {
            foreach (var unit in healthUnits) {
                Destroy(unit.gameObject);
            }
            healthUnits.Clear();
            for (int i = 0; i < maxHealth; i++)
                healthUnits.Add(Instantiate(resourcePrefab, healthHolder.transform).GetComponent<Image>());
        }

        for (int i = 0; i < healthUnits.Count; i++) {
            if (i < currentHealth)
                healthUnits[i].sprite = healthFullSprite;
            else
                healthUnits[i].sprite = healthEmptySprite;
        }
    }

    public void RefreshFocusBar(int maxFocus, int currentFocus) {
        if (focusUnits.Count != maxFocus) {
            foreach (var unit in focusUnits) {
                Destroy(unit.gameObject);
            }
            focusUnits.Clear();
            for (int i = 0; i < maxFocus; i++)
                focusUnits.Add(Instantiate(resourcePrefab, focusHolder.transform).GetComponent<Image>());
        }

        for (int i = 0; i < focusUnits.Count; i++) {
            if (i < currentFocus)
                focusUnits[i].sprite = focusFullSprite;
            else
                focusUnits[i].sprite = focusEmptySprite;
        }
    }

    public void RefreshStaminaBar(int maxStamina, int currentStamina, bool locked) {
        var scale = staminaHolder.transform.localScale;
        scale.x = maxStamina / maxStaminaMod;
        staminaHolder.transform.localScale = scale;

        staminaBar.fillAmount = (float) currentStamina / maxStamina;

        if (locked)
            staminaBar.color = Color.gray;
        else
            staminaBar.color = Color.white;

    }
    
    Dictionary<ResourceCardType, GameObject> holderMap = new();
    Dictionary<ResourceCardType, List<GameObject>> cardList = new();

    public void AddResource(ResourceCardType type, int amt = 1) {
        if (!holderMap.ContainsKey(type)) {
            holderMap.Add(type, Instantiate(cardHolder, resourceHolder.transform));
            cardList.Add(type, new());
        }
            

        for (int i = 0; i < amt; i++) {
            var card = Instantiate(cardPrefab, holderMap[type].transform);
            card.GetComponent<Image>().sprite = resourceSprites[(int)type];
            cardList[type].Add(card);
        }
    }

    public void RemoveResource(ResourceCardType type, int amt = 1) {
        if (!cardList.ContainsKey(type))
            return;

        amt = Mathf.Min(amt, cardList[type].Count);

        for (int i = amt - 1; i >= 0; i--) {
            Destroy(cardList[type][i]);
            cardList[type].RemoveAt(i);
        }

        if (cardList[type].Count == 0) {
            cardList.Remove(type);
            Destroy(holderMap[type]);
            holderMap.Remove(type);
        }
    }
}
