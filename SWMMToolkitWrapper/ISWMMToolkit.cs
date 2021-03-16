using SWMMToolkitWrapper.enums;
using SWMMToolkitWrapper.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMMToolkitWrapper
{
    public interface ISWMMToolkit
    {
        int Run(string f1, string f2, string f3);

        int Open(string f1, string f2, string f3);

        int Start(int saveFlag);

        int Step(ref double elapsedTime);
       
        int End();
     
        int Report();
        
        int GetMassBalErr(float runoffErr, float flowErr, float qualErr);
       
        int Close();
       
        int GetVersion();
       
        int GetSimulationUnit(int type, ref int value);
        int GetSimulationAnalysisSetting(int type, ref int value);
        
        int GetSimulationParam(int type, ref double value);
       
        int CountObjects(int type, ref int count);

        int GetObjectId(int type, int index, ref string id);

        int GetNodeType(int index, ref int Ntype);

        int GetLinkType(int index, ref int Ltype);

        int GetLinkConnections(int index, ref int Node1, ref int Node2);

        int GetSubcatchOutConnection(int index, ref int type, ref int Index);

        int GetNodeParam(int index, int Param, ref double value);

        int SetNodeParam(int index, int Param, double value);

        int GetLinkParam(int index, int Param, ref double value);

        int SetLinkParam(int index, int Param, double value);

        int GetLinkDirection(int index, ref string value);

        int GetSubcatchParam(int index, int Param, ref double value);

        int SetSubcatchParam(int index, int Param, double value);

        int GetSimulationDateTime(int timetype, ref int year, ref int month, ref int day, ref int hour, ref int minute, ref int second);

        int SetSimulationDateTime(int timetype, int year, int month, int day, int hour, int minute, int second);

        int GetNodeResult(int index, int type, ref double result);

        int GetLinkResult(int index, int type, ref double result);

        int GetSubcatchResult(int index, int type, ref double result);

        int SetLinkSetting(int index, double setting);

        int SetNodeInflow(int index, double flowrate);

        int GetLinkXsectType(int index, ref int Xsecttype);

        int GetLinkGeom(int index, ref double geom1, ref double geom2, ref double geom3, ref double geom4);

        int SetLinkGeom(int index, int type, double geom1, double geom2, double geom3, double geom4);

        int GetNodeStats(int index, ref NodeStats nodeStats);

        int GetLinkStats(int index, ref LinkStats linkStats);

        int GetObjectCount(ObjectTypeEnum type, ref int count);

        int GetUCF(int u, ref double ucf);

        int GetElapsedTime(DateTime dateTime, ref int days, ref int hrs, ref int mins);

        int GetVcf(ref double vcf);

        int GetRouteModel(ref int routeModel);

        int GetConduitLinkRoughness(int index, ref double roughness);

        int SetConduitLinkRoughness(int index, double roughness);

        int GetSystemRoutingStats(ref RoutingTotals routingTotals);

        int GetNodeTotalInflow(int index, ref double inflow);

        int GetOccuredNodeFlooding(ref int isFlooded);

        int GetSystemFloodingLoss(ref double floodingLoss);

        int SetMultipleLinksGeom1(int[] indexArray, int count, double goem1);

        int SetAverageDWFChangingCoef(double coef);

        int SetGenerateReportFile(int generateReport);

        int SetReportFlags(int report, int input, int subcatchments, int nodes, int links, int continuity, int flowStats, int nodeStats, int controls, int averages, int linesPerPage);
    }

}
