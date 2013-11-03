中心仓调拨计划流程发起


WarehouseTransferPlanM 中的ReqFlowID状态代表意义与销售定单SalesOrderM中的SalesFlowID相同

不过只有使用以下几种状态:
0	已经作废
1	正在录入
3	正在审批
6	已经结案

在WarehouseTransferPlanM增加字段OAFlowId来表示OA流程状态
1	待获取
2	已经获取
3	已经发起OA流程
4	OA流程已经处理完成