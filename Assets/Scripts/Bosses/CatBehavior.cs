using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;
using Cinemachine;
using Random = UnityEngine.Random;

public class CatBehavior : MonoBehaviour
{
    private StateMachine fsm;

    public List<CatAction> catActions;

    public bool doLogging = true;

    // PUBLIC CONFIGS
    public bool _isPreFightIdle = true;
    [Header("Teleport")]
    public float _teleportStartTime;
    public float _teleportDelay;
    public float _teleportEndTime;
    public float _teleportMinDistanceFromPlayer = 1f;
    public float _teleportMinDistanceFromLast = 2f;
    [Header("Waypoints")]
    public Transform _teleportBoundsBottomLeft;
    public Transform _teleportBoundsTopRight;
    [Header("Hitboxes")]
    // Controls cat dealing damage
    public BoxCollider2D _damageHitbox;

    // RUNTIME VARS
    private int _actionIndex;
    private float _actionDuration;
    private Transform _actionTarget;
    private ActionSubstate _actionSubstate;
    private float _tempTimer1;
    private int _tempCounter1;
    private Vector2 _teleportLastPos;

    // REFERENCES
    // ----------------------------------------------
    private Animator _animator;
    private CinemachineImpulseSource _impulseSource;
    private SpriteRenderer _renderer;
    private Transform _playerTransform;
    // Controls cat HP
    private BoxCollider2D _damageableHitbox;

    void Start()
    {
        fsm = new StateMachine(this);
        _animator = GetComponentInChildren<Animator>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _damageableHitbox = GetComponent<BoxCollider2D>();
        _teleportLastPos = transform.position;

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
            if (state.timer.Elapsed >= _actionDuration)
            {
                TriggerNextState();
            }
        }));

        // --- TeleportState SETUP ---
        fsm.AddState("TeleportState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering TeleportState");
            _animator.SetTrigger("teleportOut");
            _damageableHitbox.enabled = false;
            _damageHitbox.enabled = false;
        },
        onLogic: (state) => {
            // Wait for teleportOut animation to finish
            if (_actionSubstate == ActionSubstate.Start)
            {
                if (state.timer.Elapsed > _teleportStartTime)
                {
                    _actionSubstate = ActionSubstate.Action;
                    _tempTimer1 = state.timer.Elapsed;
                    if (_actionTarget != null)
                    {
                        transform.position = _actionTarget.transform.position;
                    } else
                    {
                        Vector2 teleportTo = new Vector2(Random.Range(_teleportBoundsBottomLeft.position.x, _teleportBoundsTopRight.position.x),
                            Random.Range(_teleportBoundsBottomLeft.position.y, _teleportBoundsTopRight.position.y));
                        while (Vector2.Distance(_playerTransform.position, teleportTo) < _teleportMinDistanceFromPlayer || Vector2.Distance(_teleportLastPos, teleportTo) < _teleportMinDistanceFromLast)
                        {
                            teleportTo = new Vector2(Random.Range(_teleportBoundsBottomLeft.position.x, _teleportBoundsTopRight.position.x),
                            Random.Range(_teleportBoundsBottomLeft.position.y, _teleportBoundsTopRight.position.y));
                        }
                        transform.position = teleportTo;
                        _teleportLastPos = teleportTo;
                    }
                }
            }
            // Wait for teleport delay before teleportingIn
            else if (_actionSubstate == ActionSubstate.Action)
            {
                if (state.timer.Elapsed - _tempTimer1 > _teleportDelay)
                {
                    _animator.SetTrigger("teleportIn");
                    _actionSubstate = ActionSubstate.End;
                    _tempTimer1 = state.timer.Elapsed;
                }
            }
            // Wait for teleportIn to finish
            else if (_actionSubstate == ActionSubstate.End)
            {
                if (state.timer.Elapsed - _tempTimer1 > _teleportEndTime)
                {
                    _damageableHitbox.enabled = true;
                    _damageHitbox.enabled = true;
                    TriggerNextState();
                }
            }
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
        fsm.AddTriggerTransitionFromAny("TriggerTeleport", new Transition("", "TeleportState"));
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
        _actionIndex %= catActions.Count;
        CatAction newAction = catActions[_actionIndex];
        switch (newAction.state)
        {
            case CatState.IDLE:
                _actionDuration = newAction.time;
                // Only animate idle when we explicitly say to switch to idle
                // Other states rely on Idle to reset certain variables, and switch to and fro automatically
                _animator.SetTrigger("idle");
                fsm.Trigger("TriggerIdle");
                break;
            case CatState.STRANDS:
                _actionTarget = newAction.target;
                fsm.Trigger("TriggerStrands");
                break;
            case CatState.CLAWS:
                fsm.Trigger("TriggerClaws");
                break;
            case CatState.TELEPORT:
                _actionTarget = newAction.target;
                fsm.Trigger("TriggerTeleport");
                break;
            case CatState.SLASH:
                fsm.Trigger("TriggerSlash");
                break;
            case CatState.PROJECTILE:
                fsm.Trigger("TriggerProjectile");
                break;
        }
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
        if (hp <= 0)
        {
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
