using System.Collections;
using UnityEngine;

public class LogDropper : MonoBehaviour
{
    public Rigidbody logPrefab;          
    public Transform[] spawnPoints;     
    public float[] pushes;               
    public float interval = 2f;          

    private int index;

    private void Start()
    {
        StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        while (true)
        {
            var sp = spawnPoints[index % spawnPoints.Length];
            var rb = Instantiate(logPrefab, sp.position, sp.rotation);

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            float power = pushes.Length > 0 ? pushes[index % pushes.Length] : 3f;
            rb.AddForce(sp.forward * power, ForceMode.Impulse);

            index++;
            yield return new WaitForSeconds(interval);
        }
    }
}

