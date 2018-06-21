/*******************************************************************
** 文件名:	AniCtrl.cs
** 版  权:	(C) 冰川网络股份有限公司
** 创建人:	郑秀程
** 日  期:	2017.11.07
** 版  本:	1.0
** 描  述:	
** 应  用:  角色动作控制器，用状态来管理不同状态下的动作表现

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace War.Game.Ani
{
    public class AniCtrl
    {
        /// <summary>
        /// 状态列表
        /// </summary>
        private Dictionary<EnAniState, IAniState> m_dicStates;

        /// <summary>
        /// 切换动作状态
        /// </summary>
        private EnAniState m_curState;

        /// <summary>
        /// 创建动画管理器
        /// </summary>
        /// <param name="animator">角色的动画控制器</param>
        /// <returns></returns>
        public bool Create(Animator animator)
        {
            m_dicStates = new Dictionary<EnAniState, IAniState>();

            IAniState curAniState = new BaseState();
            if(curAniState.Create(animator))
            {
                m_dicStates[EnAniState.None] = curAniState;
            }

            curAniState = new TankState();
            if (curAniState.Create(animator))
            {
                m_dicStates[EnAniState.Tank] = curAniState;
            }

            return true;
        }

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(EnAniState state)
        {
            IAniState curAniState = m_dicStates[m_curState];
            IAniState aniState = m_dicStates[state];

            curAniState.Leave();

            m_curState = state;

            aniState.Enter();
        }

        /// <summary>
        /// 获取动作控制器
        /// </summary>
        /// <returns></returns>
        public IAniState GetAniState()
        {
            return m_dicStates[m_curState];
        }

        /// <summary>
        /// 设置动作参数
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        public void SetAniParam(EnActionParamID param, object value)
        {
            IAniState curAniState = m_dicStates[m_curState];
            curAniState.SetParam(param, value);
        }

        /// <summary>
        /// 播放一个动作
        /// </summary>
        /// <param name="actionID"></param>
        /// <param name="context"></param>
        public void PlayAction(EnActionID actionID, object context)
        {
            IAniState curAniState = m_dicStates[m_curState];
            curAniState.Play(actionID, context);
        }

        /// <summary>
        /// 销毁
        /// </summary>
        public void Release()
        {
            foreach(IAniState aniState in m_dicStates.Values)
            {
                aniState.Release();
            }
            m_dicStates.Clear();
        }
    }
}
