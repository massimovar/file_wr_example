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
using FTOptix.Modbus;
using System.Collections.Generic;
#endregion

public class TagsCsvImportExport : BaseNetLogic
{
    private string csvPath;

    public override void Start()
    {
        csvPath = new ResourceUri("%PROJECTDIR%/test.csv").Uri;
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void WriteTagsValues()
    {
        // var fanArray = Project.Current.Get("CommDrivers/CODESYSDriver1/CODESYSStation1/Tags/Application/PLC_PRG/FanArray");
        // fanArray.ChildrenRemoteRead();

        var modbusTag1 = Project.Current.GetVariable("CommDrivers/ModbusDriver1/ModbusStation1/Tags/ModbusTag1");

        var variable3 = Project.Current.GetVariable("Model/Variable3");
        variable3.Value = modbusTag1.RemoteRead();

        using (var csvWriter = new CSVFileWriter(csvPath))
        {
            var csvHeader = new string[2] { "name", "value" };
            var modbusTag1Data = new string[2] { variable3.BrowseName, variable3.Value };
            Log.Info(variable3.BrowseName + " " + variable3.Value);
            csvWriter.WriteLine(csvHeader);
            csvWriter.WriteLine(modbusTag1Data);
        }

    }

    [ExportMethod]
    public void ReadTagsValues()
    {
        using (var csvReader = new CSVFileReader(csvPath) { FieldDelimiter = ',', IgnoreMalformedLines = true })
        {
            if (csvReader.EndOfFile())
                return;

            var header = csvReader.ReadLine();
            if (header == null || header.Count == 0)
                return;

            while (!csvReader.EndOfFile())
            {
                var parameters = csvReader.ReadLine();
                Log.Info(parameters[0] + " " + parameters[1]);
            }
        }
    }

    private class CSVFileReader : IDisposable
    {
        public char FieldDelimiter { get; set; } = ',';
        public char QuoteChar { get; set; } = '"';
        public bool IgnoreMalformedLines { get; set; } = false;
        public CSVFileReader(string filePath, Encoding encoding)
        {
            streamReader = new StreamReader(filePath, encoding);
        }
        public CSVFileReader(string filePath)
        {
            streamReader = new StreamReader(filePath, Encoding.UTF8);
        }
        public CSVFileReader(System.IO.StreamReader streamReader)
        {
            this.streamReader = streamReader;
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
                streamReader.Dispose();
            disposed = true;
        }
        public bool EndOfFile()
        {
            return streamReader.EndOfStream;
        }
        public List<string> ReadLine()
        {
            if (EndOfFile())
                return null;
            var line = streamReader.ReadLine();
            return ParseLine(line);
        }
        public List<List<string>> ReadAll()
        {
            var result = new List<List<string>>();
            while (!EndOfFile())
                result.Add(ReadLine());
            return result;
        }
        private List<string> ParseLine(string line)
        {
            var fields = new List<string>();
            var buffer = new StringBuilder("");
            var fieldParsing = false;
            int i = 0;
            while (i < line.Length)
            {
                if (!fieldParsing)
                {
                    if (IsWhiteSpace(line, i))
                    {
                        ++i;
                        continue;
                    }
                    if (i == 0)
                    {
                        // A line must begin with the quotation mark
                        if (!IsQuoteChar(line, i))
                        {
                            if (IgnoreMalformedLines)
                                return null;
                            else
                                throw new FormatException("Expected quotation marks at " + i);
                        }
                        fieldParsing = true;
                    }
                    else
                    {
                        if (IsQuoteChar(line, i))
                            fieldParsing = true;
                        else if (!IsFieldDelimiter(line, i))
                        {
                            if (IgnoreMalformedLines)
                                return null;
                            else
                                throw new FormatException("Wrong field delimiter at " + i);
                        }
                    }
                    ++i;
                }
                else
                {
                    if (IsEscapedQuoteChar(line, i))
                    {
                        i += 2;
                        buffer.Append(QuoteChar);
                    }
                    else if (IsQuoteChar(line, i))
                    {
                        fields.Add(buffer.ToString());
                        buffer.Clear();
                        fieldParsing = false;
                        ++i;
                    }
                    else
                    {
                        buffer.Append(line[i]);
                        ++i;
                    }
                }
            }
            return fields;
        }
        private bool IsEscapedQuoteChar(string line, int i)
        {
            return line[i] == QuoteChar && i != line.Length - 1 && line[i + 1] == QuoteChar;
        }

        private bool IsQuoteChar(string line, int i)
        {
            return line[i] == QuoteChar;
        }

        private bool IsFieldDelimiter(string line, int i)
        {
            return line[i] == FieldDelimiter;
        }

        private bool IsWhiteSpace(string line, int i)
        {
            return Char.IsWhiteSpace(line[i]);
        }

        bool disposed = false;
        StreamReader streamReader;
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


