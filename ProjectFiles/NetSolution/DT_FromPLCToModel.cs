#region Using directives
using UAManagedCore;
using FTOptix.HMIProject;
using FTOptix.Core;
using FTOptix.NetLogic;
using FTOptix.CommunicationDriver;
using System.Linq;
using FTOptix.CODESYS;
using System;
#endregion

public class DT_FromPLCToModel : BaseNetLogic
{
    private Folder modelFolder;

    [ExportMethod]
    public void GenerateNodesIntoModel()
    {
        modelFolder = Project.Current.Get<Folder>("Model");
        var startingNode = InformationModel.Get<TagStructure>(LogicObject.GetVariable("InputNode").Value);
        CreateModelTag<TagStructure>(startingNode, modelFolder);
        CheckDatabinds();
    }

    private void CreateModelTag<T>(IUANode fieldNode, IUANode parentNode, string browseNamePrefix = "")
    {
        switch (fieldNode)
        {
            case T _:
                CreateOrUpdateObject(fieldNode, parentNode, browseNamePrefix);
                break;
            case TagStructureArray _:
                CreateOrUpdateObjectArray(fieldNode, parentNode, browseNamePrefix);
                break;
            default:
                CreateOrUpdateVariable(fieldNode, parentNode, browseNamePrefix);
                break;
        }
    }

    private void CreateOrUpdateObjectArray(IUANode fieldNode, IUANode parentNode, string browseNamePrefix = "")
    {
        var tagStructureArrayTemp = (TagStructureArray)fieldNode;
        foreach (var c in tagStructureArrayTemp.Children.Where(c => !IsArrayDimentionsVar(c)))
        {
            CreateModelTag<TagStructure>(c, parentNode, fieldNode.BrowseName + "_");
        }
    }

    private void CreateOrUpdateObject(IUANode fieldNode, IUANode parentNode, string browseNamePrefix = "")
    {
        var existingNode = GetChild(fieldNode, parentNode, browseNamePrefix);
        if (existingNode == null)
        {
            existingNode = InformationModel.MakeObject(browseNamePrefix + fieldNode.BrowseName);
            parentNode.Add(existingNode);
        }

        foreach (var t in fieldNode.Children.Where(c => !IsArrayDimentionsVar(c)))
        {
            CreateModelTag<TagStructure>(t, existingNode);
        }
    }

    private void CreateOrUpdateVariable(IUANode fieldNode, IUANode parentNode, string browseNamePrefix = "")
    {
        if (IsArrayDimentionsVar(fieldNode)) return;
        var existingNode = GetChild(fieldNode, parentNode, browseNamePrefix);
        if (existingNode == null)
        {
            var mTag = (IUAVariable)fieldNode;
            existingNode = InformationModel.MakeVariable(mTag.BrowseName, mTag.DataType, mTag.ArrayDimensions);
            parentNode.Add(existingNode);
        }
        ((IUAVariable)existingNode).SetDynamicLink((UAVariable)fieldNode);
    }

    private bool IsArrayDimentionsVar(IUANode n) => n.BrowseName.ToLower().Contains("arraydimen");

    private IUANode GetChild(IUANode child, IUANode parent, string browseNamePrefix = "") => parent.Children.FirstOrDefault(c => c.BrowseName == browseNamePrefix + child.BrowseName);

    private void CheckDatabinds()
    {
        var lDataBinds = modelFolder.FindNodesByType<IUAVariable>().Where<IUAVariable>(v => { return v.BrowseName == "DynamicLink"; });
        foreach (var vDataBind in lDataBinds)
        {
            var IsResolved = LogicObject.Context.ResolvePath(vDataBind.Owner, vDataBind.Value).ResolvedNode;
            if (IsResolved == null) {Log.Info($"{Log.Node(vDataBind.Owner)} has unresolved databind"); }
        }
    }
}

