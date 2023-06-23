using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    enum Rotstate
    { 
        Up=0,
        Right=1,
        Down=2,
        Left=3,

        Invalid=-1,
    };

    [SerializeField] PuyoController[] _puyoControllers = new PuyoController[2] { default!, default! };
    [SerializeField] BoardController boardController = default!;

    Vector2Int _position;// 軸ぷよの位置
    Rotstate _rotate = Rotstate.Up;// 0:上 1:右 2:下 3: 左(子ぷよの位置)
    // Start is called before the first frame update
    void Start()
    {
        _puyoControllers[0].SetPuyoType(PuyoType.Green);
        _puyoControllers[1].SetPuyoType(PuyoType.Red);

        _position = new Vector2Int(2, 12);
        _rotate = Rotstate.Up;

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));
    }

    static readonly Vector2Int[] rotate_tbl = new Vector2Int[] {
        Vector2Int.up, Vector2Int.right,Vector2Int.down,Vector2Int.left};

    private static Vector2Int CalcChildPuyoPos(Vector2Int pos, Rotstate rot)
    {
        return pos + rotate_tbl[(int)rot];
    }

    private bool CanMove(Vector2Int pos,Rotstate rot)
    {
        if(!boardController.CanSettle(pos)) return false;
        if(!boardController.CanSettle(CalcChildPuyoPos(pos,rot)))return false;

        return true;
    }

    private bool Translate(bool is_right)
    {
        // 仮想的に移動できるか検証する
        Vector2Int pos = _position+(is_right ? Vector2Int.right:Vector2Int.left);
        if(!CanMove(pos,_rotate))return false;

        // 実際に移動
        _position = pos;

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3((float)posChild.x, (float)posChild.y, 0.0f));

        return true;
    }

    bool Rotate(bool is_right)
    {
        // &3は0011とのAND論理演算
        Rotstate rot = (Rotstate) ( ((int) _rotate +(is_right ? +1: +3)) &3 );

        // 仮想的に移動できるか検証する(上下左右にずらした時も確認)
        Vector2Int pos = _position;
        switch(rot)
        {
            case Rotstate.Down:
                // 右(左)から下:自分の下か右(左)下にブロックがあれば引き上げる
                if(!boardController.CanSettle(pos+Vector2Int.down) ||
                   !boardController.CanSettle(pos+new Vector2Int(is_right ? 1:-1,-1)))
                {
                    pos += Vector2Int.up;
                }
                break;
            case Rotstate.Right:
                bool a = !boardController.CanSettle(pos + Vector2Int.right);
                // 右:右が埋まっていれば、左に移動
                if (!boardController.CanSettle(pos + Vector2Int.right)) pos += Vector2Int.left;
                break;
            case Rotstate.Left:
                // 左:左が埋まっていれば、右に移動
                if (!boardController.CanSettle(pos + Vector2Int.left)) pos += Vector2Int.right;
                break; 
            case Rotstate.Up:
                break;
                default:
                Debug.Assert(false);
                break;
        }

        if(!CanMove(pos,rot))return false;

        // 実際に移動
        _position=pos;
        _rotate = rot;

        _puyoControllers[0].SetPos(new Vector3((float)_position.x, (float)_position.y, 0.0f));
        Vector2Int posChild = CalcChildPuyoPos(_position, _rotate);
        _puyoControllers[1].SetPos(new Vector3(((float)posChild.x), (float)posChild.y, 0.0f));

        return true;
    }

    void QuickDrop()
    {
        Vector2Int pos=_position;

        do
        {
            pos += Vector2Int.down;
        } while (CanMove(pos, _rotate));
        pos-= Vector2Int.down;

        _position= pos;

        bool is_set0 = boardController.Settle(_position,
            (int)_puyoControllers[0].GetPuyoType());
        Debug.Assert(is_set0);

        bool is_set1 = boardController.Settle(CalcChildPuyoPos(_position, _rotate),
            (int)_puyoControllers[1].GetPuyoType());
        Debug.Assert(is_set1);

        gameObject.SetActive(false);
    }
    void Update()
    {
        // 平行移動のキー入力取得
        if(Input.GetKeyDown(KeyCode.RightArrow))
        {
            Translate(true);
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Translate(false);
        }

        // 回転のキー入力取得
        if(Input.GetKeyDown(KeyCode.X))// 右回転
        {
            Rotate(true);
        }
        if (Input.GetKeyDown(KeyCode.Z))// 右回転
        {
            Rotate(false);
        }

        if(Input.GetKey(KeyCode.UpArrow))
        {
            QuickDrop();
        }
    }
}
