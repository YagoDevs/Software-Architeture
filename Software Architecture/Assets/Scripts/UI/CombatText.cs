/*
This script is used to animate a single combat text (move up + fade out) and destroy it after a short lifetime.
*/

using TMPro;
using UnityEngine;

public class CombatText : MonoBehaviour
{
    public float lifetime = 0.8f;
    public Vector3 worldVelocity = new Vector3(0, 1.0f, 0);
    public float fadeStart = 0.35f;

    TextMeshProUGUI tmp;
    float t;
    Color startColor;

    void Awake()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        if (tmp != null) startColor = tmp.color;
    }

    void Update()
    {
        t += Time.deltaTime;
        transform.position += worldVelocity * Time.deltaTime;

        if (tmp != null && t >= fadeStart)
        {
            float k = Mathf.InverseLerp(fadeStart, lifetime, t);
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, k);
            tmp.color = c;
        }

        if (t >= lifetime)
            Destroy(gameObject);
    }
}

