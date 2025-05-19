using UnityEngine;
using UnityEngine.UIElements;

public class GachaScreenController
{
    private VisualElement gachaScreenRootContent; // UXMLからロードされたガチャ画面のコンテンツ部分
    private Image characterImageElement;
    private Button summonGoldButton;
    private Button summonTicketButton;

    // public event System.Action OnScreenClosed; // モーダルではないので、このイベントは不要になる可能性

    // MainScreenControllerから、GachaScreen.uxmlをインスタンス化したVisualElementが渡される
    public void SetupGachaScreenControls(VisualElement gachaContentLoadedElement)
    {
        gachaScreenRootContent = gachaContentLoadedElement;
        if (gachaScreenRootContent == null) {
            Debug.LogError("GachaScreenController: Passed gachaContentLoadedElement is null!");
            return;
        }
        Debug.Log($"GachaScreenController.SetupGachaScreenControls called with VE: {gachaScreenRootContent.name}");

        characterImageElement = gachaScreenRootContent.Q<Image>("character-image");
        summonGoldButton = gachaScreenRootContent.Q<Button>("summon-gold-button");
        summonTicketButton = gachaScreenRootContent.Q<Button>("summon-ticket-button");

        if (summonGoldButton != null) {
            summonGoldButton.clicked -= SummonWithGold;
            summonGoldButton.clicked += SummonWithGold;
        } else { Debug.LogWarning("GachaScreenController: summon-gold-button not found."); }

        if (summonTicketButton != null) {
            summonTicketButton.clicked -= SummonWithTicket;
            summonTicketButton.clicked += SummonWithTicket;
        } else { Debug.LogWarning("GachaScreenController: summon-ticket-button not found."); }

        if (characterImageElement != null) {
            Texture2D sampleCharTexture = Resources.Load<Texture2D>("UI_Toolkit/GachaScreen/sample_character");
            if (sampleCharTexture != null) {
                characterImageElement.image = sampleCharTexture;
            } else {
                Debug.LogWarning("GachaScreenController: Sample character texture not found.");
            }
        }
    }

    private void SummonWithGold() {
        Debug.Log($"GachaScreenController: Summon with Gold button clicked!");
    }

    private void SummonWithTicket() {
        Debug.Log($"GachaScreenController: Summon with Ticket button clicked!");
    }

    // Show/HideメソッドはMainScreenControllerが親コンテナのdisplayを制御するので、ここでは不要
    // public void Show() { ... }
    // public void Hide() { ... }

    public void UnregisterCallbacks() {
        Debug.Log("GachaScreenController.UnregisterCallbacks() called.");
        if (gachaScreenRootContent == null) return;
        if (summonGoldButton != null) summonGoldButton.clicked -= SummonWithGold;
        if (summonTicketButton != null) summonTicketButton.clicked -= SummonWithTicket;
    }
}