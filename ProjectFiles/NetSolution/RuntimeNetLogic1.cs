#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.Retentivity;
using FTOptix.NativeUI;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using FTOptix.CODESYS;
using FTOptix.CommunicationDriver;
using FTOptix.Modbus;
#endregion

public class RuntimeNetLogic1 : BaseNetLogic
{
    public override void Start()
    {
        PopulateScrollView();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void PopulateScrollView(){
        var varFolder = Project.Current.Get<Folder>("Model/Folder1");
        if (varFolder == null) return;
        foreach (IUAVariable v in varFolder.Children)
        {
            var row = InformationModel.Make<RowLayout>("row" + v.BrowseName);
            var varLabel = InformationModel.Make<Label>("label" + v.BrowseName);
            varLabel.RightMargin = 10;
            varLabel.Text = v.BrowseName;
            var varSpinbox = InformationModel.Make<SpinBox>("spinbox" + v.BrowseName);
            varSpinbox.Width = 100;
            varSpinbox.GetVariable("Value").SetDynamicLink(v, DynamicLinkMode.ReadWrite);
            row.Add(varLabel);
            row.Add(varSpinbox);
            Owner.Add(row);
        }
    }
}
