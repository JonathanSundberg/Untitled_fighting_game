using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [HideInInspector]
    public Vector2 movement_dir;
    float MoveSpeed = 10f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void Move()
    {

        Vector2 movement = new Vector2(movement_dir.x, movement_dir.y) * MoveSpeed * Time.deltaTime;
        transform.Translate(movement);

    }
}
