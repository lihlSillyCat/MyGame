/*******************************************************************
** 文件名:	AniDef.cs
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

namespace War.Game.Ani
{
    /// <summary>
    /// 状态类型定义
    /// </summary>
    public enum EnAniState
    {
        None = 0,
        Tank,               //载具状态
        Max,                //最大值
    }


    /// <summary>
    /// 动作ID定义
    /// </summary>
    public enum EnActionID
    {
        DRIVER = 1,                     //开车动作
        SEAT_WITH_GUN,             //拿枪的坐姿
        SEAT_WITH_GUN_2,          //拿枪的坐姿2
        SEAT_NO_GUN,                //没枪的坐姿
    }

    /// <summary>
    /// 动作参数枚举定义
    /// </summary>
    public enum EnActionParamID     
    {
        SIDESLIP = 1,                   //载具状态时用来控制方向
        CUR_HAVE_GUN,               //当前是否有枪
    }
}
