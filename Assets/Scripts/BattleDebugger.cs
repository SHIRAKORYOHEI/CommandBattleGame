using UnityEngine;

public class BattleDebugger : MonoBehaviour
{
    public BattleCharacterStatus playerStatus;
    public BattleCharacterStatus enemyStatus;

    void Update()
    {
        // プレイヤーにダメージ
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            playerStatus.TakeDamage(10);
            Debug.Log($"プレイヤーにダメージ HP: {playerStatus.currentHP}");
        }

        // プレイヤーのSP消費
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            playerStatus.UseSP(1);
            Debug.Log($"プレイヤーSP消費 SP: {playerStatus.currentSP}");
        }

        // 敵にダメージ
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            enemyStatus.TakeDamage(15);
            Debug.Log($"敵にダメージ HP: {enemyStatus.currentHP}");
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            playerStatus.Heal(playerStatus.maxHP);
            Debug.Log("プレイヤーHP全回復");
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            playerStatus.RecoverSP(playerStatus.maxSP);
            Debug.Log("プレイヤーSP全回復");
        }
        
    }
}