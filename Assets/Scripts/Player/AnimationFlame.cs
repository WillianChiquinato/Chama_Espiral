using UnityEngine;

public class AnimationFlame : MonoBehaviour
{
    public float amplitude = 0.1f;
    public float frequency = 1f;

    private Vector3 startLocalPosition;

    void Start()
    {
        startLocalPosition = transform.localPosition;
    }

    void Update()
    {
        float yOffset = amplitude * Mathf.Sin(Time.time * frequency * 2f * Mathf.PI);
        transform.localPosition = startLocalPosition + new Vector3(0, yOffset, 0);
    }
}
