using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class CharacterListUIController
{
    private VisualElement characterListContainer;
    private VisualTreeAsset characterListItemAsset;
    private List<CharacterData> ownedCharacters;

    public event System.Action<CharacterData, VisualElement> OnCharacterListItemClickedEvent; // キャラクター選択イベント
    public event System.Action<PointerDownEvent, CharacterData, VisualElement> OnListItemPointerDownEvent; // ドラッグ開始用

    private VisualElement selectedListItemElement = null; // 現在選択中のリストアイテム

    public CharacterListUIController(VisualElement listContainer, VisualTreeAsset itemAsset, List<CharacterData> characters)
    {
        characterListContainer = listContainer;
        characterListItemAsset = itemAsset;
        ownedCharacters = characters;

        if (characterListContainer == null) Debug.LogError("CharListController: listContainer is null.");
        if (characterListItemAsset == null) Debug.LogError("CharListController: itemAsset is null.");
        if (ownedCharacters == null) Debug.LogError("CharListController: characters list is null.");
    }

    public void PopulateList()
    {
        if (characterListContainer == null || characterListItemAsset == null || ownedCharacters == null) return;
        characterListContainer.Clear();

        foreach (var charData in ownedCharacters) {
            if (charData == null) continue;
            VisualElement listItem = characterListItemAsset.Instantiate();

            var iconElement = listItem.Q<Image>("character-icon");
            if (iconElement != null) iconElement.image = charData.icon;

            listItem.Q<Label>("hp-value").text = charData.hp.ToString();
            listItem.Q<Label>("atk-value").text = charData.atk.ToString();
            listItem.Q<Label>("spd-value").text = charData.spd.ToString();
            listItem.Q<Label>("as-value").text = charData.attackSpeed.ToString("F1");

            listItem.RegisterCallback<ClickEvent>(evt => {
                HandleItemClick(charData, listItem);
                OnCharacterListItemClickedEvent?.Invoke(charData, listItem);
            });
            listItem.RegisterCallback<PointerDownEvent>(evt => {
                OnListItemPointerDownEvent?.Invoke(evt, charData, listItem);
            });

            characterListContainer.Add(listItem);
        }
    }

    private void HandleItemClick(CharacterData data, VisualElement listItemElement)
    {
        if (selectedListItemElement != null && selectedListItemElement != listItemElement) {
            selectedListItemElement.RemoveFromClassList("selected-character-list-item");
        }
        selectedListItemElement = listItemElement;
        if (selectedListItemElement != null) {
            selectedListItemElement.AddToClassList("selected-character-list-item");
        }
    }

    public void DeselectCurrentListItem()
    {
        if (selectedListItemElement != null) {
            selectedListItemElement.RemoveFromClassList("selected-character-list-item");
        }
        selectedListItemElement = null;
    }
}