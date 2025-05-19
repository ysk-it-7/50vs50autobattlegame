using UnityEngine;
using UnityEngine.UIElements;

public class FormationDragDropController
{
    private VisualElement rootElementForEvents; // ドラッグ中のグローバルイベントをキャプチャする要素 (通常は画面全体)
    private VisualElement ghostIconElement;     // ドラッグ中に表示するゴーストアイコン
    private PlacementAreaUIController placementAreaController; // ドロップ先のグリッド座標計算用

    private bool isDragging = false;
    private CharacterData draggedCharacterData = null;
    private VisualElement originalListItem; // ドラッグ元のリストアイテム (見た目を戻すため)

    // ドラッグ＆ドロップが完了し、配置エリアのグリッドにドロップされたことを通知するイベント
    public event System.Action<CharacterData, Vector2Int> OnCharacterDroppedOnGridEvent;


    public FormationDragDropController(VisualElement root, VisualElement ghostIcon, PlacementAreaUIController placementController)
    {
        rootElementForEvents = root;
        ghostIconElement = ghostIcon;
        placementAreaController = placementController;

        if (rootElementForEvents == null) Debug.LogError("DragDropController: rootElementForEvents is null.");
        if (ghostIconElement == null) Debug.LogError("DragDropController: ghostIconElement is null.");
        if (placementAreaController == null) Debug.LogError("DragDropController: placementAreaController is null.");
    }

    // キャラクターリストのアイテムでPointerDownされたときに呼ばれる
    public void StartDrag(PointerDownEvent evt, CharacterData charData, VisualElement listItem)
    {
        if (evt.button != 0 || isDragging) return; // 左クリックのみ、既にドラッグ中は無視

        isDragging = true;
        draggedCharacterData = charData;
        originalListItem = listItem;
        // originalListItem?.AddToClassList("dragging-source"); // (オプション) ドラッグ元を示すスタイル

        if (ghostIconElement != null && draggedCharacterData.icon != null)
        {
            ghostIconElement.style.backgroundImage = new StyleBackground(draggedCharacterData.icon);
            UpdateGhostIconPosition(evt.position); // evt.positionはlistItemのローカル座標ではないので注意が必要
                                                   // listItemの親の座標系か、rootElementForEventsの座標系
                                                   // ここでは、rootElementForEventsに登録するので、そのローカル座標と解釈
            ghostIconElement.style.display = DisplayStyle.Flex;
            ghostIconElement.BringToFront();
        }

        // ルート要素にグローバルなポインターイベントを登録
        rootElementForEvents.RegisterCallback<PointerMoveEvent>(OnGlobalPointerMove);
        rootElementForEvents.RegisterCallback<PointerUpEvent>(OnGlobalPointerUp);
        evt.StopPropagation(); // 他の要素がこのPointerDownを処理しないように
    }

    private void UpdateGhostIconPosition(Vector2 panelPosition) // panelPositionはrootElementForEventsのローカル座標
    {
        if (ghostIconElement == null) return;
        float ghostWidth = ghostIconElement.resolvedStyle.width;
        float ghostHeight = ghostIconElement.resolvedStyle.height;
        ghostIconElement.style.left = panelPosition.x - ghostWidth / 2;
        ghostIconElement.style.top = panelPosition.y - ghostHeight / 2;
    }

    private void OnGlobalPointerMove(PointerMoveEvent evt)
    {
        if (!isDragging) return;
        UpdateGhostIconPosition(evt.position);

        // (オプション) ドロップ先のハイライト処理
        // Vector2 dropPosInPlacementArea = rootElementForEvents.ChangeCoordinatesTo(placementAreaController.GetPlacementAreaElement(), evt.position);
        // if (placementAreaController.GetPlacementAreaElement().ContainsPoint(dropPosInPlacementArea)) {
        //     Vector2Int gridPos = placementAreaController.GetGridPositionFromLocalPosition(dropPosInPlacementArea);
        //     placementAreaController.HighlightDropTarget(gridPos, true);
        // } else {
        //     // placementAreaController.HighlightDropTarget(Vector2Int.one * -1, false); // 無効な位置でハイライト解除
        // }
    }

    private void OnGlobalPointerUp(PointerUpEvent evt)
    {
        if (!isDragging) return;

        // グローバルイベントの解除
        rootElementForEvents.UnregisterCallback<PointerMoveEvent>(OnGlobalPointerMove);
        rootElementForEvents.UnregisterCallback<PointerUpEvent>(OnGlobalPointerUp);

        if (ghostIconElement != null) ghostIconElement.style.display = DisplayStyle.None;
        // if (originalListItem != null) originalListItem.RemoveFromClassList("dragging-source");

        // ドロップ処理
        if (placementAreaController != null && draggedCharacterData != null)
        {
            // evt.positionはrootElementForEventsのローカル座標
            // これをPlacementAreaのローカル座標に変換する必要がある
            VisualElement placementAreaVE = placementAreaController.GetPlacementAreaElement(); // Helperメソッドが必要
            if (placementAreaVE != null) {
                Vector2 dropPosInPlacementArea = rootElementForEvents.ChangeCoordinatesTo(placementAreaVE, evt.position);

                if (placementAreaVE.ContainsPoint(dropPosInPlacementArea))
                {
                    Vector2Int gridPos = placementAreaController.GetGridPositionFromLocalPosition(dropPosInPlacementArea);
                    if (gridPos.x >= 0 && gridPos.y >= 0) // 有効なグリッド座標か
                    {
                        Debug.Log($"DragDropController: Character '{draggedCharacterData.displayName}' dropped on grid {gridPos}.");
                        OnCharacterDroppedOnGridEvent?.Invoke(draggedCharacterData, gridPos);
                    }
                } else {
                    Debug.Log("DragDropController: Dropped outside placement area.");
                }
            }
        }

        isDragging = false;
        draggedCharacterData = null;
        originalListItem = null;
        evt.StopPropagation();
    }

    public void UnregisterGlobalCallbacks() // FormationScreenControllerのOnDisableで呼ばれる
    {
         if (rootElementForEvents != null) {
            rootElementForEvents.UnregisterCallback<PointerMoveEvent>(OnGlobalPointerMove);
            rootElementForEvents.UnregisterCallback<PointerUpEvent>(OnGlobalPointerUp);
        }
    }
}
