using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

interface IState
{
    public enum E_State
    {
        Control=0,
        GameOver=1,

        MAX,

        Unchanged,
    }

    E_State Initialize(PlayDirector parent);
    E_State Update(PlayDirector parent);
}
public class PlayDirector : MonoBehaviour
{
    [SerializeField] GameObject player = default!;
    PlayerController _playerController=null;
    Logicallnput _logicalInput = new();

    NextQueue _nextQueue = new();
    [SerializeField] PuyoPair[] nextPuyoPairs = { default!, default! }; // ����next�̃Q�[���I�u�W�F�N�g�̐���

    // ��ԊǗ�
    IState.E_State _current_state=IState.E_State.Control;
    static readonly IState[] states = new IState[(int)IState.E_State.MAX]
    {
        new ControlState(),
        new GameOverState(),
    };
    // Start is called before the first frame update
    void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _logicalInput.Clear();
        _playerController.SetLogicalInput(_logicalInput);

        _nextQueue.Initialize();
        // ��Ԃ̏�����
        InitializeState();
        
    }
    void UpdateNextView()
    {
        _nextQueue.Each((int idx, Vector2Int n) => {
            nextPuyoPairs[idx++].SetPuyoType((PuyoType)n.x, (PuyoType)n.y);
        });
    }

    static readonly KeyCode[] key_code_tbl = new KeyCode[(int)Logicallnput.Key.MAX]
{
        KeyCode.RightArrow, // Rigth
        KeyCode.LeftArrow,  // Left
        KeyCode.X,          // RtoR
        KeyCode.Z,          // RtoL
        KeyCode.UpArrow,    // QuickDrop
        KeyCode.DownArrow,  // Down
};

    // ���͂���荞��
    void UpdateInput()
    {
        Logicallnput.Key inputDev = 0;// �f�o�C�X�l

        // �L�[���͎擾
        for (int i = 0; i < (int)Logicallnput.Key.MAX; i++)
        {
            if (Input.GetKey(key_code_tbl[i]))
            {
                inputDev |= (Logicallnput.Key)(1 << i);
            }
        }
        _logicalInput.Update(inputDev);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // ���͂���荞��
        UpdateInput();

        UpdateState();

        if(!player.activeSelf)
        {
            Spawn(_nextQueue.Update());
            UpdateNextView();
        }
    }
    class ControlState:IState
    {
        public IState.E_State Initialize(PlayDirector parent)
        {
            if(!parent.Spawn(parent._nextQueue.Update()))
            {
                return IState.E_State.GameOver;
            }

            parent.UpdateNextView();
            return IState.E_State.Unchanged;
        }
        public IState.E_State Update(PlayDirector parent)
        {
            return parent.player.activeSelf ? IState.E_State.Unchanged : IState.E_State.Control;
        }
    }

    void InitializeState()
    {
        Debug.Assert(condition: _current_state is >= 0 and < IState.E_State.MAX);

        var next_state = states[(int)_current_state].Initialize(this);

        if (next_state != IState.E_State.Unchanged)
        {
            _current_state = next_state;
            InitializeState();// �������ŏ�Ԃ��ς��悤�Ȃ�A�ċA�I�ɏ������Ăяo��

        }
    }

    void UpdateState()
    {
        Debug.Assert(condition: _current_state is >= 0 and < IState.E_State.MAX);

        var next_state = states[(int)_current_state].Update(this);
        if (next_state != IState.E_State.Unchanged)
        {
            // ���̏�ԂɑJ��
            _current_state = next_state;
            InitializeState();
        }
    }

    class GameOverState : IState
    {
        public IState.E_State Initialize(PlayDirector parent)
        {
            SceneManager.LoadScene(0);// ���g���C
            return IState.E_State.Unchanged;
        }
        public IState.E_State Update(PlayDirector parent)
        {
            return IState.E_State.Unchanged;
        }
    }
    bool Spawn(Vector2Int next) => _playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);
}
