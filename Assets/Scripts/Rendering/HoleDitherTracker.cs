using UnityEngine;

public class HoleDitherTracker : MonoBehaviour
{
    public Transform playerTransform;
    public float holeRadius = 1.5f;

    private Material mat;
    private bool isPlayerBehind = false;
    
    private static readonly int PlayerPosID = Shader.PropertyToID("_PlayerPos");
    private static readonly int RadiusID = Shader.PropertyToID("_Radius");

    void Start()
    {
        mat = GetComponent<Renderer>().material;
        mat.SetFloat(RadiusID, 0f);

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && mat != null)
        {
            isPlayerBehind = true;
            mat.SetFloat(RadiusID, holeRadius);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && mat != null)
        {
            isPlayerBehind = false;
            mat.SetFloat(RadiusID, 0f); 
        }
    }

    void Update()
    {
        if (isPlayerBehind && playerTransform != null && mat != null)
        {
            Vector3 fakePlayerPos = playerTransform.position;
            fakePlayerPos.z = transform.position.z; 

            mat.SetVector(PlayerPosID, fakePlayerPos);
        }
    }
}