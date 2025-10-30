using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusDisplayController : MonoBehaviour
{
    public BattleCharacterStatus character;
    
    [Header("HP UI")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;
    
    [Header("SP UI")]
    public TextMeshProUGUI spText;
    
    [Header("Settings")]
    public bool isEnemy = false; // Inspectorで設定

    void Start()
    {
        character.OnHPChanged += UpdateHP;
        character.OnSPChanged += UpdateSP;
        
        UpdateHP(character.currentHP, character.maxHP);
        UpdateSP(character.currentSP, character.maxSP);
        
        // 敵の場合はSP表示を非表示
        if (isEnemy && spText != null)
        {
            spText.gameObject.SetActive(false);
        }
    }

    void UpdateHP(int current, int max)
    {
        hpSlider.value = (float)current / max;
        hpText.text = $"{current} / {max}";
    }

    void UpdateSP(int current, int max)
    {
        // 敵の場合は更新しない
        if (isEnemy) return;
        
        spText.text = $"{current} / {max}";
    }

    void OnDestroy()
    {
        character.OnHPChanged -= UpdateHP;
        character.OnSPChanged -= UpdateSP;
    }
}