using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer skyBackground;
    [SerializeField]
    private SpriteRenderer grassBackground;
    [SerializeField]
    private float skySpeed = 0.25f, groundSpeed = 0.25f;
    private float skyOffset = 0f, groundOffset = 0f;
    public bool backgroundScroll = true;

    // Update is called once per frame
    void Update()
    {
        if (!backgroundScroll)
            return;
        skyOffset += skySpeed * Time.deltaTime;
        groundOffset += groundSpeed * Time.deltaTime;
        skyBackground.material.mainTextureOffset = new Vector2(skyOffset, 0.0f);
        grassBackground.material.mainTextureOffset = new Vector2(groundOffset, 0.0f);
    }
}
