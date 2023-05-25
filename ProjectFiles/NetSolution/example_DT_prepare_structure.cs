#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.CODESYS;
using FTOptix.NativeUI;
using FTOptix.NetLogic;
using FTOptix.UI;
using FTOptix.CoreBase;
using FTOptix.Modbus;
using FTOptix.Retentivity;
using FTOptix.CommunicationDriver;
using FTOptix.Core;
using System.Collections.Generic;
#endregion

public class example_DT_prepare_structure : BaseNetLogic
{
    [ExportMethod]
    public void ClearSymbolNameOnAllNodes(){
        var startingNode = InformationModel.Get(LogicObject.GetVariable("NodeToClear").Value);
        Clear(startingNode);
    }


    private static void Clear(IUANode startingNode)
    {
        foreach (var c in startingNode.Children)
        {
            switch (c)
            {
                case FTOptix.CommunicationDriver.Tag _:
                    var sn = c.GetVariable("SymbolName");
                    if (sn == null) break;
                    sn.Value = string.Empty;
                    break;
                default:
                Clear(c);
                    break;
            }
        }
    }
}
