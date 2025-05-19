// Scripts/HealthBar.cs
using UnityEngine;
using UnityEngine.UI; // Sliderを使うため

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Vector3 offset; // キャラクターからの相対位置

    private Transform targetTransform; // HPバーが追従するキャラクターのTransform

    public void Initialize(Transform characterTransform)
    {
        targetTransform = characterTransform;
        transform.SetParent(characterTransform); // キャラクターの子オブジェクトにする
        transform.localPosition = offset; // 初期位置を設定
        transform.localRotation = Quaternion.identity; // 回転をリセット
    }

    public void UpdateHealth(int currentHp, int maxHp)
    {
        if (maxHp > 0)
        {
            slider.value = (float)currentHp / maxHp;
        }
        else
        {
            slider.value = 0;
        }
    }

    // World Space Canvasの場合、キャラクターに追従させるためにLateUpdateは不要になることが多い
    // もしScreen Space CanvasでHPバーをキャラクター位置に表示したい場合はLateUpdateで座標変換が必要
    /*
    void LateUpdate()
    {
        if (targetTransform != null)
        {
            // キャラクターのワールド座標をスクリーン座標に変換し、UI要素の位置を更新
            // Vector3 screenPos = Camera.main.WorldToScreenPoint(targetTransform.position + offset);
            // transform.position = screenPos;
            // ただし、World Space Canvasをキャラクターの子にする方が簡単
        }
    }
    */
}