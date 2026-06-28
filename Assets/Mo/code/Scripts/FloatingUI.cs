using UnityEngine;
using DG.Tweening;

public class FloatingUI : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatDistance = 15f; // ระยะทางในการลอยขึ้นลง
    [SerializeField] private float duration = 2.0f;      // วินาทีต่อการลอยครบรอบหนึ่งครั้ง

    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Vector3 originalLocalPos;
    private bool isUI = false;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            isUI = true;
            originalPosition = rectTransform.anchoredPosition;
            
            // ขยับขึ้นลงแบบ Yoyo (ไปและกลับ) ตลอดเวลาอย่างสมูท
            rectTransform.DOAnchorPosY(originalPosition.y + floatDistance, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            isUI = false;
            originalLocalPos = transform.localPosition;
            
            // สำหรับ Object ทั่วไปที่ไม่ได้อยู่ใน Canvas
            transform.DOLocalMoveY(originalLocalPos.y + floatDistance, duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnDestroy()
    {
        // สั่ง Kill Tween ของ Object นี้เมื่อถูกทำลาย เพื่อป้องกัน Memory Leak
        transform.DOKill();
        if (rectTransform != null)
        {
            rectTransform.DOKill();
        }
    }
}
