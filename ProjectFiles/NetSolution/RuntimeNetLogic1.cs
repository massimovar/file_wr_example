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
using System.IO;
using System.Text;
#endregion

public class RuntimeNetLogic1 : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void WriteTagsValues()
    {
        var fanArray = Project.Current.Get("CommDrivers/CODESYSDriver1/CODESYSStation1/Tags/Application/PLC_PRG/FanArray");
        // fanArray.ChildrenRemoteRead();

        var b1 = Project.Current.GetVariable("CommDrivers/CODESYSDriver1/CODESYSStation1/Tags/Application/PLC_PRG/b_1");
        b1.RemoteRead();
        var csvPath = new ResourceUri("%PROJECTDIR%/test.csv").Uri;
        var csvWriter = new CSVFileWriter(csvPath);
        var csvHeader = new string[2] {"name", "value"};
        var b1Data = new string[2] {b1.BrowseName, b1.Value};
        csvWriter.WriteLine(csvHeader);
        csvWriter.WriteLine(b1Data);
    }

    private class CSVFileWriter : IDisposable
    {
        public char FieldDelimiter { get; set; } = ',';

        public char QuoteChar { get; set; } = '"';

        public CSVFileWriter(string filePath)
        {
            streamWriter = new StreamWriter(filePath, false, Encoding.UTF8);
        }

        public CSVFileWriter(string filePath, Encoding encoding)
        {
            streamWriter = new StreamWriter(filePath, false, encoding);
        }

        public CSVFileWriter(System.IO.StreamWriter streamReader)
        {
            this.streamWriter = streamReader;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
                streamWriter.Dispose();

            disposed = true;
        }

        public void WriteLine(string[] fields)
        {
            var stringBuilder = new StringBuilder();

            for (var i = 0; i < fields.Length; ++i)
            {
                stringBuilder.AppendFormat("{0}{1}{0}", QuoteChar, EscapeField(fields[i]), QuoteChar);
                if (i != fields.Length - 1)
                    stringBuilder.Append(FieldDelimiter);
            }

            streamWriter.WriteLine(stringBuilder.ToString());
            streamWriter.Flush();
        }

        private string EscapeField(string field)
        {
            var quoteCharString = QuoteChar.ToString();
            return field.Replace(quoteCharString, quoteCharString + quoteCharString);
        }

        bool disposed = false;
        StreamWriter streamWriter;
    }
}


