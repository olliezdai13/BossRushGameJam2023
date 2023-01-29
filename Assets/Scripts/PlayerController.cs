using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;
using System;

/*
 States:
--------------------
    Idle
    Walking
    Jumping
    Rising
    Falling
    Dash
    Attack
    AttackStun
    Hitstun
 */

public class PlayerController : MonoBehaviour, IKnockbackable
{
    private StateMachine fsm;

    // REFERENCES
    // ----------------------------------------------
    private Rigidbody2D _rb;
    private GroundSensor _groundSensor;
    private Animator _animator;

    // PLAYER STATE
    // ----------------------------------------------

    [Header("Movement")]
    [SerializeField] private float _maxSpeedX = 1f;
    [SerializeField] private float _accelerationX = 1000f;
    [SerializeField] private float _drag = 100f;
    [Header("Jumping")]
    [SerializeField] private float _maxFallSpeed = 1f;
    [SerializeField] float _jumpSpeedCap;
    [SerializeField] float _jumpAcceleration;
    [SerializeField] float _jumpTimer;
    [SerializeField] float _jumpDuration;
    [Header("Gravity")]
    [SerializeField] float _gravityRising = 20f;
    [SerializeField] float _gravityStun = 10f;
    [SerializeField] float _gravityFalling = 20f;
    [Header("Dash")]
    [SerializeField] float _dashSpeedHorizontal = 10f;
    [SerializeField] float _dashSpeedUp = 10f;
    [SerializeField] float _dashSpeedDown = 10f;
    [SerializeField] float _dashDuration = .3f;
    [SerializeField] float _dashBoostUp;
    [SerializeField] float _dashBoostDown;
    [SerializeField] float _dashBoostHorizontal;
    [SerializeField] float _dashCooldown;
    [Header("Attack")]
    [SerializeField] GameObject _hitboxLeft;
    [SerializeField] GameObject _hitboxRight;
    [SerializeField] GameObject _hitboxUp;
    [SerializeField] GameObject _hitboxDown;
    [SerializeField] float _attackCooldown;
    [SerializeField] float _attackDuration;
    [SerializeField] float _attackHitboxStart;
    [SerializeField] float _attackHitboxEnd;
    [SerializeField] float _inputBufferAttackDuration;
    [Header("AttackStun")]
    [SerializeField] float _attackStunDuration = .2f;
    [SerializeField] float _attackKnockbackForce = 1f;
    [SerializeField] float _attackStunBoostUp;
    [SerializeField] float _attackStunBoostDown;
    [SerializeField] float _attackStunBoostHorizontal;
    [SerializeField] float _attackKnockbackAerialBoostForce;
    [SerializeField] float _playerHitStunBoostUp;
    [SerializeField] float _playerHitStunBoostDown;
    [SerializeField] float _playerHitStunBoostHorizontal;

    // RUNTIME VALUES
    // ----------------------------------------------
    private float _speedX = 0f;
    private bool _facingRight = true;
    private Vector2 _dashDirection;
    private float _dashTimer;
    private float _timeSinceLastDash;
    private bool _isAttacking;
    private float _attackTimer;
    private float _timeSinceLastAttack;
    private float _inputBufferAttack;
    private Vector2 _attackDirection;
    private float _stunTimer;
    private bool _stunned;
    private Vector2 _knockbackDirection;
    private bool _attackStunFromAttack;
    private float _attackStunFromAttackForce;

    // PUBLICLY ACCESSIBLE VALUES
    public bool FacingRight { get { return _facingRight; } }
    public bool IsAttacking { get { return _isAttacking; } }
    void OnEnable()
    {
        EventManager.StartListening("onHitboxCollision", OnHitboxCollision);
    }

    void OnDisable()
    {
        EventManager.StopListening("onHitboxCollision", OnHitboxCollision);
    }
    void Start()
    {
        fsm = new StateMachine(this);

        _rb = GetComponent<Rigidbody2D>();
        _groundSensor = GetComponentInChildren<GroundSensor>();
        _animator = GetComponent<Animator>();

        // --- IDLE SETUP ---
        fsm.AddState("Idle", new State(
            OnEnterIdle
            ));

        fsm.AddTransition(new Transition(
            "Idle", "Walking", (transition) =>
            {
                return Input.GetAxisRaw("Horizontal") != 0;
            }
            ));

        fsm.AddTransition(new Transition(
            "Idle", "Jumping", (transition) =>
            {
                return Mathf.Abs(Input.GetAxisRaw("Jump")) > Mathf.Epsilon && _groundSensor.IsGrounded;
            }));

        fsm.AddTransition(new Transition(
            "Idle", "Falling", (transition) =>
            {
                return !_groundSensor.IsGrounded;
            }));

        // --- WALKING SETUP ---
        fsm.AddState("Walking", new State(
            OnEnterWalking,
            OnLogicWalking
            ));

        fsm.AddTransition(new Transition(
            "Walking", "Idle", (transition) =>
            {
                return Mathf.Abs(Input.GetAxisRaw("Horizontal")) < Mathf.Epsilon;
            }));

        fsm.AddTransition(new Transition(
            "Walking", "Jumping", (transition) =>
            {
                return Mathf.Abs(Input.GetAxisRaw("Jump")) > Mathf.Epsilon && _groundSensor.IsGrounded;
            }));

        fsm.AddTransition(new Transition(
            "Walking", "Falling", (transition) =>
            {
                return !_groundSensor.IsGrounded;
            }));

        fsm.SetStartState("Idle");

        // --- JUMPING SETUP ---
        fsm.AddState("Jumping", new State(
            OnEnterJumping,
            OnLogicJumping
            ));

        fsm.AddTriggerTransition("OnJumpStop", new Transition("Jumping", "Rising"));

        fsm.AddTransition(new Transition(
           "Jumping", "Falling", (transition) =>
           {
               return _rb.velocity.y <= 0;
            }));

        // ---RISING SETUP---
        fsm.AddState("Rising", new State(
          OnEnterRising,
          OnLogicRising
          ));

        fsm.AddTransition(new Transition(
           "Rising", "Falling", (transition) =>
           {
               return _rb.velocity.y <= 0;
           }));

        // ---FALLING SETUP---
        fsm.AddState("Falling", new State(
          OnEnterFalling,
          OnLogicFalling
          ));

        fsm.AddTransition(new Transition(
            "Falling", "Idle", (transition) =>
            {
                return Mathf.Abs(Input.GetAxisRaw("Horizontal")) < Mathf.Epsilon && _groundSensor.IsGrounded;
            }));

        fsm.AddTransition(new Transition(
            "Falling", "Walking", (transition) =>
            {
                return Mathf.Abs(Input.GetAxisRaw("Horizontal")) > Mathf.Epsilon && _groundSensor.IsGrounded;
            }));

        // ---DASH SETUP---
        fsm.AddState("Dash", new State(
            onExit: (state) =>
            {
                _timeSinceLastDash = 0;
                dashBoostOmni();
            },
            onEnter: (state) => {
                Debug.Log("Entering Dash state");
                _dashDirection = dashDirectionOmni();
                _dashTimer = 0;
                _speedX = 0;
                _rb.velocity = Vector2.zero;
            },
            onLogic: (state) =>
            {
                _dashTimer += Time.deltaTime;
                dashMoveOmni();
            }));

        fsm.AddTransition(new Transition(
            "Dash", "Idle", (transition) =>
            {
                return _groundSensor.IsGrounded && _dashTimer > _dashDuration;
            }));

        fsm.AddTransition(new Transition(
            "Dash", "Rising", (transition) =>
            {
                return !_groundSensor.IsGrounded && _dashTimer > _dashDuration;
            }));

        // Transitions to Dash
        fsm.AddTransition(new Transition(
            "Idle",
            "Dash",
            t => Input.GetAxisRaw("Dash") > Mathf.Epsilon && _timeSinceLastDash >= _dashCooldown));
        fsm.AddTransition(new Transition(
            "Walking",
            "Dash",
            t => Input.GetAxisRaw("Dash") > Mathf.Epsilon && _timeSinceLastDash >= _dashCooldown));
        fsm.AddTransition(new Transition(
            "Jumping",
            "Dash",
            t => Input.GetAxisRaw("Dash") > Mathf.Epsilon && _timeSinceLastDash >= _dashCooldown));
        fsm.AddTransition(new Transition(
            "Rising",
            "Dash",
            t => Input.GetAxisRaw("Dash") > Mathf.Epsilon && _timeSinceLastDash >= _dashCooldown));
        fsm.AddTransition(new Transition(
            "Falling",
            "Dash",
            t => Input.GetAxisRaw("Dash") > Mathf.Epsilon && _timeSinceLastDash >= _dashCooldown));

        fsm.AddState("AttackStun", new State(
            onExit: (state) =>
            {
                _stunTimer = 0;
            },
            onEnter: (state) =>
            {
                Debug.Log("Entering AttackStun state");
                _stunTimer = 0;
                float degreesToDown = Vector2.Angle(_knockbackDirection, Vector2.down);
                if (_rb.velocity.y < 0 && degreesToDown <= 30)
                {
                    _rb.velocity = new Vector2(0, _rb.velocity.y);
                } else
                {
                    _rb.velocity = Vector2.zero;
                }
                if (_attackStunFromAttack)
                {
                    _rb.AddForce(_knockbackDirection * attackBoostDirectionalWeight(), ForceMode2D.Impulse);
                } else
                {
                    _rb.AddForce(_knockbackDirection * playerHitstunBoostDirectionalWeight(_attackStunFromAttackForce), ForceMode2D.Impulse);
                }
                if (!_groundSensor.IsGrounded)
                {
                    _rb.AddForce(Vector2.up * _attackKnockbackAerialBoostForce, ForceMode2D.Impulse);
                }
            },
            onLogic: (state) =>
            {
                _stunTimer += Time.deltaTime;

                // Handle air movement
                //_rb.velocity = _knockbackDirection * _attackKnockbackForce * (_attackStunDuration - _stunTimer);

                // Handle update velocity
                if (_rb.velocity.y > 0)
                {
                    addYVel(-_gravityStun * Time.deltaTime);
                } else
                {
                    addYVel(-_gravityStun * Time.deltaTime);
                }
            }));

        fsm.AddTransition(new Transition(
            "AttackStun", "Idle", (transition) =>
            {
                return _groundSensor.IsGrounded && _stunTimer > _attackStunDuration;
            }));

        fsm.AddTransition(new Transition(
            "AttackStun", "Rising", (transition) =>
            {
                return !_groundSensor.IsGrounded && _stunTimer > _attackStunDuration;
            }));

        fsm.AddTriggerTransitionFromAny("OnAttack", new Transition("", "AttackStun"));

        fsm.Init();
    }

    void Update()
    {
        fsm.OnLogic();

        // dash timing
        _timeSinceLastDash += Time.deltaTime;

        // attack handling
        _timeSinceLastAttack += Time.deltaTime;

        if (Input.GetAxisRaw("Fire1") != 0)
        {
            _inputBufferAttack = _inputBufferAttackDuration;
        }
        if (_inputBufferAttack > 0)
        {
            _inputBufferAttack -= Time.deltaTime;
        } else
        {
            _inputBufferAttack = 0;
        }

        if (fsm.ActiveStateName == "Idle" || fsm.ActiveStateName == "Walking" || fsm.ActiveStateName == "Jumping" || fsm.ActiveStateName == "Rising" || fsm.ActiveStateName == "Falling")
        {
            if (_inputBufferAttack > 0 && !_isAttacking && _timeSinceLastAttack >= _attackCooldown)
            {
                _isAttacking = true;
                _attackTimer = 0;
                _attackDirection = inputDirectionQuad(); // TODO maybe change where the direction is determined
                enableHitbox(_attackDirection);
            }
        }
        if (_isAttacking)
        {
            _attackTimer += Time.deltaTime; 
            if (_attackTimer >= _attackDuration)
            {
                _isAttacking = false;
                _attackTimer = 0;
                _timeSinceLastAttack = 0;
            }
            if (_attackTimer >= _attackHitboxEnd)
            {
                disableHitboxes();
            }
        }
    }

    // --- IDLE ---
    private void OnEnterIdle(State<string, string> state)
    {
        Debug.Log("Entering Idle state");
        _speedX = 0;
        _rb.velocity = Vector2.zero;
        _animator.SetBool("moving", false);
    }

    // --- WALKING ---
    private void OnEnterWalking(State<string, string> state)
    {
        Debug.Log("Entering Walking state");
        _animator.SetBool("moving", true);
    }

    private void OnLogicWalking(State<string, string> obj)
    {
        // Handle collect input
        float inputRawH = Input.GetAxisRaw("Horizontal");
        float moveInputClamped = Mathf.Clamp(inputRawH, -1, 1);
        float inputH = moveInputClamped;

        _speedX += _accelerationX * inputH * Time.deltaTime;
        _speedX = Mathf.Clamp(_speedX, -_maxSpeedX, _maxSpeedX);

        // Handle update velocity
        _rb.velocity = new Vector2(_speedX, 0);
        faceInput();
    }

    private void OnExitWalking(State<string, string> state)
    {
        _animator.SetBool("moving", false);
    }

    // --- JUMPING ---
    private void OnEnterJumping(State<string, string> state)
    {
        Debug.Log("Entering Jumping state");
        disableHitboxes();
        _jumpTimer = 0;
    }

    private void OnLogicJumping(State<string, string> obj)
    {
        // Handle air movement
        float inputRawH = Input.GetAxisRaw("Horizontal");
        float moveInputClamped = Mathf.Clamp(inputRawH, -1, 1);
        float inputH = moveInputClamped;
        _speedX += _accelerationX * inputH * Time.deltaTime;
        _speedX = Mathf.Clamp(_speedX, -_maxSpeedX, _maxSpeedX);
        _rb.velocity = new Vector2(_speedX, _rb.velocity.y);
        drag();

        // Handle update velocity
        addYVel(_jumpAcceleration * Time.deltaTime);
        setYVel(Mathf.Clamp(_rb.velocity.y, 0, _jumpSpeedCap));
        _jumpTimer += Time.deltaTime;
        if (Mathf.Abs(Input.GetAxisRaw("Jump")) < Mathf.Epsilon)
        {
            stopJump(true);
        }
        if (_jumpTimer >= _jumpDuration)
        {
            stopJump(false);
        }
        faceInput();
    }

    // --- RISING ---
    private void OnEnterRising(State<string, string> state)
    {
        Debug.Log("Entering Rising state");
        disableHitboxes();
        _jumpTimer = 0;
    }

    private void OnLogicRising(State<string, string> obj)
    {
        // Handle air movement
        float inputRawH = Input.GetAxisRaw("Horizontal");
        float moveInputClamped = Mathf.Clamp(inputRawH, -1, 1);
        float inputH = moveInputClamped;
        _speedX += _accelerationX * inputH * Time.deltaTime;
        _speedX = Mathf.Clamp(_speedX, -_maxSpeedX, _maxSpeedX);
        _rb.velocity = new Vector2(_speedX, _rb.velocity.y);
        drag();

        // Handle update velocity
        addYVel(-_gravityRising * Time.deltaTime);
        faceInput();
    }

    // --- FALLING ---
    private void OnEnterFalling(State<string, string> state)
    {
        Debug.Log("Entering Falling state");
        disableHitboxes();
        _jumpTimer = 0;
    }

    private void OnLogicFalling(State<string, string> obj)
    {
        // Handle air movement
        float inputRawH = Input.GetAxisRaw("Horizontal");
        float moveInputClamped = Mathf.Clamp(inputRawH, -1, 1);
        float inputH = moveInputClamped;
        _speedX += _accelerationX * inputH * Time.deltaTime;
        _speedX = Mathf.Clamp(_speedX, -_maxSpeedX, _maxSpeedX);
        _rb.velocity = new Vector2(_speedX, Mathf.Clamp(_rb.velocity.y, -_maxFallSpeed, _maxFallSpeed));
        drag();

        // Handle update velocity
        addYVel(-_gravityFalling * Time.deltaTime);
        faceInput();
    }

    // Helpers
    void setYVel(float y)
    {
        _rb.velocity = new Vector3(_rb.velocity.x, y);
    }
    void addYVel(float y)
    {
        _rb.velocity = new Vector3(_rb.velocity.x, _rb.velocity.y + y);
    }
    void stopJump(bool sharp)
    {
        fsm.Trigger("OnJumpStop");
        if (sharp)
        {
            setYVel(_rb.velocity.y / 1.3f);
        }
    }
    void faceInput()
    {
        if (!_isAttacking)
        {
            // Handle change facing direction
            if (Input.GetAxisRaw("Horizontal") > Mathf.Epsilon)
            {
                _facingRight = true;
            }
            else if (Input.GetAxisRaw("Horizontal") < -Mathf.Epsilon)
            {
                _facingRight = false;
            }
        }
    }

    void drag()
    {
        if (_speedX > 0)
        {
            _speedX -= _drag * Time.deltaTime;
            if (_speedX < 0)
            {
                _speedX = 0;
            }
        }
        if (_speedX < 0)
        {
            _speedX += _drag * Time.deltaTime;
            if (_speedX > 0)
            {
                _speedX = 0;
            }
        }
    }

    Vector2 inputDirectionQuad()
    {
        return Mathf.Abs(Input.GetAxisRaw("Vertical")) > Mathf.Epsilon ?
                        new Vector2(0, Input.GetAxisRaw("Vertical")).normalized :
                        Mathf.Abs(Input.GetAxisRaw("Horizontal")) > Mathf.Epsilon ?
                            new Vector2(Input.GetAxisRaw("Horizontal"), 0).normalized :
                            _facingRight ?
                                new Vector2(1, 0) : new Vector2(-1, 0);
    }

    void dashMoveQuad()
    {
        if (_dashDirection.y > 0)
        {
            _rb.velocity = _dashDirection * _dashSpeedUp;
        }
        else if (_dashDirection.y < 0)
        {
            _rb.velocity = _dashDirection * _dashSpeedDown;
        }
        else if (_dashDirection.x < 0)
        {
            _rb.velocity = _dashDirection * _dashSpeedHorizontal;
        }
        else if (_dashDirection.x > 0)
        {
            _rb.velocity = _dashDirection * _dashSpeedHorizontal;
        }
    }

    void dashBoostQuad()
    {
        if (_dashDirection.y > 0)
        {
            _rb.velocity = new Vector2(0, _dashBoostUp);
        }
        else if (_dashDirection.y < 0)
        {
            _rb.velocity = new Vector2(0, -_dashBoostUp);
        }
        else if (_dashDirection.x < 0)
        {
            _rb.velocity = new Vector2(-_dashBoostHorizontal, 0);
            _speedX = -_jumpSpeedCap;
        }
        else if (_dashDirection.x > 0)
        {
            _rb.velocity = new Vector2(_dashBoostHorizontal, 0);
            _speedX = _jumpSpeedCap;
        }
        else
        {
            _rb.velocity = Vector2.zero;
        }
    }

    Vector2 dashDirectionOmni()
    {
        Vector2 direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (direction.magnitude == 0)
        {
            direction = _facingRight ? new Vector2(1, 0) : new Vector2(-1, 0);
        }
        return direction;
    }

    void dashMoveOmni()
    {
        if (Vector2.Angle(_dashDirection, Vector2.up) <= 45) {
            // Dashing generally upwards
            _rb.velocity = _dashDirection * _dashSpeedUp;
        }
        else if (Vector2.Angle(_dashDirection, Vector2.right) < 45)
        {
            // Dashing generally rightwards
            _rb.velocity = _dashDirection * _dashSpeedHorizontal;
        }
        else if (Vector2.Angle(_dashDirection, Vector2.left) < 45)
        {
            // Dashing generally leftwards
            _rb.velocity = _dashDirection * _dashSpeedHorizontal;
        }
        else if (Vector2.Angle(_dashDirection, Vector2.down) <= 45)
        {
            // Dashing generally downwards
            _rb.velocity = _dashDirection * _dashSpeedDown;
        }
    }

    void dashBoostOmni()
    {
        float degreesToUp = Vector2.Angle(_dashDirection, Vector2.up);
        float degreesToRight = Vector2.Angle(_dashDirection, Vector2.right);
        float degreesToLeft = Vector2.Angle(_dashDirection, Vector2.left);
        float degreesToDown = Vector2.Angle(_dashDirection, Vector2.down);
        float weightedBoostUp = degreesToUp <= 90 ? _dashBoostUp * ((90 - degreesToUp) / 90) : 0;
        float weightedBoostRight = degreesToRight <= 90 ? _dashBoostHorizontal * ((90 - degreesToRight) / 90) : 0;
        float weightedBoostLeft = degreesToLeft <= 90 ? _dashBoostHorizontal * ((90 - degreesToLeft) / 90) : 0;
        float weightedBoostDown = degreesToDown <= 90 ? _dashBoostDown * ((90 - degreesToDown) / 90) : 0;
        Vector2 dashBoost = _dashDirection * (weightedBoostDown + weightedBoostLeft + weightedBoostRight + weightedBoostUp);
        _rb.velocity = dashBoost;
        _speedX = dashBoost.x;
    }

    void enableHitbox(Vector2 direction)
    {
        if (direction.x > 0)
        {
            _hitboxRight.SetActive(true);
        }
        if (direction.x < 0)
        {
            _hitboxLeft.SetActive(true);
        }
        if (direction.y > 0)
        {
            _hitboxUp.SetActive(true);
        }
        if (direction.y < 0)
        {
            _hitboxDown.SetActive(true);
        }
    }

    void disableHitboxes()
    {
        _hitboxRight.SetActive(false);
        _hitboxLeft.SetActive(false);
        _hitboxUp.SetActive(false);
        _hitboxDown.SetActive(false);
    }

    // Handle behavior when player's attack hits something
    // hitPosition: Vector2
    // initiator: Initiator
    public void OnHitboxCollision(Dictionary<string, object> data)
    {
        Vector2 hitPos = (Vector2)data["hitPosition"];
        Initiator initiator = (Initiator)data["initiator"];
        if (initiator == Initiator.PLAYER)
        {
            _knockbackDirection = -(hitPos - new Vector2(transform.position.x, transform.position.y)).normalized;
            _attackStunFromAttack = true;
            fsm.Trigger("OnAttack");
        }
    }

    float attackBoostDirectionalWeight()
    {
        float degreesToUp = Vector2.Angle(_knockbackDirection, Vector2.up);
        float degreesToRight = Vector2.Angle(_knockbackDirection, Vector2.right);
        float degreesToLeft = Vector2.Angle(_knockbackDirection, Vector2.left);
        float degreesToDown = Vector2.Angle(_knockbackDirection, Vector2.down);
        float weightedBoostUp = degreesToUp <= 90 ? _attackStunBoostUp * ((90 - degreesToUp) / 90) : 0;
        float weightedBoostRight = degreesToRight <= 90 ? _attackStunBoostHorizontal * ((90 - degreesToRight) / 90) : 0;
        float weightedBoostLeft = degreesToLeft <= 90 ? _attackStunBoostHorizontal * ((90 - degreesToLeft) / 90) : 0;
        float weightedBoostDown = degreesToDown <= 90 ? _attackStunBoostDown * ((90 - degreesToDown) / 90) : 0;
        return (weightedBoostDown + weightedBoostLeft + weightedBoostRight + weightedBoostUp);
    }
    float playerHitstunBoostDirectionalWeight(float forceMultiplier)
    {
        float degreesToUp = Vector2.Angle(_knockbackDirection, Vector2.up);
        float degreesToRight = Vector2.Angle(_knockbackDirection, Vector2.right);
        float degreesToLeft = Vector2.Angle(_knockbackDirection, Vector2.left);
        float degreesToDown = Vector2.Angle(_knockbackDirection, Vector2.down);
        float weightedBoostUp = degreesToUp <= 90 ? _playerHitStunBoostUp * ((90 - degreesToUp) / 90) * forceMultiplier : 0;
        float weightedBoostRight = degreesToRight <= 90 ? _playerHitStunBoostHorizontal * ((90 - degreesToRight) / 90) * forceMultiplier : 0;
        float weightedBoostLeft = degreesToLeft <= 90 ? _playerHitStunBoostHorizontal * ((90 - degreesToLeft) / 90) * forceMultiplier : 0;
        float weightedBoostDown = degreesToDown <= 90 ? _playerHitStunBoostDown * ((90 - degreesToDown) / 90) * forceMultiplier : 0;
        return (weightedBoostDown + weightedBoostLeft + weightedBoostRight + weightedBoostUp);
    }

    // Handle behavior when player gets hit with knockback
    public void Knockback(Vector2 _direction, float force, Vector2 hitPos, Transform initiator)
    {
        _knockbackDirection = -((Vector2)initiator.position - new Vector2(transform.position.x, transform.position.y)).normalized;
        _attackStunFromAttack = false;
        _attackStunFromAttackForce = force;
        fsm.Trigger("OnAttack");
    }
}