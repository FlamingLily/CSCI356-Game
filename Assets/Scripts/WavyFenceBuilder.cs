using UnityEngine;

public class WavyFenceBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject segmentPrefab;      
    public GameObject postPrefab;        

    [Header("Shape")]
    public int count = 60;                // 片の数
    public float length = 60f;            // 全体の長さ（親のローカルX方向）
    public float amplitude = 1.5f;        // くねりの高さ
    public float wavelength = 8f;         // くねりの波長（長さ）

    [Header("Placement")]
    public float yOffset = 0.5f;          // 地面からの高さ
    public bool autoScaleToSpacing = true;// 片を隙間なく横に伸縮
    public int postEvery = 0;             // 何片ごとに柱を置くか（0で置かない）

    [ContextMenu("Build Fence")]
    public void Build()
    {
        if (segmentPrefab == null) { Debug.LogWarning("segmentPrefab not set."); return; }


        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        float dx = length / Mathf.Max(1, (count - 1));
        float k = 2f * Mathf.PI / Mathf.Max(0.001f, wavelength);

        for (int i = 0; i < count; i++)
        {
            float x = i * dx;
            float z = Mathf.Sin(k * x) * amplitude;

            // 親のローカル→ワールド
            Vector3 localPos = new Vector3(x, yOffset, z);
            Vector3 worldPos = transform.TransformPoint(localPos);

            // 接線（向き）：dz/dx = k*A*cos(kx)
            Vector3 localTangent = new Vector3(1f, 0f, k * amplitude * Mathf.Cos(k * x));
            Quaternion localRot = Quaternion.LookRotation(localTangent, Vector3.up);
            Quaternion worldRot = transform.rotation * localRot;

            var seg = Instantiate(segmentPrefab, worldPos, worldRot, transform);

            if (autoScaleToSpacing)
            {
                // ローカルXを片間隔に合わせる（厚み/Yはそのまま）
                var s = seg.transform.localScale;
                seg.transform.localScale = new Vector3(dx, s.y, s.z);
            }

            if (postPrefab != null && postEvery > 0 && i % postEvery == 0)
            {
                Instantiate(postPrefab, worldPos, transform.rotation, transform);
            }
        }
    }

    // 置いた瞬間にも作る（Play中じゃなくてもOK）
    private void OnValidate()
    {
        // エディタで値を変えたら自動で再生成
        if (Application.isEditor && !Application.isPlaying)
        {
            // segmentPrefab が入っていればプレビュー生成
            if (segmentPrefab != null) Build();
        }
    }
}
