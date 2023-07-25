using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // �ړ�����
    const int TRANS_TIME = 3;// �ړ����x�J�ڎ���
    const int ROT_TIME = 3;// ��]���x�J�ڎ���
    // ��������
    const int FALL_COUNT_UNIT = 120; // �Ђƃ}�X��������J�E���g��
    const int FALL_COUNT_SPD = 10; // �������x
    const int FALL_COUNT_FAST_SPD = 20;// �����������̑��x
    const int GROUND_FRAMES = 50; // �ڒn�ړ��\����

    enum Rotstate
    {
        Up = 0,
        Right = 1,
        Down = 2,
        Left = 3,

        Invalid = -1,
    };

    [SerializeField] PuyoController[] _puyoControllers = new PuyoController[2] { default!, default! };
    [SerializeField] BoardController boardController = default!;
    Logicallnput _logicalInput = null;

    Vector2Int _position = new Vector2Int(2,12);// ���Ղ�̈ʒu
    Rotstate _rotate = Rotstate.Up;// 0:�� 1:�E 2:�� 3: ��(�q�Ղ�̈ʒu)

    AnimationController _animationController = new AnimationController();
    Vector2Int _last_position;
    Rotstate _last_rotate = Rotstate.Up;

    // ��������
    int _fallCount = 0;
    int _groundFrame = GROUND_FRAMES; // �ڒn����

    // ���_
    uint _additiveScore = 0;

    void Start()
    {
        gameObject.SetActive(false);// �Ղ�̎�ނ��ݒ肳���܂Ŗ���
    }

    public void SetLogicalInput(Logicallnput reference)
    {
        _logicalInput=reference;
    }

    // �V�����Ղ���o��
    public bool Spawn(PuyoType axis,PuyoType child)
    {
        // �����ʒu�ɏo���邩�m�F
        Vector2Int position = new(2, 12);// �����ʒu
        Rotstate rotate = Rotstate.Up; // �ŏ��͏����
        if(!CanMove(position,rotate))return false;

        // �p�����[�^�̏�����
        _position = _last_position = position;
        _rotate=_last_rotate=rotate;
        _animationController.Set(1);
        _fallCount = 0;
        _groundFrame = GROUND_FRAMES;

        // �Ղ���o��
        _puyoControllers[0].SetPuyoType(axis);
        _puyoControllers[1].SetPuyoType(child);

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));

        gameObject.SetActive(true);

        return true;
    }

    static readonly Vector2Int[] rotate_tbl = new Vector2Int[] {
        Vector2Int.up, Vector2Int.right,Vector2Int.down,Vector2Int.left};

    private static Vector2Int CalcChildPuyoPos(Vector2Int pos, Rotstate rot)
    {
        return pos + rotate_tbl[(int)rot];
    }

    private bool CanMove(Vector2Int pos, Rotstate rot)
    {
        if (!boardController.CanSettle(pos)) return false;
        if (!boardController.CanSettle(CalcChildPuyoPos(pos, rot))) return false;

        return true;
    }

    void SetTransition(Vector2Int pos, Rotstate rot, int time)
    {
        // ��Ԃ̂��߂ɕۑ����Ă���
        _last_position = _position;
        _last_rotate = _rotate;

        // �l�̍X�V
        _position = pos;
        _rotate = rot;

        _animationController.Set(time);
    }
    private bool Translate(bool is_right)
    {
        // ���z�I�Ɉړ��ł��邩���؂���
        Vector2Int pos = _position + (is_right ? Vector2Int.right : Vector2Int.left);
        if (!CanMove(pos, _rotate)) return false;

        // ���ۂɈړ�
        SetTransition(pos, _rotate, TRANS_TIME);

        return true;
    }

    bool Rotate(bool is_right)
    {
        // &3��0011�Ƃ�AND�_�����Z
        Rotstate rot = (Rotstate)(((int)_rotate + (is_right ? +1 : +3)) & 3);

        // ���z�I�Ɉړ��ł��邩���؂���(�㉺���E�ɂ��炵�������m�F)
        Vector2Int pos = _position;
        switch (rot)
        {
            case Rotstate.Down:
                // �E(��)���牺:�����̉����E(��)���Ƀu���b�N������Έ����グ��
                if (!boardController.CanSettle(pos + Vector2Int.down) ||
                    !boardController.CanSettle(pos + new Vector2Int(is_right ? 1 : -1, -1)))
                {
                    pos += Vector2Int.up;
                }
                break;
            case Rotstate.Right:
                // �E:�E�����܂��Ă���΁A���Ɉړ�
                if (!boardController.CanSettle(pos + Vector2Int.right)) pos += Vector2Int.left;
                break;
            case Rotstate.Left:
                // ��:�������܂��Ă���΁A�E�Ɉړ�
                if (!boardController.CanSettle(pos + Vector2Int.left)) pos += Vector2Int.right;
                break;
            case Rotstate.Up:
                break;
            default:
                Debug.Assert(false);
                break;
        }
        if (!CanMove(pos, rot)) return false;

        // ���ۂɈړ�
        SetTransition(pos, rot, ROT_TIME);

        return true;
    }

    void Settle()
    {
        // ���ڐڒn
        bool is_set0 = boardController.Settle(_position, (int)_puyoControllers[0].GetPuyoType());
        Debug.Assert(is_set0);// �u�����̂͋󂢂Ă����ꏊ�̂͂�

        bool is_set1 = boardController.Settle(CalcChildPuyoPos(_position, _rotate), (int)_puyoControllers[1].GetPuyoType());
        Debug.Assert(is_set1);// �u�����̂͋󂢂Ă����ꏊ�̂͂�

        gameObject.SetActive(false);
    }

    void QuickDrop()
    {
        // ��������ԉ��܂ŗ�����
        Vector2Int pos = _position;
        do
        {
            pos += Vector2Int.down;
        } while (CanMove(pos, _rotate));
        pos -= Vector2Int.down;// ���̏ꏊ(�Ō�ɒu�����ꏊ)�ɖ߂�

        _position = pos;

        Settle();
    }



    bool Fall(bool is_fast)
    {
        _fallCount -= is_fast ? FALL_COUNT_FAST_SPD : FALL_COUNT_SPD;

        // �u���b�N���щz������A������̂��`�F�b�N
        while (_fallCount < 0 ) // �u���b�N����ԉ\�����Ȃ����Ƃ��Ȃ��C������̂ŕ��������ɑΉ�
        { 
            if(!CanMove(_position+Vector2Int.down,_rotate))
            {
                // ������Ȃ��Ȃ�
                _fallCount = 0;// �������~�߂�
                if (0 < --_groundFrame) return true;// ���Ԃ�����Ȃ�A�ړ��E��]

                // ���Ԑ؂�ɂȂ�����{���ɌŒ�
                Settle();
                return false;
            }

            // �������Ȃ牺�ɐi��
            _position += Vector2Int.down;
            _last_position += Vector2Int.down;
            _fallCount += FALL_COUNT_UNIT;
        }

        if (is_fast) _additiveScore++; // ���ɓ���āA�������Ƃ��̓{�[�i�X�ǉ�

        return true;
    }
    void Control()
    {
        // ���Ƃ�
        if (!Fall(_logicalInput.IsRaw(Logicallnput.Key.Down))) return; // �ڒn������I��

        // �A�j�����̓L�[���͂��󂯕t���Ȃ�
        if (_animationController.Update()) return;

        // ���s�ړ��̃L�[���͎擾
        if (_logicalInput.IsRepeat(Logicallnput.Key.Right))
        {
            if (Translate(true)) return;
        }
        if (_logicalInput.IsRepeat(Logicallnput.Key.Left))
        {
            if (Translate(false)) return;
        }

        // ��]�̃L�[���͎擾
        if (_logicalInput.IsTrigger(Logicallnput.Key.RotR))// �E��]
        {
            if (Rotate(true)) return;
        }
        if (_logicalInput.IsTrigger(Logicallnput.Key.RotL))// ����]
        {
            if (Rotate(false)) return;
        }
        // �N�C�b�N�h���b�v�̃L�[���͎擾
        if (_logicalInput.IsRelease(Logicallnput.Key.QuickDrop))
        {
            QuickDrop();
        }
    }
    void FixedUpdate()
    {
        // ������󂯂ē�����
        Control();

        // �\��
        Vector3 dy = Vector3.up * (float)_fallCount / (float)FALL_COUNT_UNIT;
        float anim_rate = _animationController.GetNormalized();
        _puyoControllers[0].SetPos(dy+Interpolate(_position, Rotstate.Invalid, _last_position, Rotstate.Invalid, anim_rate));
        _puyoControllers[1].SetPos(dy+Interpolate(_position, _rotate, _last_position, _last_rotate, anim_rate));

    }

    // rate�� 1 -> 0 �ŁApos_last -> pos, rot_last->rot�ɑJ�ځB
    // rot �� RotState.Invalid �Ȃ��]���l�����Ȃ��i���Ղ�p�j
    static Vector3 Interpolate(Vector2Int pos, Rotstate rot, Vector2Int pos_last, Rotstate rot_last, float rate)
    {
        // ���s�ړ�
        Vector3 p = Vector3.Lerp(
            new Vector3((float)pos.x, (float)pos.y, 0.0f),
            new Vector3((float)pos_last.x, (float)pos_last.y, 0.0f), rate);

        if (rot == Rotstate.Invalid) return p;

        // ��]
        float theta0 = 0.5f * Mathf.PI * (float)(int)rot;
        float theta1 = 0.5f * Mathf.PI * (float)(int)rot_last;
        float theta = theta1 - theta0;

        // �߂����ɉ��
        if (+Mathf.PI < theta) theta = theta - 2.0f * Mathf.PI;
        if (theta < -Mathf.PI) theta = theta + 2.0f * Mathf.PI;

        theta = theta0 + rate * theta;

        return p + new Vector3(Mathf.Sin(theta), Mathf.Cos(theta), 0.0f);
    }

    public uint popScore()
    {
        uint score = _additiveScore;
        _additiveScore = 0;

        return score;

    }
}
