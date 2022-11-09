using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Column : MonoBehaviour
{
    //Variables
    public GameObject Top;
    public GameObject Bottom;

    //Component
    BoxCollider2D boxCollider2D;
    private float changePerDifficulty = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    public void Setup(int value)
    {
        //Get the Position of the top and bottom columns
        Vector3 topTransform = Top.transform.position;
        Vector3 botTransform = Bottom.transform.position;

        //Change the position based on the value
        topTransform.y += value * changePerDifficulty;
        botTransform.y -= value * changePerDifficulty;

        //Save the transformed position to the game object
        Top.transform.position = topTransform;
        Bottom.transform.position = botTransform;

        //Get the collider size
        Vector2 colliderSize = boxCollider2D.size;
        colliderSize.y = topTransform.y - botTransform.y;
        boxCollider2D.size = colliderSize / 2.0f;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Is it a bird ?
        if(collision.GetComponent<GamePlayer>() != null)
        {
            GameController.instance.Scored(GameController.instance.columnScore);
            collision.GetComponent<GamePlayer>().PlaySound("Point");
        }
    }
}
