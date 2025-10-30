using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum CommandType
{
    Attack = 0,
    Skill = 1,
    Item = 2
}

public enum BattlePhase
{
    Start,
    PlayerTurn,
    EnemyTurn,
    End
}

public class BattleManager : MonoBehaviour
{
    public BattleCharacterStatus[] playerCharacters;
    public BattleCharacterStatus[] enemyCharacters;
    
    public UIManager uiManager;
    
    [Header("Item Settings")]
    public int itemHealAmount = 20;
    public int itemMaxCount = 5;
    private int itemCurrentCount;

    [Header("Turn Settings")]
    public float enemyActionDelay = 1.5f;

    private BattlePhase currentPhase;
    private int currentPlayerIndex = 0;
    private int currentTurnNumber = 1;
    private bool isWaitingForPlayerInput = false;
    
    private void Awake()
    {
        if (uiManager == null)
            Debug.LogError("UIManager が割り当てられていません！");

        for (int i = 0; i < playerCharacters.Length; i++)
        {
            if (playerCharacters[i] == null)
                Debug.LogError($"playerCharacters[{i}] が null です！（index: {i}）");
        }

        for (int i = 0; i < enemyCharacters.Length; i++)
        {
            if (enemyCharacters[i] == null)
                Debug.LogError($"enemyCharacters[{i}] が null です！（index: {i}）");
        }
    }

    private void Start()
    {
        itemCurrentCount = itemMaxCount;
        StartBattle();
    }

    public void StartBattle()
    {
        currentPhase = BattlePhase.Start;
        currentPlayerIndex = 0;
        currentTurnNumber = 1;
        
        Debug.Log("===== バトル開始！ =====");
        StartCoroutine(BattleFlow());
    }

    private IEnumerator BattleFlow()
    {
        while (currentPhase != BattlePhase.End)
        {
            switch (currentPhase)
            {
                case BattlePhase.Start:
                    yield return StartPhase();
                    break;
                    
                case BattlePhase.PlayerTurn:
                    yield return PlayerTurnPhase();
                    break;
                    
                case BattlePhase.EnemyTurn:
                    yield return EnemyTurnPhase();
                    break;
            }
        }
        
        Debug.Log("===== バトル終了！ =====");
    }

    private IEnumerator StartPhase()
    {
        Debug.Log($"===== ターン {currentTurnNumber} =====");
        currentPlayerIndex = 0;
        
        // ターン開始時の処理
        /*foreach (var character in playerCharacters)
        {
            if (character != null)
                character.OnTurnStart();
        }*/
        
        currentPhase = BattlePhase.PlayerTurn;
        yield return null;
    }

    private IEnumerator PlayerTurnPhase()
    {
        Debug.Log("=== PlayerTurnPhase START ===");
    
        while (currentPlayerIndex < playerCharacters.Length)
        {
            Debug.Log($"--- プレイヤー{currentPlayerIndex + 1}のターン ---");
        
            if (playerCharacters[currentPlayerIndex])
            {
                Debug.Log($"UIを表示します: ShowCommandUI({currentPlayerIndex})");
                uiManager.ShowCommandUI(currentPlayerIndex);
            
                isWaitingForPlayerInput = true;
                Debug.Log("入力待機開始");
            
                while (isWaitingForPlayerInput)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            
                Debug.Log("入力受付完了");
            }
            else
            {
                Debug.Log($"プレイヤー{currentPlayerIndex + 1}は行動不能");
            }
        
            currentPlayerIndex++;
            yield return new WaitForSeconds(0.3f);
        }
    
        Debug.Log("=== PlayerTurnPhase END ===");
        currentPhase = BattlePhase.EnemyTurn;
    }

    private IEnumerator EnemyTurnPhase()
    {
        Debug.Log("--- 敵のターン ---");
        
        // UIを非表示
        uiManager.HideCommandUI();
        
        // 全ての敵が行動
        for (int i = 0; i < enemyCharacters.Length; i++)
        {
            if (!enemyCharacters[i].IsDead())
            {
                yield return new WaitForSeconds(enemyActionDelay);
                ExecuteEnemyAction(i);
            }
        }
        
        // 勝敗判定
        if (CheckBattleEnd())
        {
            currentPhase = BattlePhase.End;
        }
        else
        {
            // 次のターンへ
            currentTurnNumber++;
            currentPhase = BattlePhase.Start;
        }
        
        yield return new WaitForSeconds(1f);
    }

    public void ExecuteCommand(CommandType command, int targetIndex)
    {
        BattleCharacterStatus actor = playerCharacters[currentPlayerIndex];

        if (actor == null || actor.IsDead())
        {
            Debug.Log("このキャラクターは行動できません");
            isWaitingForPlayerInput = false;
            return;
        }

        switch (command)
        {
            case CommandType.Attack:
                if (targetIndex >= 0 && targetIndex < enemyCharacters.Length)
                    Attack(actor, enemyCharacters[targetIndex]);
                break;
            
            case CommandType.Item:
                if (targetIndex >= 0 && targetIndex < playerCharacters.Length)
                    UseItem(playerCharacters[targetIndex]);
                break;
            
            case CommandType.Skill:
                UseReflectSkill(actor);
                break;
        }
    
        // 勝敗判定
        if (CheckBattleEnd())
            currentPhase = BattlePhase.End;
    
        // 入力待ちを解除（次のプレイヤーへ）
        isWaitingForPlayerInput = false;
    }

    private void ExecuteEnemyAction(int enemyIndex)
    {
        BattleCharacterStatus enemy = enemyCharacters[enemyIndex];
        
        // 生きているプレイヤーをランダムに選択
        List<BattleCharacterStatus> aliveTargets = playerCharacters.Where(player => !player.IsDead()).ToList();

        if (aliveTargets.Count > 0)
        {
            BattleCharacterStatus target = aliveTargets[UnityEngine.Random.Range(0, aliveTargets.Count)];
            Attack(enemy, target);
        }
    }

    private bool CheckBattleEnd()
    {
        // プレイヤー全滅チェック
        bool allPlayersDead = true;
        int alivePlayerCount = 0;
    
        foreach (var player in playerCharacters)
        {
            if (player != null && !player.IsDead())
            {
                allPlayersDead = false;
                alivePlayerCount++;
            }
        }
    
        if (allPlayersDead)
        {
            StartCoroutine(ShowResultScreen(false));
            return true;
        }
    
        // 敵全滅チェック
        bool allEnemiesDead = true;
        int aliveEnemyCount = 0;
    
        foreach (var enemy in enemyCharacters)
        {
            if (enemy != null && !enemy.IsDead())
            {
                allEnemiesDead = false;
                aliveEnemyCount++;
            }
        }
    
        if (allEnemiesDead)
        {
            StartCoroutine(ShowResultScreen(true));
            return true;
        }
        return false;
    }

    void Attack(BattleCharacterStatus attacker, BattleCharacterStatus target)
    {
        if (target.IsDead())
        {
            Debug.Log($"{target.gameObject.name}はすでに倒れている");
            return;
        }
    
        int damage = attacker.stats.attackDamage;
    
        // ターゲットにダメージ（反射判定含む）
        bool reflected = target.TakeDamage(damage);
    
        if (reflected)
        {
            // 反射成功：攻撃者にダメージを返す
            attacker.TakeDamage(damage);
            Debug.Log($"攻撃が反射された！{attacker.gameObject.name}に{damage}ダメージ");
        }
        else
        {
            Debug.Log($"{attacker.gameObject.name}が{target.gameObject.name}に{damage}ダメージ");
        }
    
        // 死亡チェック
        if (target.IsDead())
        {
            Debug.Log($"{target.gameObject.name}は倒れた！");
        }
    
        if (attacker.IsDead())
        {
            Debug.Log($"{attacker.gameObject.name}は反射ダメージで倒れた！");
        }
    }

    void UseItem(BattleCharacterStatus target)
    {
        // 死んでいるキャラには使えない
        if (target.IsDead())
        {
            Debug.Log($"{target.gameObject.name}は倒れているのでアイテムは使えない");
            return;
        }
    
        if (itemCurrentCount > 0)
        {
            itemCurrentCount--;
            target.Heal(itemHealAmount);
            Debug.Log($"アイテムを使用！{target.gameObject.name}のHPを{itemHealAmount}回復　残り{itemCurrentCount}個");
        }
    }

    public bool CanUseItem()
    {
        return itemCurrentCount > 0;
    }

    void UseReflectSkill(BattleCharacterStatus caster)
    {
        if (caster.isReflecting && caster.reflectCount > 0)
        {
            Debug.Log($"{caster.gameObject.name} はすでに反射スキルが有効です");
            return; 
        }
    
        if (!caster.UseSP(caster.stats.reflectSkillSPCost))
        {
            Debug.Log($"{caster.gameObject.name} SPが足りない！");
            return;
        }

        caster.ActivateReflect();
        Debug.Log($"{caster.gameObject.name} が反射スキルを使用！");
    }
    
    public int GetCurrentTurn()
    {
        return currentTurnNumber;
    }
    
    public BattlePhase GetCurrentPhase()
    {
        return currentPhase;
    }
    
    private IEnumerator ShowResultScreen(bool isVictory)
    {
        yield return new WaitForSeconds(1f);
        uiManager.ShowResultScreen(isVictory);
    }
}