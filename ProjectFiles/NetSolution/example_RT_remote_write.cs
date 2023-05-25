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
using System.Linq;
#endregion

public class example_RT_remote_write : BaseNetLogic
{
    private IUANode sourceNode;
    private IUANode destinationNode;

    public override void Start()
    {
        sourceNode = InformationModel.Get(LogicObject.GetVariable("sourceNode").Value);
        destinationNode = InformationModel.Get(LogicObject.GetVariable("destinationNode").Value);
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void RemoteWriteToDestination()
    {
        var sourceNodeValues = GetAllTags(sourceNode);
        var destinationNodeValues = GetAllTags(destinationNode);
        // Simplification: assuming that the two lists are identical in elements order.
        for (int i = 0; i < sourceNodeValues.ToList().Count; i++)
        {
            destinationNodeValues[i].RemoteWrite(sourceNodeValues[i].Value);
        }
    }

    [ExportMethod]
    public void ArrayElementRemoteWrite(){
        var rndVal = new Random().Next();
        var array = Project.Current.GetVariable("Model/TestArray");

        int[] arrayVal = (int[]) array.Value.Value;
        arrayVal[1] = rndVal;
        array.Value = arrayVal;
    }

    private List<FTOptix.CommunicationDriver.Tag> GetAllTags(IUANode sourceNode)
    {
        var res = new List<FTOptix.CommunicationDriver.Tag>();
        foreach (var c in sourceNode.Children)
        {
            switch (c)
            {
                case FTOptix.CommunicationDriver.Tag _:
                    res.Add((FTOptix.CommunicationDriver.Tag)c);
                    break;
                default:
                    res.AddRange(GetAllTags(c));
                    break;
            }
        }

        return res;
    }
}
