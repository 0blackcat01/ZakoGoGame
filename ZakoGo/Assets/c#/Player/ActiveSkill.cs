using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;


public class ActiveSkill : MonoBehaviour
{
    [SerializeField] private int maxCharges = 4;
    [SerializeField] private float chargeCooldown = 120f;
    [SerializeField] private Image[] chargeIndicators;
    public Sprite[] sprites;

    public int currentCharges = 0;
    private float cooldownTimer = 0f;
    private bool isCooldownActive = false;

    public TextMeshProUGUI CostTxt;

    private void Start()
    {
        chargeCooldown = 60 - GameNum.CostRestore;
        if (chargeIndicators.Length != maxCharges)
        {
            Debug.LogError("Charge indicators count doesn't match max charges!");
        }
        UpdateChargeUI();
        CostTxt.text = currentCharges.ToString();
    }

    private void Update()
    {
        if (currentCharges < maxCharges && !isCooldownActive)
        {
            StartCoroutine(ChargeCooldown());
        }
    }

    private IEnumerator ChargeCooldown()
    {
        isCooldownActive = true;
        cooldownTimer = chargeCooldown;

        while (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
            yield return null;
        }

        currentCharges = Mathf.Min(currentCharges + 1, maxCharges);
        CostTxt.text = currentCharges.ToString();
        UpdateChargeUI();
        isCooldownActive = false;
    }

    public void UseSkill(int Cost)
    {
        if (currentCharges >= Cost)
        {
            Debug.Log("Skill activated!");
            currentCharges -= Cost;
            UpdateChargeUI();
        }
        else
        {
            Debug.Log("Not enough charges!");
        }
    }

    private void UpdateChargeUI()
    {
        for (int i = 0; i < chargeIndicators.Length; i++)
        {
            if(i < currentCharges)
            {
                if(i == 3)
                {
                    chargeIndicators[i].sprite = sprites[2];
                }
                else
                {
                    chargeIndicators[i].sprite = sprites[1];
                }
                
            }
            else
            {
                chargeIndicators[i].sprite = sprites[0];
            }
            
        }
    }
}