using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] PlayerMovement pm;
    [SerializeField] Animator anim;
    private bool isWalking;
    private Vector2 moveValueV2;

    private void Update()
    {
        UpdatingValueS();
        ProcessAnimator();
    }

    private void UpdatingValueS()
    {
        // Selalu update nilai movevalue untuk value yang ada di animator
        if(pm.MoveState && pm.input != Vector2.zero)
        {
            moveValueV2 = pm.input;
        }
        isWalking = pm.isWalking;
    }

    private void ProcessAnimator()
    {
        anim.SetBool("IsWalking", isWalking);

        if (!isWalking && !(moveValueV2 != Vector2.zero)) return;
        TransformFlippingProcess();
    }

    private void TransformFlippingProcess()
    {
        if (moveValueV2.x > 0.1f)
        {
            AnimatorValueUpdate(0f);
        }
        else if (moveValueV2.x < -0.1f)
        {
            AnimatorValueUpdate(180f);
        }
        else
        {
            // Jika berjalan lurus ke atas/bawah, pertahankan skala terakhir
            AnimatorValueUpdate(transform.localScale.x < 0 ? 180f : 0f);
        }
    }

    public void ForceLookDirection(SpawnDirection direction)
    {
        if (direction == SpawnDirection.NONE) return;

        // Tentukan nilai Vector2 berdasarkan pilihan Dropdown
        switch (direction)
        {
            case SpawnDirection.UP:    moveValueV2 = new Vector2(0, 1); break;
            case SpawnDirection.DOWN:  moveValueV2 = new Vector2(0, -1); break;
            case SpawnDirection.LEFT:  moveValueV2 = new Vector2(-1, 0); break;
            case SpawnDirection.RIGHT: moveValueV2 = new Vector2(1, 0); break;
        }

        // Tentukan status skala berdasarkan nilai X (Kiri = 180f untuk logika ini)
        float rotationValue = (moveValueV2.x < 0) ? 180f : 0f;

        // Jalankan update animator dan skala secara instan
        AnimatorValueUpdate(rotationValue);
        
        // Paksa agar langsung idle setelah perubahan arah
        isWalking = false;
        anim.SetBool("IsWalking", false);
    }

    private void AnimatorValueUpdate(float dir)
    {
        // SOLUSI FINAL: Menggunakan Skala Negatif (X: -1) agar Shader Kustom ikut terbalik
        // Namun sisi depan (Normal) tetap menghadap kamera sehingga pencahayaan tetap terang.
        if (dir == 180f) // KIRI
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else // KANAN (0f)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }

        // Sebagai Trigger untuk transisi
        isWalking = true;

        // Nilai dari input animator selalu terupdate dan WAJIB diupdate untuk animasi         
        anim.SetFloat("Xinput", moveValueV2.x);
        anim.SetFloat("Yinput", moveValueV2.y);
    }
}
