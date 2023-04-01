using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kingdee.BOS.Util;
using Kingdee.BOS.Core.Report;
using Kingdee.K3.CRM.OPP.APP.Report;
using Kingdee.BOS.App;
using Kingdee.BOS.App.Data;
using Kingdee.BOS.Contracts;
using Kingdee.BOS.Orm.DataEntity;
using System.ComponentModel;


namespace Kingdee.DFZY.sale.report.SaleAllInReport
{
    [Description("销售合同执行明细报表二开扩展插件")]
    [HotUpdate]
    public class SaleAllInReport : ContractDetailRpt
    {
        /// <summary>
        /// 主要取数逻辑方法
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="tableName"></param>
        public override void BuilderReportSqlAndTempTable(IRptParams filter, string tableName)
        {
            base.BuilderReportSqlAndTempTable(filter, tableName);
            //获取到原有的临时表内容
            IDBService service = ServiceHelper.GetService<IDBService>();
            string tmpTableName = service.CreateTemporaryTableName(this.Context);
            DBUtils.Execute(this.Context, string.Format("select * into {0} from {1}", tmpTableName, tableName));

            //用原有的表联查项目
            dorpTableName(tableName);
            setTmpData(tableName, tmpTableName, filter);

        }

        /// <summary>
        /// 清除系统原有临时表
        /// </summary>
        /// <param name="tableName"></param>
        private void dorpTableName(string tableName)
        {
            string dropSql = string.Format("drop table {0}", tableName);
            DBUtils.Execute(this.Context, dropSql);
        }

        /// <summary>
        /// 增加数据,并且添加过滤
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tempTableName"></param>
        private void setTmpData(string tableName, string tempTableName, IRptParams filter)
        {
            DynamicObject customFilter = filter.FilterParameter.CustomFilter  ;
            DynamicObject proBegin = customFilter["F_VRGO_proBegin"] as DynamicObject;
            DynamicObject proEnd = customFilter["F_VRGO_proEnd"] as DynamicObject;
            DynamicObject xiangMuType = customFilter["F_VRGO_xiangMuTypeName"] as DynamicObject;
             //string xqdh = Convert.ToString(customFilter["F_VRGO_KHXQDH"]);

             string projectBeginNumber = proBegin == null ? "" : Convert.ToString(proBegin["Number"]);
             string projectEndNumber = proEnd == null ? "" : Convert.ToString(proEnd["Number"]);
             string xiangMuTypeNumber = xiangMuType == null ? "" : Convert.ToString(xiangMuType["Number"]);
            StringBuilder sql = new StringBuilder(string.Format(@"/*dialect*/select 
                                                                            project.fnumber as F_VRGO_proNumber  
                                                                            ,project.fxmid as F_VRGO_proId  
                                                                            ,Cust100003.F_VRGO_SQRQ as F_VRGO_proSqrq
                                                                            ,Cust100003.F_VRGO_xmmc as F_VRGO_proName
                                                                            ,project.FDOCUMENTSTATUS  as F_VRGO_proStatus
                                                                            ,customerl.fname as F_VRGO_customName
                                                                            ,customer.fnumber as F_VRGO_cusNumber
                                                                            ,project.F_VRGO_XMMCC as F_VRGO_keTiHao
                                                                            ,xiangMuType.fnumber as F_VRGO_xiangMuTypeNumber
                                                                            ,xiangMuTypeL.FDATAVALUE  as F_VRGO_xiangMuTypeName
                                                                            ,saleContractFin.FBillAllAmount as F_VRGO_FBillAllAmount
																			,saleContract.F_VRGO_YFKAmount2 as saleContractAllYiShouKuanAmount  
                                                                            ,saleContract.F_VRGO_YKPAmount3 as saleContractAllYiKaiPiaoAmount  
																			,saleContractEntryF.FALLAMOUNT as F_VRGO_FEntryALLAMOUNT
																			,saleContract.F_VRGO_YFKAmount2 * saleContractEntryF.FALLAMOUNT/saleContractFin.FBillAllAmount as F_VRGO_saleContractYiShouKuanAmount
                                                                            ,saleContract.F_VRGO_YKPAmount3 * saleContractEntryF.FALLAMOUNT/saleContractFin.FBillAllAmount as F_VRGO_saleContractYiKaiPiaoAmount
                                                                            ,temp.* into {0}
                                                                            from {1} temp
                                                                            inner join T_CRM_CONTRACT saleContract on  saleContract.fid=temp.fid
                                                                            inner join T_CRM_CONTRACTFIN  saleContractFin on  saleContract.fid=saleContractFin.fid
																			inner join T_CRM_CONTRACTENTRY saleContractEntry on saleContractEntry.FID=saleContract.fid and saleContractEntry.fseq=temp.fseq
																			inner join T_CRM_CONTRACTENTRY_F  saleContractEntryF on saleContractEntry.FENTRYID=saleContractEntryF.FENTRYID
                                                                            inner join V_VRGO_Project project  on saleContract.F_VRGO_project=project.fxmid
                                                                            inner join  VRGO_t_Cust100003 Cust100003 on Cust100003.fid=project.fxmid
                                                                            inner join T_BAS_ASSISTANTDATAENTRY xiangMuType on xiangMuType.fentryid=project.F_VRGO_Assistant
                                                                            left join T_BAS_ASSISTANTDATAENTRY_L xiangMuTypeL on xiangMuTypeL.fentryid=project.F_VRGO_Assistant
                                                                            left join  T_BD_CUSTOMER_l  customerl on project.F_VRGO_XQKH=customerl.fcustid
                                                                            left join  T_BD_CUSTOMER  customer on project.F_VRGO_XQKH=customer.fcustid
                                                                            
                            where 1=1 ", tableName, tempTableName));
            if (!projectBeginNumber.Equals(""))
            {
                sql.AppendFormat(" and project.fnumber>= '{0}'", projectBeginNumber);
            }
            if (!projectEndNumber.Equals(""))
            {
                sql.AppendFormat(" and project.fnumber<= '{0}'", projectEndNumber);
            }
            if (!xiangMuTypeNumber.Equals(""))
            {
                sql.AppendFormat(" and xiangMuType.fnumber= '{0}'", xiangMuTypeNumber);
            }
            DBUtils.Execute(this.Context, sql.ToString());
        }
    }
}
