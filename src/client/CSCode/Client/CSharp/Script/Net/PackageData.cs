/*******************************************************************
** 文件名:	PackageData.cs
** 版  权:	(C) 冰川网络有限公司
** 创建人:	许德纪
** 日  期:	2017.12.16
** 版  本:	1.0
** 描  述:	
** 应  用:  数据包转接层

**************************** 修改记录 ******************************
** 修改人: 
** 日  期: 
** 描  述: 
********************************************************************/

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using War.Base;


namespace War.Script
{
    public class PackageData: IPackageData
    {
        //消息特定消息码
        public static readonly int MAX_ACTION_MODULE_MSG = 400;
        //包头大小
        public static readonly int MESSAGE_HEAD_LEN = 3;
        //服务ID
        public byte serverID;
        //消息ID
        public UInt16 msgID;
        //消息内容
        public byte[] sendData;
        public QueueNode<ByteData> recivedata;

        //接收的socket标识
        public uint m_nClassID;




        //取得打包的数据
        public int GetPackSize()
        {
            int nDataLen = 0;

            if (null != sendData)
            {
                nDataLen = sendData.Length;
            }

            return 8 + nDataLen;
        }

        //打包数据(对象内容写到 data里面)
        public int Pack(byte[] buff, int nOffset)
        {

            if (null == sendData)
            {
                return 0;
            }
            int nPose = nOffset;
            buff[nPose++] = serverID;
            byte[] msgIDBytes = BitConverter.GetBytes(msgID);
            Array.Copy(msgIDBytes, 0, buff, nPose, msgIDBytes.Length);
            nPose += msgIDBytes.Length;
            Array.Copy(sendData, 0, buff, nPose, sendData.Length);
            return nPose + sendData.Length - nOffset;
        }

        //解包数据(data内容,写在对象里面)
        public bool Unpack(byte[] buffer, int offset, int bufferLen, MemPool pool)
        {
            int nPose = offset;
            serverID = buffer[nPose];
            ++nPose;
            msgID = BitConverter.ToUInt16(buffer, nPose);
            nPose += 2;
            try
            {
                int nLen = bufferLen - MESSAGE_HEAD_LEN;
                recivedata = pool.Aloc(nLen);//new byte[nLen];
                Array.Copy(buffer, offset + MESSAGE_HEAD_LEN, recivedata.item.data, 0, nLen);
                recivedata.item.nLen = nLen;
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
            return true;
        }

        //设置包的类别ID(通常是socketID)
        public void SetClassID(uint nClassID)
        {
            m_nClassID = nClassID;
        }

        //取得类别ID
        public uint GetClassID()
        {
            return m_nClassID;
        }

        public void Release(MemPool pool)
        {
            if(null!=recivedata)
            {
                pool.Recycle(ref recivedata);
                recivedata = null;
            }
            sendData = null;
        }
    }
}
