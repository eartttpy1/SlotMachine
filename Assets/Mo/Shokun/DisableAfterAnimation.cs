using UnityEngine;

public class DisableAfterAnimation : MonoBehaviour
{
    [Tooltip("ระยะเวลาเป็นวินาทีก่อนที่จะปิด GameObject นี้ (ตั้งให้เท่ากับหรือมากกว่าความยาวแอนิเมชันเล็กน้อย)")]
    public float delay = 1.0f;

    private void OnEnable()
    {
        // ยกเลิก Invoke เดิม (ถ้ามี) เพื่อป้องกันการเรียกซ้ำซ้อน
        CancelInvoke("DisableMe");
        // สั่งให้เรียกฟังก์ชัน DisableMe หลังจากผ่านไป 'delay' วินาที
        Invoke("DisableMe", delay);
    }

    private void DisableMe()
    {
        gameObject.SetActive(false);
    }
}
