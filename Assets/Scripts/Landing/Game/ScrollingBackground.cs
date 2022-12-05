using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    private SpriteRenderer skyBackground;
    private SpriteRenderer grassBackground;
    float offset = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        skyBackground = GetComponent<SpriteRenderer>();
        grassBackground = gameObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        offset += 0.25f * Time.deltaTime;
        skyBackground.material.mainTextureOffset = new Vector2(offset, 0.0f);
        grassBackground.material.mainTextureOffset = new Vector2(offset, 0.0f);
    }
}
