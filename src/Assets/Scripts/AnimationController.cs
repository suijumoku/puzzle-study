using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController
{
    const float DELTA_TIME_MAX = 1.0f;
    float _time = 0.0f;
    float _inv_time_max = 1.0f;

    public void Set(float max_time)
    {
        Debug.Assert(0.0f < max_time);//�@���̑J�ڎ��Ԃ͕s��

        _time = max_time;
        _inv_time_max = 1.0f / max_time;
    }

    //�A�j���[�V�������Ȃ�true��Ԃ�
    public bool Update(float delta_time)
    {

        // �X�V���Ԃ̏���𓱓�����
        if (DELTA_TIME_MAX < delta_time) delta_time = DELTA_TIME_MAX;

        _time -= delta_time;

        // 0�ɂȂ�����I��
        if (_time <= 0.0f)
        {
            _time = 0.0f;// ���̐��ɂ��Ȃ�
            return false;
        }
        return true;
    }

    public float GetNormalized()
    {
        return _time * _inv_time_max;
    }
}
