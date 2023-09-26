using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FSM;
using Cinemachine;
using Random = UnityEngine.Random;

public class CatBehavior : MonoBehaviour
{
    enum StrandsSubstate
    {
        TeleportOut, Spawn, Fade, Attack, TeleportIn
    }

    private StateMachine fsm;

    public List<CatAction> catActionsEasy;
    public List<CatAction> catActionsHard;

    public bool doLogging = true;

    // PUBLIC CONFIGS
    public bool _isHardMode = false;
    public bool _isPreFightIdle = true;

    public SpriteRenderer _spriteRenderer;
    [Header("Teleport")]
    public GameObject _afterimagePrefab;
    public float _teleportStartTime;
    public float _teleportDelay;
    public float _teleportEndTime;
    public float _teleportMinDistanceFromPlayer = 1f;
    public float _teleportMinDistanceFromLast = 2f;
    [Header("MiddlePause")]
    public float _middlePauseDelay = 2f;
    public GameObject _whirlPrefab;
    [Header("Strands")]
    public GameObject _strandPrefab;
    public float _strandsTeleportOutDelay = 1;
    public float _strandsSpawnDelayEasy = 1;
    public float _strandsSpawnDelayHard = 1;
    public float _strandsFadeDelay = 1;
    public float _strandsAttackDelay = 1;
    public float _strandsTeleportInDelay = 1;
    public int _strandsCountEasy = 6;
    public float _strandWaitEasy = 0.1f;
    public int _strandsCountHard = 6;
    public float _strandWaitHard = 0.1f;
    public float _strandAttackWait = 0.02f;
    [Header("Swipe")]
    public GameObject _swipeObject;
    public float _swipeDuration;
    [Header("Claws")]
    public Vector2 _clawsSpawnOffset;
    public float _clawsStrandDistance = 0.2f;
    public float _clawsMinRotation = -135f;
    public float _clawsMaxRotation = -45f;
    public float _clawsSpawnDelayEasy = 1;
    public float _clawsSpawnDelayHard = 1;
    public float _clawsAttackDelayEasy = 1;
    public float _clawsAttackDelayHard = 1;
    public float _clawsTotalAttacksDurationEasy = 6;
    public float _clawsTotalAttacksDurationHard = 8;
    public int _clawsCountHard = 8;
    public int _clawsCountEasy = 6;
    [Header("Waypoints")]
    public Transform _teleportBoundsBottomLeft;
    public Transform _teleportBoundsTopRight;
    public Transform _strandsBoundsBottomLeft;
    public Transform _strandsBoundsTopRight;
    public Transform _waypointCenter;
    [Header("Hitboxes")]
    // Controls cat dealing damage
    public BoxCollider2D _damageHitbox;
    [Header("Dialogue")]
    public DialogueObject _dialogueHalfHP;

    // RUNTIME VARS
    private int _actionIndex;
    private float _actionDuration;
    private Transform _actionTarget;
    private ActionSubstate _actionSubstate;
    private float _tempTimer1;
    private int _tempCounter1;
    private Vector2 _teleportLastPos;
    private StrandsSubstate _strandsSubstate;
    private List<GameObject> _strandInstances;
    private Coroutine _strandsCoroutine;
    private bool _flickersStarted = false;
    private bool _dialogueTriggered = false;

    // REFERENCES
    // ----------------------------------------------
    private Animator _animator;
    private CinemachineImpulseSource _impulseSource;
    private Transform _playerTransform;
    // Controls cat HP
    private BoxCollider2D _damageableHitbox;
    private bool stopFsm = false;

    void Start()
    {
        fsm = new StateMachine(this);
        EventManager.TriggerEvent("soundtrack", new Dictionary<string, object> { { "name", SoundtrackNames.Wind } });
        _animator = GetComponentInChildren<Animator>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _damageableHitbox = GetComponent<BoxCollider2D>();
        _teleportLastPos = transform.position;
        _strandInstances = new List<GameObject>();

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

        // --- TeleportOutState SETUP ---
        fsm.AddState("TeleportOutState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering TeleportOutState");
            GameObject afterimage = Instantiate(_afterimagePrefab, transform.position, Quaternion.identity);
            afterimage.GetComponent<SpriteRenderer>().flipX = _spriteRenderer.flipX;
            _spriteRenderer.enabled = false;
            EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatTeleportOut } });
        },
        onLogic: (state) => {
            // Delay before moving on from teleportOut (usually delay until teleportIn)
            if (_actionSubstate == ActionSubstate.Start)
            {
                if (state.timer.Elapsed > _teleportStartTime)
                {
                    TriggerNextState();
                }
            }
        }));

        // --- TeleportInState SETUP ---
        fsm.AddState("TeleportInState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering TeleportInState");
            _actionSubstate = ActionSubstate.Action;
            _tempTimer1 = state.timer.Elapsed;
            if (_actionTarget != null)
            {
                transform.position = _actionTarget.transform.position;
            }
            else
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
            if (transform.position.x < _playerTransform.position.x)
            {
                FaceLeft();
                _swipeObject.transform.rotation = Quaternion.Euler(0, 0, 0);
            } else
            {
                FaceRight();
                _swipeObject.transform.rotation = Quaternion.Euler(180, 0, 180);
            }
        },
        onLogic: (state) => {
            // Wait for teleport delay before teleportingIn
            if (_actionSubstate == ActionSubstate.Action)
            {
                if (state.timer.Elapsed - _tempTimer1 > _teleportDelay)
                {
                    EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatTeleportIn } });
                    _animator.SetTrigger("teleportIn");
                    _spriteRenderer.enabled = true;
                    _actionSubstate = ActionSubstate.End;
                    _tempTimer1 = state.timer.Elapsed;
                }
            }
            // Wait for teleportIn to finish
            else if (_actionSubstate == ActionSubstate.End)
            {
                if (state.timer.Elapsed - _tempTimer1 > _teleportEndTime)
                {
                    TriggerNextState();
                }
            }
        }));

        // --- MiddlePauseState SETUP ---
        fsm.AddState("MiddlePauseState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering MiddlePauseState");
            _actionSubstate = ActionSubstate.Start;
            _animator.SetTrigger("teleportOut");
            _actionTarget = _waypointCenter;
            EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatTeleportOut } });
        },
        onLogic: (state) => {
            // Wait for teleportOut animation to finish
            if (_actionSubstate == ActionSubstate.Start)
            {
                if (state.timer.Elapsed > _teleportStartTime)
                {
                    _actionSubstate = ActionSubstate.Action;
                    _tempTimer1 = state.timer.Elapsed;
                    transform.position = _actionTarget.transform.position;
                }
            }
            // Wait for teleport delay before teleportingIn
            else if (_actionSubstate == ActionSubstate.Action)
            {
                if (state.timer.Elapsed - _tempTimer1 > _middlePauseDelay)
                {
                    EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatTeleportIn } });
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
                    TriggerHardMode();
                }
            }
        },
        onExit: (state) =>
        {
            Instantiate(_whirlPrefab, transform.position, Quaternion.identity);
        }));

        // --- StrandsState SETUP ---
        fsm.AddState("StrandsState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering StrandsState");
            _animator.SetTrigger("teleportOut");
            _strandsSubstate = StrandsSubstate.TeleportOut;
            _actionTarget = _waypointCenter;
            EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatTeleportOut } });
            foreach (GameObject strand in _strandInstances)
            {
                Destroy(strand);
            }
            _strandInstances.Clear();
        },
        onExit: (state) => {
            if (_strandsCoroutine != null) StopCoroutine(_strandsCoroutine);
        },
        onLogic: (state) => {
            // Wait to teleport out then spawn strands
            if (_strandsSubstate == StrandsSubstate.TeleportOut)
            {
                if (state.timer.Elapsed > _strandsTeleportOutDelay)
                {
                    transform.position = _actionTarget.transform.position;
                    _strandsCoroutine = StartCoroutine(SpawnStrands((_isHardMode ? _strandsCountHard : _strandsCountEasy)));
                    _strandsSubstate = StrandsSubstate.Spawn;
                    _tempTimer1 = state.timer.Elapsed;
                }
            }
            // Wait for strands to spawn then fade
            else if (_strandsSubstate == StrandsSubstate.Spawn)
            {
                // Sync flickers once all strands have slid in
                if (_strandInstances != null && !_flickersStarted)
                {
                    if (_strandInstances.TrueForAll((instance) =>
                    {
                        return instance.GetComponent<Strand>().flickerStarted;
                    }))
                    {
                        foreach (GameObject strandInstance in _strandInstances)
                        {
                            if (strandInstance.GetComponent<Strand>().flickerStarted)
                            {
                                strandInstance.GetComponent<Strand>().Flicker();
                            }
                        }
                        _flickersStarted = true;
                    }
                }
                if (state.timer.Elapsed - _tempTimer1 > (_isHardMode ? _strandsSpawnDelayHard : _strandsSpawnDelayEasy))
                {
                    if (_strandInstances != null)
                    {
                        foreach (GameObject strand in _strandInstances)
                        {
                            strand.GetComponent<Strand>().Fade();
                        }
                    }
                    _strandsSubstate = StrandsSubstate.Fade;
                    _tempTimer1 = state.timer.Elapsed;
                }
            }
            // Wait for strands to fade then spawn attacks
            else if (_strandsSubstate == StrandsSubstate.Fade)
            {
                if (state.timer.Elapsed - _tempTimer1 > _strandsFadeDelay)
                {
                    _strandsCoroutine = StartCoroutine(StartAttacks(_isHardMode));
                    _strandsSubstate = StrandsSubstate.Attack;
                    _tempTimer1 = state.timer.Elapsed;
                }
            }
            // Wait for strands to attack then teleport in
            else if (_strandsSubstate == StrandsSubstate.Attack)
            {
                if (state.timer.Elapsed - _tempTimer1 > _strandsAttackDelay)
                {
                    _animator.SetTrigger("teleportIn");
                    EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatTeleportIn } });
                    _strandsSubstate = StrandsSubstate.TeleportIn;
                    _tempTimer1 = state.timer.Elapsed;
                }
            }
            // Wait for teleportIn to finish
            else if (_strandsSubstate == StrandsSubstate.TeleportIn)
            {
                if (state.timer.Elapsed - _tempTimer1 > _strandsTeleportInDelay)
                {
                    TriggerNextState();
                }
            }
        }));

        // --- ClawsState SETUP ---
        fsm.AddState("ClawsState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering ClawsState");
            _animator.SetTrigger("teleportOut");
            _strandsSubstate = StrandsSubstate.TeleportOut;
            _actionTarget = _waypointCenter;
            EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatTeleportOut } });
            foreach (GameObject strand in _strandInstances)
            {
                Destroy(strand);
            }
            _strandInstances.Clear();
        },
        onExit: (state) => {
            if (_strandsCoroutine != null) StopCoroutine(_strandsCoroutine);
        },
        onLogic: (state) => {
            // Wait to teleport out then spawn strands
            if (_strandsSubstate == StrandsSubstate.TeleportOut)
            {
                if (state.timer.Elapsed > _strandsTeleportOutDelay)
                {
                    transform.position = _actionTarget.transform.position;
                    _strandsCoroutine = StartCoroutine(SpawnClaws((_isHardMode ? _clawsCountHard : _clawsCountEasy)));
                    _strandsSubstate = StrandsSubstate.Attack;
                    _tempTimer1 = state.timer.Elapsed;
                }
            }
            // Wait for strands to attack then teleport in
            else if (_strandsSubstate == StrandsSubstate.Attack)
            {
                if (state.timer.Elapsed - _tempTimer1 > (_isHardMode ? _clawsTotalAttacksDurationHard : _clawsTotalAttacksDurationEasy))
                {
                    TriggerNextState();
                }
            }
        }));

        // --- SlashState SETUP ---
        fsm.AddState("SlashState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering SlashState");
            if (_isHardMode) { _animator.SetTrigger("swipeHard"); } else { _animator.SetTrigger("swipe"); };
        },
        onLogic: (state) => {
            // Wait for teleportOut animation to finish
            if (_actionSubstate == ActionSubstate.Start)
            {
                if (state.timer.Elapsed > _swipeDuration)
                {
                    TriggerNextState();
                }
            }
        }));


        fsm.AddState("DeadState", new State(
        onEnter: (state) =>
        {
            if (doLogging) Debug.Log("Entering DeadState");
            transform.position = _waypointCenter.position;
            EventManager.TriggerEvent("onWhirlDespawn", new Dictionary<string, object> {});
            BoxCollider2D[] colliders = GetComponentsInChildren<BoxCollider2D>();
            foreach (BoxCollider2D collider in colliders)
            {
                if (collider.isTrigger) collider.enabled = false;
            }
            // TODO: play dead animation
            _animator.SetTrigger("dead");
        }));

        fsm.AddTriggerTransitionFromAny("TriggerIdle", new Transition("", "IdleState"));
        fsm.AddTriggerTransitionFromAny("TriggerMiddlePause", new Transition("", "MiddlePauseState"));
        fsm.AddTriggerTransitionFromAny("TriggerTeleportOut", new Transition("", "TeleportOutState"));
        fsm.AddTriggerTransitionFromAny("TriggerTeleportIn", new Transition("", "TeleportInState"));
        fsm.AddTriggerTransitionFromAny("TriggerStrands", new Transition("", "StrandsState"));
        fsm.AddTriggerTransitionFromAny("TriggerClaws", new Transition("", "ClawsState"));
        fsm.AddTriggerTransitionFromAny("TriggerSlash", new Transition("", "SlashState"));
        fsm.AddTriggerTransitionFromAny("TriggerDead", new Transition("", "DeadState"));

        fsm.Init();
    }

    void Update()
    {
        fsm.OnLogic();
    }

    private void TriggerHardMode()
    {
        _actionIndex = catActionsHard.Count - 1;
        _isHardMode = true;
        TriggerNextState();
    }

    private void TriggerNextState()
    {
        if (!stopFsm)
        {

            // Trigger a transition idle
            _actionDuration = 0;
            fsm.Trigger("TriggerIdle");
            // End transition
            _actionSubstate = ActionSubstate.Start;
            _actionIndex++;
            _actionIndex %= (_isHardMode ? catActionsHard.Count : catActionsEasy.Count);
            CatAction newAction = (_isHardMode ? catActionsHard[_actionIndex] : catActionsEasy[_actionIndex]);
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
                case CatState.TELEPORT_OUT:
                    fsm.Trigger("TriggerTeleportOut");
                    break;
                case CatState.TELEPORT_IN:
                    _actionTarget = newAction.target;
                    fsm.Trigger("TriggerTeleportIn");
                    break;
                case CatState.SLASH:
                    fsm.Trigger("TriggerSlash");
                    break;
                case CatState.PROJECTILE:
                    fsm.Trigger("TriggerProjectile");
                    break;
            }
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
        int maxhp = (int)data["maxHp"];
        if (hp <= 0)
        {
            fsm.Trigger("TriggerDead");
        } else if (!_dialogueTriggered)
        {
            if (hp <= maxhp / 2)
            {
                GameObject ui = GameObject.FindGameObjectWithTag("UI");
                DialogueManager dialogueManager = ui.GetComponent<DialogueManager>();
                if (dialogueManager && _dialogueHalfHP)
                {
                    _dialogueTriggered = true;
                    dialogueManager.ShowDialogue(_dialogueHalfHP, closeTime: 1.5f, onEnd: () =>
                    {
                        EventManager.TriggerEvent("soundtrackAmpUp", new Dictionary<string, object> { });
                    });
                    fsm.Trigger("TriggerMiddlePause");
                }
            }
        }
    }

    void OnDialogueClose(Dictionary<string, object> data)
    {
        DialogueObject dialogue = (DialogueObject)data["dialogue"];
        if (dialogue.FreezePlayer)
        {
            Invoke("StartFight", 1f);
        }
        if (dialogue.name == "CatBossDeath")
        {
            fsm.Trigger("TriggerTeleportOut");
            stopFsm = true;
        }
    }

    private void StartFight()
    {
        _isPreFightIdle = false;
    }

    private void FaceLeft()
    {
        _spriteRenderer.flipX = true;
    }
    private void FaceRight()
    {
        _spriteRenderer.flipX = false;
    }

    private IEnumerator SpawnStrands(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 strandSpawnPos = new Vector2(Random.Range(_strandsBoundsBottomLeft.position.x, _strandsBoundsTopRight.position.x),
                    Random.Range(_strandsBoundsBottomLeft.position.y, _strandsBoundsTopRight.position.y));
            Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
            if (_isHardMode)
            {
                EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatStrandSpawnHard } });
            }
            else
            {
                EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatStrandSpawn } });
            }
            GameObject strand = Instantiate(_strandPrefab, strandSpawnPos, randomRotation);
            strand.GetComponent<Strand>().Init(true).Slide();
            _strandInstances.Add(strand);
            yield return new WaitForSeconds((_isHardMode ? _strandWaitHard : _strandWaitEasy));
        }
    }

    private IEnumerator SpawnClaws(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 strandSpawnPos = _playerTransform.position;
            Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(_clawsMinRotation, _clawsMaxRotation));
            Vector2 angleVector = (randomRotation * Vector2.right).normalized;
            Vector2 spawnOffsetVector = Vector2.Perpendicular(angleVector);
            if (_isHardMode)
            {
                EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatStrandSpawnHard } });
            }
            else
            {
                EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatStrandSpawn } });
            }
            GameObject strand1 = Instantiate(_strandPrefab, strandSpawnPos + (spawnOffsetVector * _clawsStrandDistance) + _clawsSpawnOffset, randomRotation);
            GameObject strand2 = Instantiate(_strandPrefab, strandSpawnPos + _clawsSpawnOffset, randomRotation);
            GameObject strand3 = Instantiate(_strandPrefab, strandSpawnPos - (spawnOffsetVector * _clawsStrandDistance) + _clawsSpawnOffset, randomRotation);
            strand1.GetComponent<Strand>().Init(false).Slide();
            strand2.GetComponent<Strand>().Init(true).Slide();
            strand3.GetComponent<Strand>().Init(false).Slide();
            _strandInstances.Add(strand1);
            _strandInstances.Add(strand2);
            _strandInstances.Add(strand3);
            Invoke("ClawAttack", (_isHardMode ? _clawsAttackDelayHard : _clawsAttackDelayEasy));
            yield return new WaitForSeconds((_isHardMode ? _clawsSpawnDelayHard : _clawsSpawnDelayEasy));
        }
    }

    private IEnumerator StartAttacks(bool isHardMode)
    {
        if (_strandInstances != null)
        {
            if (isHardMode)
            {
                EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatStrandAttackHard } });
            }
            else
            {
                EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatStrandAttack } });
            }
            foreach (GameObject strand in _strandInstances)
            {
                strand.GetComponent<Strand>().Attack();
                yield return new WaitForSeconds(_strandAttackWait);
            }
        }
    }
    private void ClawAttack()
    {
        if (_strandInstances != null && _strandInstances.Count >= 3)
        {
            if (_isHardMode)
            {
                EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatClawAttack } });
            }
            else
            {
                EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.CatClawAttack } });
            }
            for (int i = 0; i < 3; i++)
            {
                if (_strandInstances.Count > 0) _strandInstances[0].GetComponent<Strand>().Attack();
                if (_strandInstances.Count > 0) _strandInstances.RemoveAt(0);
            }
        }
    }

    public void PlaySwipeSfx()
    {
        EventManager.TriggerEvent("sfx", new Dictionary<string, object> { { "name", SfxNames.PlayerAttackMiss } });
    }
}
