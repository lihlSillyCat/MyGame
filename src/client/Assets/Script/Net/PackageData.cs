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
        public static readonly int MESSAGE_HEAD_LEN = 8;



        public byte m_srcEndPoint;

        public byte m_dstEndPoint;

        public UInt16 m_keyModule;
        public UInt32 m_keyAction;

        public UInt32 m_sid;

        public byte[] m_senddata;

        public QueueNode<ByteData> m_Recivedata;



        //接收的socket标识
        public uint m_nClassID;




        //取得打包的数据
        public int GetPackSize()
        {
            int nDataLen = 0;

            if (null != m_senddata)
            {
                nDataLen = m_senddata.Length;
            }

            return 8 + nDataLen;
        }

        //打包数据(对象内容写到 data里面)
        public int Pack(byte[] buff, int nOffset)
        {

            if (null == m_senddata)
            {
                return 0;
            }

            int nPose = nOffset;

            buff[nPose++] = m_srcEndPoint;
            buff[nPose++] = m_dstEndPoint;

            byte[] moduleBytes = BitConverter.GetBytes(m_keyModule);
            Array.Copy(moduleBytes, 0, buff, nPose, moduleBytes.Length);
            nPose += moduleBytes.Length;

            
           
            

            byte[] actionBytes = BitConverter.GetBytes(m_keyAction);
            Array.Copy(actionBytes, 0, buff, nPose, actionBytes.Length);
            nPose += actionBytes.Length;

            Array.Copy(m_senddata, 0, buff, nPose, m_senddata.Length);


            return nPose + m_senddata.Length - nOffset;


        }

        //解包数据(data内容,写在对象里面)
        public bool Unpack(byte[] buffer,  int offset,int bufferLen,MemPool pool)
        {

            int nPose = offset;
            m_srcEndPoint = buffer[nPose];
            ++nPose;
            m_dstEndPoint = buffer[nPose];
            ++nPose;
            m_keyModule = BitConverter.ToUInt16(buffer, nPose);
            nPose += 2;
            m_keyAction = BitConverter.ToUInt32(buffer, nPose);
            nPose += 4;


  



            try
            {
                if (m_keyAction < MAX_ACTION_MODULE_MSG)
                //if (keyModule == 3)
                {
                    try
                    {
                        int nActionMsgHeadLen = MESSAGE_HEAD_LEN + 4;
                        // Action消息的头8个字节是UID，不参与proto解析
                        m_sid = (UInt32)BitConverter.ToUInt32(buffer, offset + MESSAGE_HEAD_LEN);
                        int nLen = bufferLen - nActionMsgHeadLen;
                        m_Recivedata = pool.Aloc(nLen); //new byte[bufferLen - nActionMsgHeadLen];
                        Array.Copy(buffer, offset+nActionMsgHeadLen, m_Recivedata.item.data, 0, nLen);
                        m_Recivedata.item.nLen = nLen;
       
                        //m_MessageEventHandler(packetID, srcEndpoint, dstEndpoint, keyModule, keyAction, sid, protoBytes);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarningFormat("协议进行Action类型解析失败: {0}。将当作普通协议处理。Module={1}, ID={2}", e.Message, m_keyModule, m_keyAction);

                        //byte[] protoBytes = new byte[bufferLen - nHeadLen];
                        // Array.Copy(buffer, nHeadLen, protoBytes, 0, protoBytes.Length);
                        // m_MessageEventHandler(packetID, srcEndpoint, dstEndpoint, keyModule, keyAction, 0, protoBytes);
                    }
                }
                else
                {
                    int nLen = bufferLen - MESSAGE_HEAD_LEN;
                    m_Recivedata = pool.Aloc(nLen);//new byte[nLen];
                    Array.Copy(buffer, offset+MESSAGE_HEAD_LEN, m_Recivedata.item.data, 0, nLen);
                    m_Recivedata.item.nLen = nLen;
     
                    // m_MessageEventHandler(packetID, srcEndpoint, dstEndpoint, keyModule, keyAction, 0, protoBytes);
                }
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
            if(null!=m_Recivedata)
            {
                pool.Recycle(ref m_Recivedata);
                m_Recivedata = null;
            }
          
            m_senddata = null;
            
        }
    }
}
