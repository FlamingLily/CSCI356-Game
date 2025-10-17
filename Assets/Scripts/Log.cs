using System.Collections;
using UnityEngine;

public class Log : MonoBehaviour   // ? ????? Log.cs ???
{
    public Rigidbody logPrefab;           // ???Prefab?Rigidbody???
    public Transform[] spawnPoints;       // ?????Empty?Transform?
    public float[] pushes;                // ?????????

    [Header("Speed Tuning")]
    public float interval = 0.7f;         // ????
    public float gravityMultiplier = 1.8f;// ????????
    public float initialDownSpeed = 6f;   // ?????
    public float lifetime = 8f;           // ???????

    private int index;

    private void Start()
    {
        StartCoroutine(Loop());
    }

    private IEnumerator Loop()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawnPoints set.");
            yield break;
        }

        while (true)
        {
            // 1) ????
            var sp = spawnPoints[index % spawnPoints.Length];

            // 2) ?????(Y)?????(forward)??????
            var rot = Quaternion.FromToRotation(Vector3.up, sp.forward);

            // 3) ??
            var rb = Instantiate(logPrefab, sp.position, rot);

            // 4) ?????
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;

            // 5) ????????
            float power = (pushes != null && pushes.Length > 0)
                ? pushes[index % pushes.Length]
                : 3f;
            rb.AddForce(sp.forward * power, ForceMode.Impulse);

            // 6) ??????
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
            rb.linearVelocity += Vector3.down * initialDownSpeed;

            // 7) ???????
            Destroy(rb.gameObject, lifetime);

            index++;
            yield return new WaitForSeconds(interval);
        }
    }
}
