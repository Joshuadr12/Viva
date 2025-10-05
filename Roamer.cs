using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Roamer : MonoBehaviour
{
    [SerializeField] float velocity = 5;
    [SerializeField, Tooltip("The number of times the roamer moves before changing direction")] int movements = 4;

    int move = 0;
    float direction;
    Rigidbody2D rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        direction = Mathf.Sign(transform.localScale.x);
        rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Move()
    {
        transform.localScale = new Vector3(direction, 1, 1);
        rigidbody.velocity = Vector3.right * velocity * direction;
        move++;
        while (move >= movements)
        {
            direction *= -1;
            move -= movements;
        }
    }
}