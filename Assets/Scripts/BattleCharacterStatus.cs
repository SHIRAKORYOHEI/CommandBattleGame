using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleCharacterStatus : MonoBehaviour
{
    public CharacterStats stats;
    
    public int currentHP;
    public int currentSP;
    
    public int maxHP => stats.maxHP;
    public int maxSP => stats.maxSP;

    public bool isReflecting = false;
    public int reflectCount = 0;
    
    public event Action<int, int> OnHPChanged;
    public event Action<int, int> OnSPChanged;

    private void Awake()
    {
        currentHP = stats.maxHP;
        currentSP = stats.maxSP;
    }

    public bool TakeDamage(int damage)
    {
        // 反射中なら跳ね返す判定
        if (isReflecting && reflectCount > 0)
        {
            reflectCount--;
        
            int roll = Random.Range(0, 100);
            Debug.Log($"{gameObject.name} 反射判定: {roll}% (残り{reflectCount}回)");
        
            if (roll < stats.reflectSuccessRate)
            {
                Debug.Log($"{gameObject.name} が攻撃を反射！");
            
                if (reflectCount <= 0)
                {
                    isReflecting = false;
                    Debug.Log($"{gameObject.name} の反射回数が終了");
                }
            
                return true; // 反射成功
            }
            else
            {
                Debug.Log($"{gameObject.name} の反射が失敗... ダメージを受ける");
            }
        
            if (reflectCount <= 0)
            {
                isReflecting = false;
                Debug.Log($"{gameObject.name} の反射回数が終了");
            }
        }
    
        // ダメージ処理
        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;
        OnHPChanged?.Invoke(currentHP, maxHP);
    
        return false; // 反射失敗 or 反射中でない
    }
    
    public bool IsDead()
    {
        return currentHP <= 0;
    }

    public void OnDeath()
    {
        GetComponent<SpriteRenderer>().color = Color.gray;
    }
    public void OnTurnStart()
    {
        // 必要に応じてバフ・デバフの更新処理
    }
    
    public bool UseSP(int amount)
    {
        if (currentSP < amount) return false;
        
        currentSP -= amount;
        OnSPChanged?.Invoke(currentSP, maxSP);
        return true;
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        OnHPChanged?.Invoke(currentHP, maxHP);
    }

    public void RecoverSP(int amount)
    {
        currentSP = Mathf.Min(currentSP + amount, maxSP);
        OnSPChanged?.Invoke(currentSP, maxSP);
    }
    
    public void ActivateReflect()
    {
        // すでに反射中の場合は上書きしない
        if (isReflecting && reflectCount > 0)
        {
            Debug.Log($"{gameObject.name} はすでに反射スキルが有効です");
            return;
        }
        
        isReflecting = true;
        reflectCount = stats.reflectMaxCount;
        Debug.Log($"{gameObject.name} の反射スキル発動！{reflectCount}回まで{stats.reflectSuccessRate}%の確率で攻撃を跳ね返す");
    }
}