using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class MainScreenController : MonoBehaviour
{
    private VisualElement rootElement;
    private Button homeButton;
    private Button gachaButton;
    private Button formationButton;
    private Button enhanceButton;
    private Button shopButton;
    private List<Button> navButtons;

    private VisualElement homeScreenContentContainer;
    private VisualElement gachaScreenContentContainer;
    private VisualElement formationScreenContentContainer;
    private VisualElement enhanceScreenContentContainer;
    private VisualElement shopScreenContentContainer;
    private List<VisualElement> screenContainers;

    private VisualTreeAsset gachaScreenAsset;
    private VisualElement loadedGachaContent;
    private GachaScreenController gachaController;

    private VisualTreeAsset formationScreenAsset;
    private VisualElement loadedFormationScreenContent;
    private FormationScreenController formationController;

    private System.Action homeButtonAction;
    private System.Action gachaButtonAction;
    private System.Action formationButtonAction;
    private System.Action enhanceButtonAction;
    private System.Action shopButtonAction;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) { Debug.LogError("MainScreenController: UIDocument component not found."); return; }
        rootElement = uiDocument.rootVisualElement;
        if (rootElement == null) { Debug.LogError("MainScreenController: rootVisualElement is null."); return; }

        homeButton = rootElement.Q<Button>("home-button");
        gachaButton = rootElement.Q<Button>("gacha-button");
        formationButton = rootElement.Q<Button>("formation-button");
        enhanceButton = rootElement.Q<Button>("enhance-button");
        shopButton = rootElement.Q<Button>("shop-button");
        navButtons = new List<Button> { homeButton, gachaButton, formationButton, enhanceButton, shopButton };

        homeScreenContentContainer = rootElement.Q<VisualElement>("home-screen-content-container");
        gachaScreenContentContainer = rootElement.Q<VisualElement>("gacha-screen-content-container");
        formationScreenContentContainer = rootElement.Q<VisualElement>("formation-screen-content-container");
        enhanceScreenContentContainer = rootElement.Q<VisualElement>("enhance-screen-content-container");
        shopScreenContentContainer = rootElement.Q<VisualElement>("shop-screen-content-container");
        screenContainers = new List<VisualElement> { homeScreenContentContainer, gachaScreenContentContainer, formationScreenContentContainer, enhanceScreenContentContainer, shopScreenContentContainer };

        gachaScreenAsset = Resources.Load<VisualTreeAsset>("UI_Toolkit/GachaScreen/GachaScreen");
        if (gachaScreenAsset == null) { Debug.LogError("MainScreenController: GachaScreen.uxml (VisualTreeAsset) not found!"); }
        formationScreenAsset = Resources.Load<VisualTreeAsset>("UI_Toolkit/FormationScreen/FormationScreen");
        if (formationScreenAsset == null) { Debug.LogError("MainScreenController: FormationScreen.uxml (VisualTreeAsset) not found!"); }

        if (gachaController == null) {
            gachaController = new GachaScreenController();
        }
        if (formationController == null) {
            formationController = new FormationScreenController();
        }

        homeButtonAction = () => { SetActiveButton(homeButton); ShowScreen(homeScreenContentContainer, "ホーム (戦闘エリア)"); };
        gachaButtonAction = () => { SetActiveButton(gachaButton); ShowScreen(gachaScreenContentContainer, "ガチャ"); };
        formationButtonAction = () => { SetActiveButton(formationButton); ShowScreen(formationScreenContentContainer, "編成"); };
        enhanceButtonAction = () => { SetActiveButton(enhanceButton); ShowScreen(enhanceScreenContentContainer, "強化"); };
        shopButtonAction = () => { SetActiveButton(shopButton); ShowScreen(shopScreenContentContainer, "購買"); };

        RegisterButtonCallback(homeButton, homeButtonAction);
        RegisterButtonCallback(gachaButton, gachaButtonAction);
        RegisterButtonCallback(formationButton, formationButtonAction);
        RegisterButtonCallback(enhanceButton, enhanceButtonAction);
        RegisterButtonCallback(shopButton, shopButtonAction);

        SetActiveButton(homeButton);
        ShowScreen(homeScreenContentContainer, "ホーム (戦闘エリア)");
    }

    void RegisterButtonCallback(Button button, System.Action onClickAction)
    {
        if (button != null) {
            button.clicked -= onClickAction;
            button.clicked += onClickAction;
        }
    }

    void SetActiveButton(Button activeButton)
    {
        if (activeButton == null) return;
        foreach (var btn in navButtons) {
            if (btn != null) {
                btn.EnableInClassList("active", btn == activeButton);
            }
        }
    }

    void ShowScreen(VisualElement screenToShow, string screenName)
    {
        if (screenToShow == null) {
            Debug.LogError($"Screen container for '{screenName}' is null.");
            return;
        }
        foreach (var container in screenContainers) {
            if (container != null) {
                container.style.display = (container == screenToShow) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        if (screenToShow == gachaScreenContentContainer) {
            LoadAndShowGachaContent();
        } else if (screenToShow == formationScreenContentContainer) {
            LoadAndShowFormationContent();
        } else {
            if(screenToShow.childCount == 0 || screenName == "ホーム (戦闘エリア)") {
                screenToShow.Clear();
                Label placeholder = new Label($"{screenName}のコンテンツ");
                placeholder.AddToClassList("screen-placeholder-text");
                placeholder.style.flexGrow = 1;
                placeholder.style.unityTextAlign = TextAnchor.MiddleCenter;
                screenToShow.Add(placeholder);
            }
        }
    }

    void LoadAndShowGachaContent()
    {
        if (gachaScreenAsset == null) { Debug.LogError("Cannot load gacha content: gachaScreenAsset is null."); return; }
        if (gachaScreenContentContainer == null) { Debug.LogError("Cannot load gacha content: gachaScreenContentContainer is null."); return; }
        if (gachaController == null) { Debug.LogError("Cannot load gacha content: gachaController is null."); return; }


        if (loadedGachaContent == null || loadedGachaContent.parent != gachaScreenContentContainer) {
            gachaScreenContentContainer.Clear();
            loadedGachaContent = gachaScreenAsset.Instantiate();
            loadedGachaContent.name = "gacha-content-instance";
            StyleElementForScreenContent(loadedGachaContent);
            gachaScreenContentContainer.Add(loadedGachaContent);
            gachaController.SetupGachaScreenControls(loadedGachaContent);
        }
    }

    void LoadAndShowFormationContent()
    {
        if (formationScreenAsset == null) { Debug.LogError("Cannot load formation content: formationScreenAsset is null."); return; }
        if (formationScreenContentContainer == null) { Debug.LogError("Cannot load formation content: formationScreenContentContainer is null."); return; }
        if (formationController == null) { Debug.LogError("Cannot load formation content: formationController is null."); return; }

        if (loadedFormationScreenContent == null || loadedFormationScreenContent.parent != formationScreenContentContainer) {
            formationScreenContentContainer.Clear();
            loadedFormationScreenContent = formationScreenAsset.Instantiate();
            loadedFormationScreenContent.name = "formation-content-instance";
            StyleElementForScreenContent(loadedFormationScreenContent);
            formationScreenContentContainer.Add(loadedFormationScreenContent);
            formationController.Initialize(loadedFormationScreenContent);
        }
    }

    void StyleElementForScreenContent(VisualElement element)
    {
        if (element == null) return;
        element.style.flexGrow = 1;
        element.style.width = new StyleLength(Length.Percent(100));
        element.style.height = new StyleLength(Length.Percent(100));
    }

    // BattleManagerから呼ばれるメソッド
    public List<CharacterData> GetCurrentFormationDataForBattle()
    {
        if (formationController != null)
        {
            // ★★★ ここの呼び出しを FormationScreenController のメソッド名に合わせる ★★★
            return formationController.GetCurrentActiveFormationForBattle(); // 修正済み
        }
        Debug.LogWarning("MainScreenController: formationController is null. Returning empty list for battle formation.");
        return new List<CharacterData>();
    }

    public void UpdateBattleLogOnUI(string message) // BattleManagerから呼ばれることを想定
    {
        var battleLogLabel = rootElement?.Q<Label>("battle-log-label"); // UXMLにこの名前のLabelが必要
        if (battleLogLabel != null)
        {
            battleLogLabel.text += message + "\n";
        }
    }

    void OnDisable()
    {
        if (homeButton != null && homeButtonAction != null) homeButton.clicked -= homeButtonAction;
        if (gachaButton != null && gachaButtonAction != null) gachaButton.clicked -= gachaButtonAction;
        if (formationButton != null && formationButtonAction != null) formationButton.clicked -= formationButtonAction;
        if (enhanceButton != null && enhanceButtonAction != null) enhanceButton.clicked -= enhanceButtonAction;
        if (shopButton != null && shopButtonAction != null) shopButton.clicked -= shopButtonAction;

        if (gachaController != null) {
            // gachaController.OnScreenClosed -= HandleGachaScreenClosed;
            gachaController.UnregisterCallbacks();
        }
        if (formationController != null) {
            formationController.UnregisterCallbacks();
        }
    }
}