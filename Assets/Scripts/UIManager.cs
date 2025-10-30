using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour
{
    public BattleManager battleManager;
    
    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private GameObject commandUI;
    [SerializeField] private GameObject targetSelectionUI;
    
    [Header("Target Buttons")]
    [SerializeField] private TargetButton[] targetButtons;
    private List<TargetButton> activeTargetButtons = new List<TargetButton>();
    
    private CommandType selectedCommand;
    private int currentPlayerIndex = 0;
    
    private GameObject lastSelectedTarget;
    [SerializeField] private Button cancelButton;
    
    [SerializeField] private TextMeshProUGUI resultText;
    
    private void Start()
    {
        commandUI.SetActive(false);
        targetSelectionUI.SetActive(false);
        resultText.gameObject.SetActive(false);
        Cursor.visible = false;
    }

    void Update()
    {
        if (EventSystem.current == null)
        {
            Debug.LogError("EventSystem.current が null！");
            return;
        }
    
        GameObject selected = EventSystem.current.currentSelectedGameObject;
    
        // キャンセルボタンが選択中の時の処理
        if (cancelButton != null && selected == cancelButton.gameObject)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                OnCancelButtonNavigateUp();
            }
        
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                NavigateFromCancel(Vector3.left);
            }
        
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                NavigateFromCancel(Vector3.right);
            }
        }
    }
    
    public void ShowCommandUI(int playerIndex)
    {
        currentPlayerIndex = playerIndex;
    
        if (commandUI != null)
        {
            commandUI.SetActive(true);
        
            // コマンドUIの中から最初のアクティブなボタンを探す
            SelectFirstActiveButton(commandUI);
        }
        Debug.Log($"プレイヤー{playerIndex + 1}のコマンド選択中...");
    }
    
// 1フレーム待ってから選択（確実に反映させる）
    private IEnumerator SelectButtonNextFrame()
    {
        yield return null;
    
        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // 一度クリア
            yield return null;
            EventSystem.current.SetSelectedGameObject(firstSelectedButton); // 再選択
        }
    }

    public void HideCommandUI()
    {
        if (commandUI != null)
            commandUI.SetActive(false);
        
        if (targetSelectionUI != null)
            targetSelectionUI.SetActive(false);
    }

    public void OnCommandButtonPressed(int commandTypeInt)
    {
        selectedCommand = (CommandType)commandTypeInt;

        switch (selectedCommand)
        {
            case CommandType.Attack:
                // 敵をターゲット選択
                ShowTargetSelection(true);
                break;
            
            case CommandType.Item:
                // 味方をターゲット選択
                ShowTargetSelection(false);
                break;
            
            case CommandType.Skill:
                commandUI.SetActive(false);
                battleManager.ExecuteCommand(selectedCommand, -1);
                break;
        }
    }

    private void ShowTargetSelection(bool selectEnemy)
    {
        commandUI.SetActive(false);
        targetSelectionUI.SetActive(true);
    
        // ターゲットボタンの表示を更新
        UpdateTargetButtons(selectEnemy);
    
        Debug.Log(selectEnemy ? "敵を選択してください" : "味方を選択してください");
    }

    private void SelectFirstActiveButton(GameObject parent)
    {
        Selectable firstButton = null;
    
        // 子オブジェクトからアクティブなSelectableを探す
        Selectable[] selectables = parent.GetComponentsInChildren<Selectable>(false);
    
        if (selectables.Length > 0)
            firstButton = selectables[0];
    
        if (firstButton != null)
            StartCoroutine(SelectButtonNextFrame(firstButton.gameObject));
        else
            Debug.LogWarning($"{parent.name} にアクティブなボタンがありません");
    }
    
    private IEnumerator SelectButtonNextFrame(GameObject button)
    {
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
    
        if (button != null && button.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(button);
            Debug.Log($"Selected: {button.name}");
        }
        else
        {
            Debug.LogWarning("選択しようとしたボタンが非アクティブです");
        }
    }
    // Navigationを動的に再構築
    private void RebuildNavigation(List<GameObject> activeButtons)
    {
        for (int i = 0; i < activeButtons.Count; i++)
        {
            Selectable selectable = activeButtons[i].GetComponent<Selectable>();
            if (selectable == null) continue;
        
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;
        
            if (i > 0)
                nav.selectOnLeft = activeButtons[i - 1].GetComponent<Selectable>();
        
            if (i < activeButtons.Count - 1)
                nav.selectOnRight = activeButtons[i + 1].GetComponent<Selectable>();
        
            if (cancelButton != null)
                nav.selectOnDown = cancelButton;
        
            selectable.navigation = nav;
        }
    
        // キャンセルボタンのNavigationも更新
        if (cancelButton != null && activeButtons.Count > 0)
        {
            Navigation cancelNav = new Navigation();
            cancelNav.mode = Navigation.Mode.Explicit;
        
            // 上は最初のアクティブボタン
            cancelNav.selectOnUp = activeButtons[0].GetComponent<Selectable>();
        
            cancelButton.navigation = cancelNav;
        }
    }

    public void OnTargetSelected(int targetIndex)
    {
        Debug.Log($"Target selected: {targetIndex}");
        
        // BattleManagerにコマンド実行を依頼
        battleManager.ExecuteCommand(selectedCommand, targetIndex);
        
        targetSelectionUI.SetActive(false);
    }

    public void OnCancelTargetSelection()
    {
        targetSelectionUI.SetActive(false);
        commandUI.SetActive(true);
    
        // コマンドUIの最初のボタンを選択
        SelectFirstActiveButton(commandUI);
    }
    
    

    public void OnTargetButtonSelected(GameObject selectedButton)
    {
        // キャンセルボタン以外が選択されたら記憶
        if (selectedButton != cancelButton.gameObject)
        {
            lastSelectedTarget = selectedButton;
        }
    }

    public void OnCancelButtonNavigateUp()
    {
        if (lastSelectedTarget != null)
            EventSystem.current.SetSelectedGameObject(lastSelectedTarget);
    }

    private void NavigateFromCancel(Vector3 direction)
    {
        if (lastSelectedTarget == null) return;
    
        // 最後に選んだボタンのNavigationを取得
        Selectable lastSelectable = lastSelectedTarget.GetComponent<Selectable>();
        if (lastSelectable == null) return;
    
        // 指定方向の次のボタンを取得
        Selectable nextSelectable = null;
    
        if (direction == Vector3.left)
        {
            nextSelectable = lastSelectable.FindSelectableOnLeft();
        }
        else if (direction == Vector3.right)
        {
            nextSelectable = lastSelectable.FindSelectableOnRight();
        }
    
        // 次のボタンが見つかったら選択
        EventSystem.current.SetSelectedGameObject(nextSelectable != null
            ? nextSelectable.gameObject
            // 見つからなかったら最後のボタンに戻る
            : lastSelectedTarget);
    }

    private void UpdateTargetButtons(bool showEnemies)
    {
        BattleCharacterStatus[] targets = showEnemies ? 
            battleManager.enemyCharacters : 
            battleManager.playerCharacters;
    
        List<GameObject> activeButtons = new List<GameObject>();
    
        for (int i = 0; i < targetButtons.Length; i++)
        {
            if (i < targets.Length && targets[i] != null && !targets[i].IsDead())
            {
                targetButtons[i].gameObject.SetActive(true);
                targetButtons[i].targetIndex = i;
                activeButtons.Add(targetButtons[i].gameObject);
            
                // ボタンのテキストを更新
                Text buttonText = targetButtons[i].GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    string name = showEnemies ? $"敵 {i + 1}" : $"味方 {i + 1}";
                    buttonText.text = $"{name}\nHP: {targets[i].currentHP}/{targets[i].maxHP}";
                }
            }
            else
            {
                targetButtons[i].gameObject.SetActive(false);
            }
        }
    
        // Navigationを再設定
        RebuildNavigation(activeButtons);
    
        // 最初の生きているボタンを選択
        if (activeButtons.Count > 0)
        {
            StartCoroutine(SelectButtonNextFrame(activeButtons[0]));
        }
    }
    
    public void ShowResultScreen(bool isVictory)
    {
        Debug.Log($"=== ShowResultScreen 呼ばれた: isVictory={isVictory} ===");
    
        HideCommandUI();
    
        if (resultText != null)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = isVictory ? "Victory!" : "Defeat...";
        }
    }
}