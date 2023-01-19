using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMove : MonoBehaviour
{
    public float moveSpeed;
    public float jumpVelocity;
    public bool facingRight;
    private Rigidbody2D _rigidbody;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool inputJump = Input.GetAxis("Jump") != 0;
        float inputX = Input.GetAxis("Horizontal");
        if (inputX > 0)
        {
            facingRight = true;
        } else if (inputX < 0)
        {
            facingRight = false;
        }
        _rigidbody.velocity = new Vector2(inputX * moveSpeed, _rigidbody.velocity.y);
        if (inputJump)
        {
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, jumpVelocity);
        }
    }
}
