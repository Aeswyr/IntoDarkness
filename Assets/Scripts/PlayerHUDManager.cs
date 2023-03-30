using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDManager : Singleton<PlayerHUDManager>
{
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
    
}
