using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPMUtility
{
    public class BPMMsgHepler
    {
        /// <summary>
        /// 获取数据成功后的信息
        /// </summary>
        /// <param name="proccessName"></param>
        /// <param name="billTypeName"></param>
        /// <param name="billId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string GetSourceSucceed(string proccessName, string billTypeName, int billId, string status)
        {
            return String.Format("发起流程数据源获取成功;{0}流程 {1}{2} 已经被Dispatche服务获取，{1}{2} 已经修改成为待发起流程状态{3}！",
                proccessName,billTypeName,billId,status);        
        }

        /// <summary>
        /// 正确发起流程的信息
        /// </summary>
        /// <param name="proccessName"></param>
        /// <param name="billTypeName"></param>
        /// <param name="billId"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static string StartProccessSucceed(string proccessName, string billTypeName, int billId, string result)
        {            
            return String.Format("发起流程成功;{0}流程 {1}{2} 发起成功，FlowPortal流程服务返回发起结果信息为{3}！",
                proccessName, billTypeName, billId, result);
        }

        /// <summary>
        /// 发起流程成功后写回数据源信息，修改状态
        /// </summary>
        /// <param name="proccessName"></param>
        /// <param name="billTypeName"></param>
        /// <param name="billId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string SucceedWirteBack(string proccessName, string billTypeName, int billId, string status)
        {
            return string.Format("发起流程状态写入：{0} 流程 {1}{2}发起流程成功，原申请单修改为OA工作流审批状态{3}！",
                proccessName, billTypeName, billId, status); 
        }

        /// <summary>
        /// 发起流程失败后写回数据源信息，修改状态
        /// </summary>
        /// <param name="proccessName"></param>
        /// <param name="billTypeName"></param>
        /// <param name="billId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public static string FailedWirteBack(string proccessName, string billTypeName, int billId, string status)
        {
            return string.Format("发起流程状态写入：{0} 流程 {1}{2}发起流程失败，原申请单修改为待Dispatche服务获取状态{3}！",
                proccessName, billTypeName, billId, status); 
        }
    }
}
