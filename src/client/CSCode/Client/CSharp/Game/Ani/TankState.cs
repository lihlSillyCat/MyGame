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

    class TankState : BaseState
    {
        /// <summary>
        /// 执行函数列表
        /// </summary>
        private Dictionary<EnActionID, PlayActionDelegate> m_dicActionFunc;

        override public bool Create(Animator animator)
        {
            m_dicActionFunc = new Dictionary<EnActionID, PlayActionDelegate>();
            m_dicActionFunc[EnActionID.DRIVER] = new PlayActionDelegate(OnPlayDriverAction);
            m_dicActionFunc[EnActionID.SEAT_WITH_GUN] = new PlayActionDelegate(OnPlaySeatWithGunAction);
            m_dicActionFunc[EnActionID.SEAT_WITH_GUN_2] = new PlayActionDelegate(OnPlaySeatWithGunAction2);
            m_dicActionFunc[EnActionID.SEAT_NO_GUN] = new PlayActionDelegate(OnPlaySeatNoGunAction);

            base.Create(animator);
            return true;
        }

        override public void Release()
        {
            base.Release();
        }

        override public void Enter()
        {
            if (m_animator == null)
            {
                Debug.LogError("播放动作失败， m_animator = null.");
                return;
            }

            m_animator.SetBool("GetOn", true);
        }

        override public void Leave()
        {
            if (m_animator == null)
            {
                Debug.LogError("播放动作失败， m_animator = null.");
                return;
            }

            m_animator.SetBool("GetOn", false);
        }

        override public bool SetParam(EnActionParamID param, object value)
        {
            if(null == m_animator)
            {
                return false;
            }

            if(!base.SetParam(param, value))
            {
                switch(param)
                {
                    case EnActionParamID.SIDESLIP:
                        {
                            float v = float.Parse(value.ToString());
                            m_animator.SetFloat("Sideslip", v);
                        }
                        return true;
                    default:
                        break;
                }
            }
            return false;
        }

        override public void Play(EnActionID actionID, object contenxt)
        {
            if(m_animator == null)
            {
                Debug.LogError("播放动作失败， m_animator = null.");
                return;
            }

            PlayActionDelegate func;
            if(m_dicActionFunc.TryGetValue(actionID, out func))
            {
                if(func != null)         //定了动作处理函数
                {
                    func(contenxt);
                }
                else                     //添加了动作列表，但是没有指定处理函数，就让基类来处理
                {
                    base.Play(actionID, contenxt);
                }
            }
        }

        private void OnPlayDriverAction(object contenxt)
        {
            m_animator.SetBool("GetOn", true);
            m_animator.SetInteger("SeatNumber", 0);
            m_animator.SetFloat("Sideslip", 0);
        }

        private void OnPlaySeatWithGunAction(object contenxt)
        {
            m_animator.SetBool("OnGround", false);
            m_animator.SetBool("GetOn", true);
            m_animator.SetInteger("SeatNumber", 1);
        }


        private void OnPlaySeatWithGunAction2(object contenxt)
        {
            m_animator.SetBool("OnGround", false);
            m_animator.SetBool("GetOn", true);
            m_animator.SetInteger("SeatNumber", 2);
        }

        private void OnPlaySeatNoGunAction(object contenxt)
        {
            m_animator.SetBool("OnGround", false);
            m_animator.SetBool("GetOn", true);
            m_animator.SetInteger("SeatNumber", 3);
        }
    }
}
