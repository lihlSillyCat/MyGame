/*******************************************************************
** 文件名:	IPackageData.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2017.12.16
** 版  本:	1.0
** 描  述:	
** 应  用:  网络层回调

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace War.Base
{
    public interface IPackageData
    {
        //取得打包的数据
        int GetPackSize();

        //打包数据(对象内容写到 data里面)
        int Pack(byte[] data, int nOffset);

        //解包数据(data内容,写在对象里面)
        bool Unpack(byte[] data, int nOffset,int nSize, MemPool pool);

        //释放
        void Release(MemPool pool);

        //设置包的类别ID(通常是socketID)
        void SetClassID(uint nClassID);

        //取得类别ID
        uint GetClassID();
    }
}
