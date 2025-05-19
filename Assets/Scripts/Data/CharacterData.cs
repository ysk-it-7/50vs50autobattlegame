using UnityEngine;

public class CharacterData
{
    public string id;
    public string displayName = "キャラクター";
    public Texture2D icon;
    public string iconResourcePath; // JSON保存/ロード用にアイコンのリソースパスを保持
    public int hp;
    public int atk;
    public int spd;
    public float attackSpeed;
}