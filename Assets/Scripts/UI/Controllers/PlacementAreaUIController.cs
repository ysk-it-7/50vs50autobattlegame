using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class PlacementAreaUIController
{
    private VisualElement placementAreaElement;
    private List<CharacterData> ownedCharactersProvider;
    private int gridColumns;
    private int gridRows;
    private Vector2 actualCellSize;
    private Vector2 actualPlacementAreaSize;

    public event System.Action<Vector2Int> OnGridCellTappedEvent;
    // public event System.Action<CharacterData, Vector2Int> OnCharacterDroppedOnGridEvent; // ★ CS0067対応: 未使用なので削除

    public PlacementAreaUIController(VisualElement areaElement, List<CharacterData> ownedCharacters, int cols, int rows)
    {
        placementAreaElement = areaElement;
        ownedCharactersProvider = ownedCharacters;
        gridColumns = cols;
        gridRows = rows;

        if (placementAreaElement == null) { Debug.LogError("PlacementAreaController: areaElement is null."); return; }

        placementAreaElement.RegisterCallback<GeometryChangedEvent>(OnPlacementAreaGeometryChanged);
        placementAreaElement.RegisterCallback<PointerDownEvent>(OnPlacementAreaPointerDown);
    }

    private void OnPlacementAreaGeometryChanged(GeometryChangedEvent evt)
    {
        actualPlacementAreaSize = new Vector2(evt.newRect.width, evt.newRect.height);
        if (gridColumns > 0 && gridRows > 0) {
            actualCellSize = new Vector2(actualPlacementAreaSize.x / gridColumns, actualPlacementAreaSize.y / gridRows);
        } else {
            actualCellSize = Vector2.zero;
        }
        // Debug.Log($"PlacementArea UI resized. AreaSize: {actualPlacementAreaSize}, CellSize: {actualCellSize}");
        // サイズ変更時に現在の編成を再描画する必要があれば、イベントでFormationScreenControllerに通知する
    }

    private void OnPlacementAreaPointerDown(PointerDownEvent evt)
    {
        if (evt.button != 0 || placementAreaElement == null || actualCellSize.x <= 0 || actualCellSize.y <= 0) return;
        Vector2 localClickPosition = evt.localPosition; // PointerDownEventのlocalPositionはターゲット要素のローカル座標
        Vector2Int gridPos = GetGridPositionFromLocalPosition(localClickPosition);
        OnGridCellTappedEvent?.Invoke(gridPos);
    }

    public Vector2Int GetGridPositionFromLocalPosition(Vector2 localPosition)
    {
        if (actualCellSize.x <= 0 || actualCellSize.y <= 0) return Vector2Int.one * -1;
        int gridX = Mathf.FloorToInt(localPosition.x / actualCellSize.x);
        int gridY = Mathf.FloorToInt(localPosition.y / actualCellSize.y);
        gridX = Mathf.Clamp(gridX, 0, gridColumns - 1);
        gridY = Mathf.Clamp(gridY, 0, gridRows - 1);
        return new Vector2Int(gridX, gridY);
    }

    public void RedrawFormation(List<PlacedCharacterInfoForSave> formationToDisplay)
    {
        if (placementAreaElement == null) return;
        placementAreaElement.Clear();
        if (formationToDisplay == null) return;

        foreach (var placedInfo in formationToDisplay) {
            CharacterData charData = ownedCharactersProvider.FirstOrDefault(cd => cd.id == placedInfo.characterId);
            if (charData != null) {
                AddSingleCharacterUIToGrid(charData, placedInfo.GetGridPosition());
            }
        }
    }

    public VisualElement AddSingleCharacterUIToGrid(CharacterData charData, Vector2Int gridPos)
    {
        if (placementAreaElement == null || charData == null || charData.icon == null || actualCellSize.x <= 0 || actualCellSize.y <= 0) {
            return null;
        }
        VisualElement charIconWrapper = new VisualElement();
        charIconWrapper.name = "placed_char_" + charData.id + "_" + gridPos.x + "_" + gridPos.y;
        charIconWrapper.AddToClassList("placed-character-icon-wrapper");
        charIconWrapper.userData = new PlacedCharacterInfoForSave(charData.id, gridPos);

        Image charIcon = new Image { image = charData.icon };
        charIconWrapper.Add(charIcon);

        charIconWrapper.style.position = Position.Absolute;
        charIconWrapper.style.left = gridPos.x * actualCellSize.x;
        charIconWrapper.style.top = gridPos.y * actualCellSize.y;
        charIconWrapper.style.width = actualCellSize.x;
        charIconWrapper.style.height = actualCellSize.y;

        placementAreaElement.Add(charIconWrapper);
        return charIconWrapper;
    }

    public void RemoveCharacterUIFromGrid(Vector2Int gridPos)
    {
        if (placementAreaElement == null) return;
        VisualElement elementToRemove = null;
        foreach(var child in placementAreaElement.Children()) {
            if (child.userData is PlacedCharacterInfoForSave pci && pci.GetGridPosition() == gridPos) {
                elementToRemove = child;
                break;
            }
        }
        if (elementToRemove != null) {
            elementToRemove.RemoveFromHierarchy();
        }
    }

    // ★ CS1061エラー対応: このメソッドを追加
    public VisualElement GetPlacementAreaElement()
    {
        return placementAreaElement;
    }

    public void UnregisterCallbacks()
    {
        if (placementAreaElement != null) {
            placementAreaElement.UnregisterCallback<GeometryChangedEvent>(OnPlacementAreaGeometryChanged);
            placementAreaElement.UnregisterCallback<PointerDownEvent>(OnPlacementAreaPointerDown);
        }
    }
}