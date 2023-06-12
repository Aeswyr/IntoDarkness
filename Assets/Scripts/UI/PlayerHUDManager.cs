using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUDManager : Singleton<PlayerHUDManager>
{
    [Header("Bars")]

    [SerializeField] private RectTransform healthHolder;
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI healthCounter;
    [SerializeField] private RectTransform healthMarker;
    [SerializeField] private RectTransform focusHolder;
    [SerializeField] private Image focusBar;
    [SerializeField] private TextMeshProUGUI focusCounter;
    [SerializeField] private RectTransform focusMarker;

    [SerializeField] private GameObject exertPrefab;
    [SerializeField] private Transform exertHolder;
    [SerializeField] private Sprite exertFull;
    [SerializeField] private Sprite exertEmpty;
    private List<GameObject> exert = new();

    [Header("Resource Cards")]
    [SerializeField] private GameObject resourceHolder;
    [SerializeField] private GameObject cardHolder;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] List<Sprite> resourceSprites;

    [Header("Info")]
    [SerializeField] private GameObject clockBar;
    [SerializeField] private float durationDay;
    [SerializeField] private float durationNight;

    [Header("Other")]
    [SerializeField] private Animator animator;
    [SerializeField] private Image fireBar;

    public void RefreshHealthBar(int maxHealth, int currentHealth) {
        healthCounter.text = $"{currentHealth}/{maxHealth}";

        float maxWidth = healthHolder.rect.width - 2;
        float offset = maxWidth * (float)currentHealth / maxHealth;



        healthBar.fillAmount = (float)currentHealth / maxHealth;

        Vector3 pos = healthMarker.anchoredPosition;
        if (currentHealth > 0)
            pos.x = offset;
        else
            pos.x = -1;
        healthMarker.anchoredPosition = pos;
    }

    public void RefreshFocusBar(int maxFocus, int currentFocus) {
        focusCounter.text = $"{currentFocus}/{maxFocus}";

        float maxWidth = focusHolder.rect.width - 2;
        float offset = maxWidth * (float)currentFocus / maxFocus;



        focusBar.fillAmount = (float)currentFocus / maxFocus;

        Vector3 pos = focusMarker.anchoredPosition;
        if (currentFocus > 0)
            pos.x = offset;
        else
            pos.x = -1;
        focusMarker.anchoredPosition = pos;
    }

    public void RefreshExertBar(int maxExert, int currentExert) {
        foreach (var obj in exert)
            Destroy(obj);
        exert.Clear();

        for (int i = 0; i < maxExert; i++) {
            var newExert = Instantiate(exertPrefab, exertHolder);
            exert.Add(newExert);
            var image = newExert.GetComponent<Image>();
            if (i < currentExert)
                image.sprite = exertFull;
            else
                image.sprite = exertEmpty;
        }
    }

    public void RefreshFireBar(int maxFire, int currentFire) {
        fireBar.fillAmount = (float)currentFire / maxFire;
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

    public void SetTime(float time) {
        float cycleDuration = durationDay + durationNight;
        float cycleTime = time % cycleDuration;

        float projectionTime = 0.5f * cycleTime / durationDay;
        if (cycleTime > durationDay)
            projectionTime = 0.5f + 0.5f * (cycleTime - durationDay) / durationNight;

        Vector3 position = clockBar.transform.localPosition;
        position.x = -32 + -((96 + Mathf.CeilToInt(128 * projectionTime)) % 128);
        clockBar.transform.localPosition = position;

    }

    public void ToggleHUD(bool toggle) {
        if (toggle)
            animator.SetTrigger("hud_enable");
        else
            animator.SetTrigger("hud_disable");
    }
}
