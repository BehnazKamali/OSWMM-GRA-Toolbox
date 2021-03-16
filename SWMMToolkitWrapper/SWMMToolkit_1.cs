using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SWMMToolkitWrapper.Structures;
using SWMMToolkitWrapper.enums;

namespace SWMMToolkitWrapper
{
    public class SWMMToolkit_1 : ISWMMToolkit
    {

        #region standard methods at the original SWMM .dll

        [DllImport("swmm5_1.dll")]
        private static extern int swmm_run(string f1, string f2, string f3);

        [DllImport("swmm5_1.dll")]
        private static extern int swmm_open(string f1, string f2, string f3);

        [DllImport("swmm5_1.dll")]
        private static extern int swmm_start(int saveFlag);

        [DllImport("swmm5_1.dll")]
        private static extern int swmm_step(ref double elapsedTime);

        [DllImport("swmm5_1.dll")]
        private static extern int swmm_end();

        [DllImport("swmm5_1.dll")]
        private static extern int swmm_report();

        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getMassBalErr(ref float runoffErr, ref float flowErr, ref float qualErr);

        [DllImport("swmm5_1.dll")]
        private static extern int swmm_close();

        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getVersion();

        #endregion standard methods at the original SWMM .dll

        #region new API methods

        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getSimulationUnit(int type, ref int value);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getSimulationAnalysisSetting(int type, ref int value);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getSimulationParam(int type, ref double value);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_countObjects(int type, ref int count);
        [DllImport("swmm5_1.dll")]
        //private static extern int swmm_getObjectId(int type, int index, [MarshalAs(UnmanagedType.LPStr)] string id);
        private static extern int swmm_getObjectId(int type, int index, ref string id);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getNodeType(int index, ref int Ntype);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getLinkType(int index, ref int Ltype);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getLinkConnections(int index, ref int Node1, ref int Node2);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getSubcatchOutConnection(int index, ref int type, ref int Index);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getNodeParam(int index, int Param, ref double value);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_setNodeParam(int index, int Param, double value);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getLinkParam(int index, int Param, ref double value);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_setLinkParam(int index, int Param, double value);
        [DllImport("swmm5_1.dll")]
        //private static extern int swmm_getLinkDirection(int index, [MarshalAs(UnmanagedType.LPStr)] string value);
        private static extern int swmm_getLinkDirection(int index, ref string value);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getSubcatchParam(int index, int Param, ref double value);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_setSubcatchParam(int index, int Param, double value);
        [DllImport("swmm5_1.dll")]
        //private static extern int swmm_getSimulationDateTime(int timetype, [MarshalAs(UnmanagedType.LPStr)] string dtimestr);
        private static extern int swmm_getSimulationDateTime(int timetype, ref int year, ref int month, ref int day, ref int hour, ref int minute, ref int second);
        [DllImport("swmm5_1.dll")]
        //private static extern int swmm_setSimulationDateTime(int timetype, [MarshalAs(UnmanagedType.LPStr)] string dtimestr);
        private static extern int swmm_setSimulationDateTime(int timetype, int year, int month, int day, int hour, int minute, int second);
        //[DllImport("swmm5_1.dll")]
        //private static extern int swmm_getCurrentDateTimeStr([MarshalAs(UnmanagedType.LPStr)] string dtimestr);
        //private static extern int swmm_getCurrentDateTimeStr(ref string dtimestr);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getNodeResult(int index, int type, ref double result);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getLinkResult(int index, int type, ref double result);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getSubcatchResult(int index, int type, ref double result);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_setLinkSetting(int index, double setting);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_setNodeInflow(int index, double flowrate);

        // My Release

        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getLinkXsectType(int index, ref int Xsecttype);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_setLinkGeom(int index, int type, double geom1, double geom2, double geom3, double geom4);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getLinkGeom(int index, ref double geom1, ref double geom2, ref double geom3, ref double geom4);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getNodeStats(int index, ref NodeStats nodeStats);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getLinkStats(int index, ref LinkStats linkStats);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getObjectCount(ObjectTypeEnum type, ref int count);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getUCF(int u, ref double ucf);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getElapsedTime(DateTime dateTime, ref int days, ref int hrs, ref int mins);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getVcf(ref double vcf);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getRouteModel(ref int routeModel);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getConduitLinkRoughness(int index, ref double roughness);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_setConduitLinkRoughness(int index, double roughness);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getSystemRoutingStats(ref RoutingTotals routingTotals);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getNodeInflow(int index, ref double inflow);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getOccuredNodeFlooding(ref int isFlooded);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_getSystemFloodingLoss(ref double floodingLoss);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_setMultipleLinksGeom1(int[] indexArray, int count, double goem1);
        [DllImport("swmm5_1.dll")]
        private static extern int swmm_setAverageDWFChangingCoef(double coef);
        [DllImport("swmm5.dll")]
        private static extern int swmm_setGenerateReportFile(int generateReport);
        [DllImport("swmm5.dll")]
        private static extern int swmm_setReportFlags(int report, int input, int subcatchments, int nodes, int links, int continuity, int flowStats, int nodeStats, int controls, int averages, int linesPerPage);

        #endregion new API methods

        #region public standard methods
        public int Run(string f1, string f2, string f3)
        {
            return swmm_run(f1, f2, f3);
        }

        public int Open(string f1, string f2, string f3)
        {
            return swmm_open(f1, f2, f3);
        }

        public int Start(int saveFlag)
        {
            return swmm_start(saveFlag);
        }

        public int Step(ref double elapsedTime)
        {
            return swmm_step(ref elapsedTime);
        }

        public int End()
        {
            return swmm_end();
        }

        public int Report()
        {
            return swmm_report();
        }

        public int GetMassBalErr(float runoffErr, float flowErr, float qualErr)
        {
            return swmm_getMassBalErr(ref runoffErr, ref flowErr, ref qualErr);
        }

        public int Close()
        {
            return swmm_close();
        }

        public int GetVersion()
        {
            return swmm_getVersion();
        }


        #endregion public standard methods

        #region new API methods
        public int GetSimulationUnit(int type, ref int value)
        {
            return swmm_getSimulationUnit(type, ref value);
        }

        public int GetSimulationAnalysisSetting(int type, ref int value)
        {
            return swmm_getSimulationAnalysisSetting(type, ref value);
        }

        public int GetSimulationParam(int type, ref double value)
        {
            return swmm_getSimulationParam(type, ref value);
        }

        public int CountObjects(int type, ref int count)
        {
            return swmm_countObjects(type, ref count);
        }

        public int GetObjectId(int type, int index, ref string id)
        {
            return swmm_getObjectId(type, index, ref id);
        }

        public int GetNodeType(int index, ref int Ntype)
        {
            return swmm_getNodeType(index, ref Ntype);
        }

        public int GetLinkType(int index, ref int Ltype)
        {
            return swmm_getLinkType(index, ref Ltype);
        }

        public int GetLinkConnections(int index, ref int Node1, ref int Node2)
        {
            return swmm_getLinkConnections(index, ref Node1, ref Node2);
        }

        public int GetSubcatchOutConnection(int index, ref int type, ref int Index)
        {
            return swmm_getSubcatchOutConnection(index, ref type, ref Index);
        }
        public int GetNodeParam(int index, int Param, ref double value)
        {
            return swmm_getNodeParam(index, Param, ref value);
        }

        public int SetNodeParam(int index, int Param, double value)
        {
            return swmm_setNodeParam(index, Param, value);
        }

        public int GetLinkParam(int index, int Param, ref double value)
        {
            return swmm_getLinkParam(index, Param, ref value);
        }

        public int SetLinkParam(int index, int Param, double value)
        {
            return swmm_setLinkParam(index, Param, value);
        }

        public int GetLinkDirection(int index, ref string value)
        {
            return swmm_getLinkDirection(index, ref value);
        }

        public int GetSubcatchParam(int index, int Param, ref double value)
        {
            return swmm_getSubcatchParam(index, Param, ref value);
        }

        public int SetSubcatchParam(int index, int Param, double value)
        {
            return swmm_setSubcatchParam(index, Param, value);
        }

        public int GetSimulationDateTime(int timetype, ref int year, ref int month, ref int day, ref int hour, ref int minute, ref int second)
        {
            return swmm_getSimulationDateTime(timetype, ref year, ref month, ref day, ref hour, ref minute, ref second);
        }

        public int SetSimulationDateTime(int timetype, int year, int month, int day, int hour, int minute, int second)
        {
            return swmm_setSimulationDateTime(timetype, year, month, day, hour, minute, second);
        }
        //public int GetCurrentDateTimeStr(ref string dtimestr)
        //{
        //    return swmm_getCurrentDateTimeStr(ref dtimestr);
        //}
        public int GetNodeResult(int index, int type, ref double result)
        {
            return swmm_getNodeResult(index, type, ref result);
        }
        public int GetLinkResult(int index, int type, ref double result)
        {
            return swmm_getLinkResult(index, type, ref result);
        }
        public int GetSubcatchResult(int index, int type, ref double result)
        {
            return swmm_getSubcatchResult(index, type, ref result);
        }
        public int SetLinkSetting(int index, double setting)
        {
            return swmm_setLinkSetting(index, setting);
        }
        public int SetNodeInflow(int index, double flowrate)
        {
            return swmm_setNodeInflow(index, flowrate);
        }

        #endregion new API methods

        // My Release
        #region My API methods

        public int GetLinkXsectType(int index, ref int Xsecttype)
        {
            return swmm_getLinkXsectType(index, ref Xsecttype);
        }

        public int GetLinkGeom(int index, ref double geom1, ref double geom2, ref double geom3, ref double geom4)
        {
            return swmm_getLinkGeom(index, ref geom1, ref geom2, ref geom3, ref geom4);
        }

        public int SetLinkGeom(int index, int type, double geom1, double geom2, double geom3, double geom4)
        {
            return swmm_setLinkGeom(index, type, geom1, geom2, geom3, geom4);
        }

        public int GetNodeStats(int index, ref NodeStats nodeStats)
        {
            return swmm_getNodeStats(index, ref nodeStats);
        }

        public int GetLinkStats(int index, ref LinkStats linkStats)
        {
            return swmm_getLinkStats(index, ref linkStats);
        }

        public int GetObjectCount(ObjectTypeEnum type, ref int count)
        {
            return swmm_getObjectCount(type, ref count);
        }

        public int GetUCF(int u, ref double ucf)
        {
            return swmm_getUCF(u, ref ucf);
        }

        public int GetElapsedTime(DateTime dateTime, ref int days, ref int hrs, ref int mins)
        {
            return swmm_getElapsedTime(dateTime, ref days, ref hrs, ref mins);
        }

        public int GetVcf(ref double vcf)
        {
            return swmm_getVcf(ref vcf);
        }

        public int GetRouteModel(ref int routeModel)
        {
            return swmm_getRouteModel(ref routeModel);
        }

        public int GetConduitLinkRoughness(int index, ref double roughness)
        {
            return swmm_getConduitLinkRoughness(index, ref roughness);
        }

        public int SetConduitLinkRoughness(int index, double roughness)
        {
            return swmm_setConduitLinkRoughness(index, roughness);
        }

        public int GetSystemRoutingStats(ref RoutingTotals routingTotals)
        {
            return swmm_getSystemRoutingStats(ref routingTotals);
        }

        public int GetNodeTotalInflow(int index, ref double inflow)
        {
            return swmm_getNodeInflow(index, ref inflow);
        }

        public int GetOccuredNodeFlooding(ref int isFlooded)
        {
            return swmm_getOccuredNodeFlooding(ref isFlooded);
        }

        public int GetSystemFloodingLoss(ref double floodingLoss)
        {
            return swmm_getSystemFloodingLoss(ref floodingLoss);
        }

        public int SetMultipleLinksGeom1(int[] indexArray, int count, double goem1)
        {
            return swmm_setMultipleLinksGeom1(indexArray, count, goem1);
        }

        public int SetAverageDWFChangingCoef(double coef)
        {
            return swmm_setAverageDWFChangingCoef(coef);
        }

        public int SetGenerateReportFile(int generateReport)
        {
            return swmm_setGenerateReportFile(generateReport);
        }

        public int SetReportFlags(int report, int input, int subcatchments, int nodes, int links, int continuity, int flowStats, int nodeStats, int controls, int averages, int linesPerPage)
        {
            return swmm_setReportFlags(report, input, subcatchments, nodes, links, continuity, flowStats, nodeStats, controls, averages, linesPerPage);
        }

        #endregion

    }
}
