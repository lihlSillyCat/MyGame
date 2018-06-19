/*******************************************************************
** 文件名:	AniCtrl.cs
** 版  权:	(C) 冰川网络股份有限公司
** 创建人:	郑秀程
** 日  期:	2017.11.07
** 版  本:	1.0
** 描  述:	
** 应  用:  角色动画状态的基类，可以处理一些一般性的动作表现

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
    class BaseState : IAniState
    {
        //动作设置器
        protected Animator m_animator;

        //执行函数代理
        protected delegate void PlayActionDelegate(object contenxt);

        /// <summary>
        /// 执行函数列表
        /// </summary>
        private Dictionary<EnActionID, PlayActionDelegate> m_dicActionFunc;

        virtual public bool Create(Animator animator)
        {
            m_animator = animator;
            m_dicActionFunc = new Dictionary<EnActionID, PlayActionDelegate>();
            return true;
        }

        virtual public void Release()
        {
            m_animator = null;
        }

        virtual public void Enter()
        {
            
        }

        virtual public void Leave()
        {
            
        }

        virtual public void UpdateContext(object conext)
        {
        }

        virtual public bool SetParam(EnActionParamID param, object value)
        {
            switch(param)
            {
                case EnActionParamID.CUR_HAVE_GUN:
                    {
                        //m_animator.SetBool("CurrentHaveGun", bool.Parse(value.ToString()));
                    }
                    break;
            }
            return false;
        }

        virtual public void Play(EnActionID actionID, object contenxt)
        {
            if (m_animator == null)
            {
                Debug.LogError("播放动作失败， m_animator = null.");
                return;
            }

            PlayActionDelegate func;
            if (m_dicActionFunc.TryGetValue(actionID, out func))
            {
                if (func != null)         //定了动作处理函数
                {
                    func(contenxt);
                }
            }

        }

    }
}
