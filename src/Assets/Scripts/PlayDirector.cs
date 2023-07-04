using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDirector : MonoBehaviour
{
    [SerializeField] GameObject player = default!;
    PlayerController _playerController=null;
    Logicallnput _logicalInput = new();

    NextQueue _nextQueue = new();
    [SerializeField] PuyoPair[] nextPuyoPairs = { default!, default! }; // ����next�̃Q�[���I�u�W�F�N�g�̐���

    // Start is called before the first frame update
    void Start()
    {
        _playerController = player.GetComponent<PlayerController>();
        _logicalInput.Clear();
        _playerController.SetLogicalInput(_logicalInput);

        _nextQueue.Initialize();
        Spawn(_nextQueue.Update());
        UpdateNextView();
        
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

        if(!player.activeSelf)
        {
            Spawn(_nextQueue.Update());
            UpdateNextView();
        }
    }
    bool Spawn(Vector2Int next) => _playerController.Spawn((PuyoType)next[0], (PuyoType)next[1]);
}
