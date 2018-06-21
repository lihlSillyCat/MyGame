/*******************************************************************
** 文件名:	IAniState.cs
** 版  权:	(C) 冰川网络股份有限公司
** 创建人:	郑秀程
** 日  期:	2017.11.07
** 版  本:	1.0
** 描  述:	
** 应  用:  角色状态接口

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
    public interface IAniState
    {
        /// <summary>
        /// 创建
        /// </summary>
        /// <returns></returns>
        bool Create(Animator animator);

        /// <summary>
        /// 释放
        /// </summary>
        void Release();

        /// <summary>
        /// 进入状态
        /// </summary>
        void Enter();

        /// <summary>
        /// 离开状态
        /// </summary>
        void Leave();

        /// <summary>
        /// 播放一个动作
        /// </summary>
        /// <param name="actionID"></param>
        /// <param name="contenxt"></param>
        void Play(EnActionID actionID, object contenxt);

        /// <summary>
        /// 通过参数名称直接设置参数值
        /// </summary>
        /// <param name="param"></param>
        /// <param name="value"></param>
        /// <returns>设置成功返回true</returns>
        bool SetParam(EnActionParamID param, object value);

        /// <summary>
        /// 更新现场
        /// </summary>
        /// <param name="conext"></param>
        void UpdateContext(object conext);

    }
}
