using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;
using Cinemachine;


//Sample rat king state list
//IDLE => 3 seconds
//DASH 
//DASH
//DASH
//DASH
//IDLE => 3 seconds
//THROW
//IDLE => 4 seconds
//JUMP => player
//SPIN
//IDLE => 1 second
//JUMP => player
//SPIN
//IDLE => 1 second
//JUMP => spawn
enum ActionSubstate
{
    Start, Action, End
}
enum JumpSubstate
{
    PreJump, Jump, Hover, Fall, Ground
}

public class RatBehavior : MonoBehaviour
{
    private StateMachine fsm;

    public List<RatAction> ratActions;

    public bool doLogging = true;

    // PUBLIC CONFIGS
    public bool _isPreFightIdle = true;
    [Header("Dash")]
    public float _dashSpeed = 1f;
    public float _dashStartTime = .5f;
    public float _dashEndTime = .25f;
    [Header("Throw")]
    public int _throwCount = 3;
    public float _throwStartTime = .5f;
    public float _throwEndTime = .25f;
    public GameObject projectilePrefab;
    public Transform _projectileSpawnRight;
    public Transform _projectileSpawnLeft;
    [Header("Jump")]
    public float _jumpSpeed = 1;
    public float _jumpHoverTime = 1;
    public float _jumpFallSpeed = 1;
    public float _jumpGroundTime = 1;
    public float _jumpWindupTime = 1;
    public GameObject _shadowObject;
    [Header("Waypoints")]
    public Transform _waypointGroundLevel;
    public Transform _waypointAirLevel;
    public Transform _waypointDashLeft;
    public Transform _waypointDashRight;
    [Header("Spin")]
    public float _spinDuration = 2f;
    public float _spinStartTime = .25f;
    public float _spinEndTime = .25f;
    public GameObject _spinHitbox;
    public ParticleSystem _spinParticleSystem;
    public CinemachineImpulseSource _spinImpulseSource;

    // RUNTIME VARS
    private int _actionIndex;
    private float _actionDuration;
    private Transform _actionTarget;
    private ActionSubstate _actionSubstate;
    private float _tempTimer1;
    private int _tempCounter1;
    private JumpSubstate _jumpSubstate;
    private Vector2 _moveTarget;
    private Coroutine throw1;
    private Coroutine throw2;
    private Coroutine throw3;
    private Coroutine throw4;

    // REFERENCES
    // ----------------------------------------------
    private Animator _animator;
    private CinemachineImpulseSource _impulseSource;
    private SpriteRenderer _renderer;
    private Transform _playerTransform;

    void Start()
    {
        fsm = new StateMachine(this);
        //_rb = GetComponent<Rigidbody2D>();
        _animator = GetComponentInChildren<Animator>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        if (_spinHitbox)
        {
            _spinHitbox.SetActive(false);
        }
        _shadowObject.SetActive(false);

        // --- PreFightIdleState SETUP ---
        fsm.AddState("PreFightIdleState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering PreFightIdleState");
            _animator.SetTrigger("idle");
        },
        onLogic: (state) => {
            if (!_isPreFightIdle)
            {
                TriggerNextState();
            }
        }));

        // --- IdleState SETUP ---
        fsm.AddState("IdleState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering IdleState");
        }, 
        onLogic: (state) => {
            if (state.timer.Elapsed >= _actionDuration) {
                TriggerNextState();
            }
        }));

        // --- DashState SETUP ---
        fsm.AddState("DashState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering DashState");
            EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.RatDashWindup } });
            if (CloserToLeft())
            {
                _moveTarget = _waypointDashRight.position;
                FaceRight();
            } else
            {
                _moveTarget = _waypointDashLeft.position;
                FaceLeft();
            }
            _animator.SetTrigger("dashWindup");
        },
        onLogic: (state) => {
            if (_actionSubstate == ActionSubstate.Start)
            {
                // Play windup animation
                // Play windup sfx
                if (state.timer.Elapsed > _dashStartTime)
                {
                    _actionSubstate = ActionSubstate.Action;
                    EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.RatDash } });
                    _animator.SetTrigger("dash");
                }
            } else if (_actionSubstate == ActionSubstate.Action)
            {
                float step = _dashSpeed * Time.deltaTime;
                transform.position = Vector2.MoveTowards(transform.position, _moveTarget, step); 
                if (Vector2.Distance(transform.position, _moveTarget) < Mathf.Epsilon)
                {
                    _actionSubstate = ActionSubstate.End;
                    _tempTimer1 = state.timer.Elapsed;
                    _animator.SetTrigger("idle");
                }
            } else if (_actionSubstate == ActionSubstate.End)
            {
                if (state.timer.Elapsed - _tempTimer1 > _dashEndTime)
                {
                    TriggerNextState();
                }
            }
        },
        onExit: (state) =>
        {
            if (CloserToLeft())
            {
                FaceRight();
            } else
            {
                FaceLeft();
            }
        }));

        // --- ThrowState SETUP ---
        fsm.AddState("ThrowState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering ThrowState");
            _tempCounter1 = 0;
        },
        onLogic: (state) => {
            if (_actionSubstate == ActionSubstate.Start)
            {
                // Face correct direction
                // Play windup animation
                // Play windup sfx
                if (state.timer.Elapsed > _dashStartTime)
                {
                    _actionSubstate = ActionSubstate.Action;
                    _animator.SetTrigger("throw");
                }
            }
            else if (_actionSubstate == ActionSubstate.Action)
            {
                // Ensures this only runs once
                if (_tempCounter1 == 0)
                {
                    if (CloserToLeft())
                    {
                        throw1 = StartCoroutine(ThrowProjectile(new Vector2(.2f, 1.5f).normalized, 10f, 0f, () => { }, _projectileSpawnRight.position));
                        throw2 = StartCoroutine(ThrowProjectile(new Vector2(.45f, 1.5f).normalized, 10f, .4f, () => { }, _projectileSpawnRight.position));
                        throw3 = StartCoroutine(ThrowProjectile(new Vector2(.72f, 1.5f).normalized, 10f, .8f, () => { }, _projectileSpawnRight.position));
                        throw4 = StartCoroutine(ThrowProjectile(new Vector2(1.1f, 1.5f).normalized, 10f, 1.2f, () =>
                        {
                            _actionSubstate = ActionSubstate.End;
                            _tempTimer1 = state.timer.Elapsed;
                        }, _projectileSpawnRight.position));
                    } else
                    {
                        throw1 = StartCoroutine(ThrowProjectile(new Vector2(-.2f, 1.5f).normalized, 10f, 0f, () => { }, _projectileSpawnLeft.position));
                        throw2 = StartCoroutine(ThrowProjectile(new Vector2(-.45f, 1.5f).normalized, 10f, .4f, () => { }, _projectileSpawnLeft.position));
                        throw3 = StartCoroutine(ThrowProjectile(new Vector2(-.72f, 1.5f).normalized, 10f, .8f, () => { }, _projectileSpawnLeft.position));
                        throw4 = StartCoroutine(ThrowProjectile(new Vector2(-1.1f, 1.5f).normalized, 10f, 1.2f, () =>
                        {
                            _actionSubstate = ActionSubstate.End;
                            _tempTimer1 = state.timer.Elapsed;
                        }, _projectileSpawnLeft.position));
                    }
                    
                    _tempCounter1 = 1;
                }
            }
            else if (_actionSubstate == ActionSubstate.End)
            {
                if (state.timer.Elapsed - _tempTimer1 > _throwEndTime)
                {
                    TriggerNextState();
                }
                _animator.SetTrigger("idle");
            }
        },
        onExit: (state) =>
        {
            if (throw1 != null) StopCoroutine(throw1);
            if (throw2 != null) StopCoroutine(throw2);
            if (throw3 != null) StopCoroutine(throw3);
            if (throw4 != null) StopCoroutine(throw4);
        }));

        // --- JumpState SETUP ---
        fsm.AddState("JumpState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering JumpState");
            _jumpSubstate = JumpSubstate.Jump;
            _shadowObject.SetActive(true);
            _tempTimer1 = state.timer.Elapsed;
            _animator.SetTrigger("dashWindup");
        },
        onLogic: (state) => {
            if (_jumpSubstate == JumpSubstate.PreJump)
            {
                if (state.timer.Elapsed - _tempTimer1 > _jumpWindupTime)
                {
                    _jumpSubstate = JumpSubstate.Jump;
                    _tempTimer1 = state.timer.Elapsed;
                    _animator.SetTrigger("jump");
                }
            } else  if (_jumpSubstate == JumpSubstate.Jump)
            {
                _shadowObject.transform.position = new Vector2(transform.position.x, _shadowObject.transform.position.y);
                _moveTarget = new Vector2(_playerTransform.position.x, _waypointAirLevel.position.y);
                if (Vector2.Distance(transform.position, _moveTarget) < Mathf.Epsilon)
                {
                    _jumpSubstate = JumpSubstate.Hover;
                    _tempTimer1 = state.timer.Elapsed;
                } else
                {
                    transform.position = Vector2.MoveTowards(transform.position, _moveTarget, _jumpSpeed * Time.deltaTime);
                }
            } else if (_jumpSubstate == JumpSubstate.Hover)
            {
                if (state.timer.Elapsed - _tempTimer1 < _jumpHoverTime / 1.1f)
                {
                    _shadowObject.transform.position = new Vector2(transform.position.x, _shadowObject.transform.position.y);
                    _moveTarget = new Vector2(_playerTransform.position.x, _waypointAirLevel.position.y);
                    transform.position = Vector2.MoveTowards(transform.position, _moveTarget, _jumpSpeed * Time.deltaTime);
                }
                if (state.timer.Elapsed - _tempTimer1 > _jumpHoverTime)
                {
                    _jumpSubstate = JumpSubstate.Fall;
                    _tempTimer1 = state.timer.Elapsed;
                    _moveTarget = new Vector2(_moveTarget.x, _waypointGroundLevel.position.y);
                    _animator.SetTrigger("fall");
                }
            }
            else if (_jumpSubstate == JumpSubstate.Fall)
            {
                if (Vector2.Distance(transform.position, _moveTarget) < Mathf.Epsilon)
                {
                    _jumpSubstate = JumpSubstate.Ground;
                    _shadowObject.SetActive(false);
                    _tempTimer1 = state.timer.Elapsed;
                    _impulseSource.GenerateImpulseWithForce(.2f);
                    EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.RatLand } });
                }
                else
                {
                    transform.position = Vector2.MoveTowards(transform.position, _moveTarget, _jumpFallSpeed * Time.deltaTime);
                }
            }
            else if (_jumpSubstate == JumpSubstate.Ground)
            {
                if (state.timer.Elapsed - _tempTimer1 > _jumpGroundTime)
                {
                    TriggerNextState();
                    _tempTimer1 = 0;
                }
            }
        }));

        // --- SpinState SETUP ---
        fsm.AddState("SpinState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering SpinState");
            _animator.SetTrigger("idle");
        },
        onLogic: (state) => {
            if (_actionSubstate == ActionSubstate.Start)
            {
                // Play windup animation
                // Play windup sfx
                if (state.timer.Elapsed > _spinStartTime)
                {
                    _actionSubstate = ActionSubstate.Action;
                    _tempTimer1 = state.timer.Elapsed;
                    _spinHitbox.SetActive(true);
                    _spinParticleSystem.Emit(1);
                    _spinImpulseSource.GenerateImpulseWithForce(1);
                    EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.RatSpin } });
                    _animator.SetTrigger("spin");
                }
            }
            else if (_actionSubstate == ActionSubstate.Action)
            {
                if (state.timer.Elapsed - _tempTimer1 > _spinDuration)
                {
                    _actionSubstate = ActionSubstate.End;
                    _tempTimer1 = state.timer.Elapsed;
                    _spinHitbox.SetActive(false);
                }
            }
            else if (_actionSubstate == ActionSubstate.End)
            {
                if (state.timer.Elapsed - _tempTimer1 > _spinEndTime)
                {
                    TriggerNextState();
                }
            }
        },
        onExit: (state) =>
        {
            _spinHitbox.SetActive(false);
        }));

        fsm.AddState("DeadState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering DeadState");
            _renderer.color = Color.red;
            BoxCollider2D[] colliders = GetComponentsInChildren<BoxCollider2D>();
            foreach (BoxCollider2D collider in colliders)
            {
                if (collider.isTrigger) collider.enabled = false;
            }
            // TODO: play dead animation
            _animator.SetTrigger("idle");
        }));

        fsm.AddTriggerTransitionFromAny("TriggerIdle", new Transition("", "IdleState"));
        fsm.AddTriggerTransitionFromAny("TriggerDash", new Transition("", "DashState"));
        fsm.AddTriggerTransitionFromAny("TriggerThrow", new Transition("", "ThrowState"));
        fsm.AddTriggerTransitionFromAny("TriggerJump", new Transition("", "JumpState"));
        fsm.AddTriggerTransitionFromAny("TriggerSpin", new Transition("", "SpinState"));
        fsm.AddTriggerTransitionFromAny("TriggerDead", new Transition("", "DeadState"));

        fsm.Init();
    }

    void Update()
    {
        fsm.OnLogic();
    }

    private void TriggerNextState()
    {
        // Trigger a transition idle
        _actionDuration = 0;
        fsm.Trigger("TriggerIdle");
        // End transition
        _actionSubstate = ActionSubstate.Start;
        _actionIndex++;
        _actionIndex %= ratActions.Count;
        RatAction newAction = ratActions[_actionIndex];
        switch (newAction.state)
        {
            case RatState.IDLE:
                _actionDuration = newAction.time;
                // Only animate idle when we explicitly say to switch to idle
                // Other states rely on Idle to reset certain variables, and switch to and fro automatically
                _animator.SetTrigger("idle");
                fsm.Trigger("TriggerIdle");
                break;
            case RatState.DASH:
                _actionTarget = newAction.target;
                fsm.Trigger("TriggerDash");
                break;
            case RatState.THROW:
                fsm.Trigger("TriggerThrow");
                break;
            case RatState.JUMP:
                _actionTarget = newAction.target;
                fsm.Trigger("TriggerJump");
                break;
            case RatState.SPIN:
                fsm.Trigger("TriggerSpin");
                break;
        }
    }

    IEnumerator ThrowProjectile(Vector2 direction, float force, float delaySeconds, Action onThrowEnd, Vector2 spawnPos)
    {
        yield return new WaitForSeconds(delaySeconds);
        GameObject newObject = Instantiate(projectilePrefab, spawnPos, transform.rotation) as GameObject;
        EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.RatThrow } });
        ArcProjectile arcProjectile = newObject.GetComponent<ArcProjectile>();
        arcProjectile.Init(direction, force);
        onThrowEnd();
    }

    bool CloserToLeft()
    {
        return Vector2.Distance(transform.position, _waypointDashLeft.position) < Vector2.Distance(transform.position, _waypointDashRight.position);
    }

    void OnEnable()
    {
        EventManager.StartListening("onBossHealthChange", OnBossHealthChange);
        EventManager.StartListening("onDialogueClose", OnDialogueClose);
    }
    void OnDisable()
    {
        EventManager.StopListening("onBossHealthChange", OnBossHealthChange);
        EventManager.StopListening("onDialogueClose", OnDialogueClose);
    }

    void OnBossHealthChange(Dictionary<string, object> data)
    {
        int hp = (int)data["newHp"];
        if (hp <= 0) {
            fsm.Trigger("TriggerDead");
        }
    }

    void OnDialogueClose(Dictionary<string, object> data)
    {
        DialogueObject dialogue = (DialogueObject)data["dialogue"];
        if (dialogue.FreezePlayer)
        {
            Invoke("StartFight", 1f);
        }
    }

    private void StartFight()
    {
        _isPreFightIdle = false;
    }

    private void FaceLeft()
    {
        _renderer.flipX = false;
    }
    private void FaceRight()
    {
        _renderer.flipX = true;
    }
}
