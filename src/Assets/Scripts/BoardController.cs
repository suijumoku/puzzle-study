using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

struct FallData
{
    public readonly int X { get; }
    public readonly int Y { get; }
    public readonly int Dest { get; }// �������
    public FallData(int x, int y, int dest)
    {
        X=x;
        Y=y;
        Dest=dest;
    }

}
public class BoardController : MonoBehaviour
{
    public const int FALL_FRAME_PER_CELL = 5;// �P�ʃZ��������̗����t���[��
    public const int BOARD_WIDTH = 6;
    public const int BOARD_HEIGHT = 14;

    [SerializeField]GameObject prefabPuyo = default!;

    int[,] _board=new int[BOARD_HEIGHT, BOARD_WIDTH];
    GameObject[,] _Puyos=new GameObject[BOARD_HEIGHT, BOARD_WIDTH];

    //������ۂ̈ꎟ�I�֐�
    List<FallData> _falls = new();
    int _fallFrames = 0;

    private void ClearAll()
    {
        for (int y = 0; y < BOARD_HEIGHT; y++)
        {
            for(int x=0;x<BOARD_WIDTH;x++)
            {
                _board[y,x] = 0;

                if (_Puyos[y,x]!= null) Destroy(_Puyos[y,x]);
                _Puyos[y,x]=null;
            }
        }
    }
    public void Start()
    {
        ClearAll();
    }


    public static bool IsValidated(Vector2Int pos)
    {
        return 0 <= pos.x && pos.x < BOARD_WIDTH
            && 0 <= pos.y && pos.y < BOARD_HEIGHT;
    }

    public bool CanSettle(Vector2Int pos)
    {
        if (!IsValidated(pos)) return false;

        return 0 == _board[pos.y, pos.x];
    }
    public bool Settle(Vector2Int pos,int val)
    {
        if(!CanSettle(pos))return false;

        _board[pos.y, pos.x] = val;

        Debug.Assert(_Puyos[pos.y, pos.x] == null);
        Vector3 world_position = transform.position + new Vector3(pos.x, pos.y, 0.0f);
        _Puyos[pos.y, pos.x] = Instantiate(prefabPuyo, world_position, Quaternion.identity, transform);
        _Puyos[pos.y, pos.x].GetComponent<PuyoController>().SetPuyoType((PuyoType)val);

        return true;
    }

    public bool CheckFall()
    {
        _falls.Clear();
        _fallFrames = 0;

        // �������̍����̋L�^�p
        int[] dsts = new int[BOARD_WIDTH];
        for(int x = 0; x < BOARD_WIDTH; x++) dsts[x] = 0;

        int max_check_line = BOARD_HEIGHT - 1;// ���͂Ղ�Ղ�ʂł͍ŏ�i�͗����Ă��Ȃ�
        for(int y=0;y<max_check_line;y++)// �������Ɍ���
        {
            for(int x=0;x<BOARD_WIDTH;x++)
            {
                if (_board[y, x] == 0) continue;

                int dst = dsts[x];
                dsts[x] = y+1;// ��̂Ղ悪�����Ă���Ȃ玩���̏�

                if (y == 0) continue;// ��ԉ��Ȃ痎���Ȃ�

                if (_board[y - 1, x] != 0) continue;// ��������Ώ��O

                _falls.Add(new FallData(x, y, dst));

                // �f�[�^��ύX���Ă���
                _board[dst, x] = _board[y,x];
                _board[y, x] = 0;
                _Puyos[dst, x] = _Puyos[y, x];
                _Puyos[y, x] = null;

                dsts[x] = dst + 1;// ���̕��͗���������ɏ�ɏ��
            }
        }

        return _falls.Count != 0;
    }

    public bool Fall()
    {
        _fallFrames++;

        float dy = _fallFrames / (float)FALL_FRAME_PER_CELL;
        int di = (int)dy;

        for(int i=_falls.Count-1;0<=i;i--)// ���[�v���ō폜���Ă����S�Ȃ悤�Ɍ�납�猟��
        {
            FallData f = _falls[i];

            Vector3 pos = _Puyos[f.Dest, f.X].transform.localPosition;
            pos.y = f.Y - dy;

            if(f.Y<=f.Dest+di)
            {
                pos.y = f.Dest;
                _falls.RemoveAt(i);
            }
            _Puyos[f.Dest,f.X].transform.localPosition = pos;// �\���ʒu�̍X�V

        }
        return _falls.Count != 0;
    }
}
