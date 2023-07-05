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
        Falling=2,

        MAX,

        Unchanged,
    }

    E_State Initialize(PlayDirector parent);
    E_State Update(PlayDirector parent);
}
[RequireComponent(typeof(BoardController))]
public class PlayDirector : MonoBehaviour
{
    [SerializeField] GameObject player = default!;
    PlayerController _playerController=null;
    Logicallnput _logicalInput = new();
    BoardController _boardController = default!;

    NextQueue _nextQueue = new();
    [SerializeField] PuyoPair[] nextPuyoPairs = { default!, default! }; // 次のnextのゲームオブジェクトの制御

    // 状態管理
    IState.E_State _current_state=IState.E_State.Falling;
    static readonly IState[] states = new IState[(int)IState.E_State.MAX]
    {
        new ControlState(),
        new GameOverState(),
        new FallingState(),
    };
    // Start is called before the first frame update
    void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _boardController=GetComponent<BoardController>();
        _logicalInput.Clear();
        _playerController.SetLogicalInput(_logicalInput);


        _nextQueue.Initialize();
        // 状態の初期化
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

    // 入力を取り込む
    void UpdateInput()
    {
        Logicallnput.Key inputDev = 0;// デバイス値

        // キー入力取得
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
        // 入力を取り込む
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
            return parent.player.activeSelf ? IState.E_State.Unchanged : IState.E_State.Falling;
        }
    }

    class FallingState:IState
    {
        public IState.E_State Initialize(PlayDirector parent)
        {
            return parent._boardController.CheckFall() ? IState.E_State.Unchanged : IState.E_State.Control;
        }
        public IState.E_State Update(PlayDirector parent)
        {
            return parent._boardController.Fall() ? IState.E_State.Unchanged : IState.E_State.Control;
        }
    }

    void InitializeState()
    {
        Debug.Assert(condition: _current_state is >= 0 and < IState.E_State.MAX);

        var next_state = states[(int)_current_state].Initialize(this);

        if (next_state != IState.E_State.Unchanged)
        {
            _current_state = next_state;
            InitializeState();// 初期化で状態が変わるようなら、再帰的に初期を呼び出す

        }
    }

    void UpdateState()
    {
        Debug.Assert(condition: _current_state is >= 0 and < IState.E_State.MAX);

        var next_state = states[(int)_current_state].Update(this);
        if (next_state != IState.E_State.Unchanged)
        {
            // 次の状態に遷移
            _current_state = next_state;
            InitializeState();
        }
    }

    class GameOverState : IState
    {
        public IState.E_State Initialize(PlayDirector parent)
        {
            SceneManager.LoadScene(0);// リトライ
            return IState.E_State.Unchanged;
        }
        public IState.E_State Update(PlayDirector parent)
        {
            return IState.E_State.Unchanged;
        }
    }
    bool Spawn(Vector2Int next) => _playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);
}
