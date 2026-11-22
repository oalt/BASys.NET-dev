using System.Text;
using System.Xml;

namespace BASys.DataAccess
{
    /// <summary>
    /// Converts an ETS product database export (*.vd_, text format "EX-IM") into the XML format that is read by
    /// <see cref="EibProductDataReader"/> (port of the Java VDConverter).
    /// </summary>
    public class VdConverter
    {
        private bool _readCaption;

        private bool _readRecord;

        private bool _readTable;

        private bool _readValue;

        private bool _finished;

        private int _valueCount;

        private string? _recordName;

        private List<string> _captions = new List<string>();

        private TextWriter? _writer;

        private int _lineNumber;

        public void Convert(string inputFileName, string outputFileName)
        {
            string? directory = Path.GetDirectoryName(outputFileName);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (StreamReader reader = new StreamReader(inputFileName, Encoding.Latin1))
            using (StreamWriter writer = new StreamWriter(outputFileName, false, Encoding.Latin1))
            {
                _writer = writer;
                Reset();
                ConvertLines(reader);
                _writer = null;
            }
        }

        private void Reset()
        {
            _readCaption = false;
            _readRecord = false;
            _readTable = false;
            _readValue = false;
            _finished = false;
            _valueCount = 0;
            _recordName = null;
            _captions = new List<string>();
            _lineNumber = 0;
        }

        private void ConvertLines(StreamReader reader)
        {
            string? line = reader.ReadLine();
            _lineNumber = 1;

            if (line != "EX-IM")
            {
                throw new InvalidDataException("Wrong file format: the file does not start with EX-IM.");
            }

            _writer!.Write("<?xml version=\"1.0\" encoding=\"ISO-8859-1\" ?>\n");
            _writer.Write("<eib-products>\n");

            line = reader.ReadLine();
            _lineNumber++;
            bool done = false;

            while (!done)
            {
                string? next = reader.ReadLine();
                _lineNumber++;

                while (next != null && next.StartsWith("\\\\", StringComparison.Ordinal))
                {
                    line = line + next.Substring(2);
                    next = reader.ReadLine();
                    _lineNumber++;
                }

                if (line != null)
                {
                    ParseLine(line);
                }

                done = next == null || _finished;
                line = next;
            }

            if (!_finished)
            {
                if (_recordName != null)
                {
                    _writer.Write("\t</" + _recordName + ">\n");
                }

                _writer.Write("</eib-products>\n");
            }
        }

        private void ParseLine(string line)
        {
            string currentLine = line;

            if (currentLine.StartsWith("N ", StringComparison.Ordinal) || currentLine.StartsWith("K ", StringComparison.Ordinal)
                || currentLine.StartsWith("D ", StringComparison.Ordinal) || currentLine.StartsWith("V ", StringComparison.Ordinal)
                || currentLine.StartsWith("H ", StringComparison.Ordinal))
            {
                // header lines carry no data
            }
            else if (currentLine.StartsWith("-", StringComparison.Ordinal))
            {
                _readCaption = false;
                _readRecord = false;
                _readTable = false;
                _readValue = false;
            }
            else if (currentLine.StartsWith("T ", StringComparison.Ordinal))
            {
                string[] lineData = currentLine.Split(' ');
                int tableNumber;

                if (lineData.Length > 1 && int.TryParse(lineData[1], out tableNumber))
                {
                    _captions = new List<string>();
                    _readTable = true;
                }
                else
                {
                    _readValue = true;
                    _readRecord = false;
                }
            }
            else if (currentLine.StartsWith("C", StringComparison.Ordinal))
            {
                if (_readTable && !_readCaption)
                {
                    _readCaption = true;
                }
            }
            else if (currentLine.StartsWith("R ", StringComparison.Ordinal))
            {
                _readValue = false;
                _readCaption = false;
                _readRecord = true;
                _readTable = false;
            }
            else if (currentLine.StartsWith("XXX", StringComparison.Ordinal))
            {
                if (_recordName != null)
                {
                    _writer!.Write("\t</" + _recordName + ">\n");
                }

                _writer!.Write("</eib-products>\n");
                _finished = true;
            }
            else
            {
                _readValue = true;
                _readRecord = false;
            }

            if (!_finished)
            {
                if (_readRecord)
                {
                    string[] recordData = currentLine.Split(' ');
                    CheckTokenCount(recordData, 5, currentLine);
                    _valueCount = 0;

                    if (_recordName != null)
                    {
                        _writer!.Write("\t</" + _recordName + ">\n");
                    }

                    _recordName = recordData[4];
                    _writer!.Write("\t<" + _recordName + ">\n");
                }

                if (_readCaption)
                {
                    string[] captionData = currentLine.Split(' ');
                    CheckTokenCount(captionData, 6, currentLine);
                    _captions.Add(captionData[5]);
                }

                if (_readValue)
                {
                    if (currentLine.Length > 0)
                    {
                        if (_valueCount >= _captions.Count)
                        {
                            throw new InvalidDataException("Error in line " + _lineNumber + ": value without caption: " + currentLine);
                        }

                        string caption = _captions[_valueCount];
                        _writer!.Write("\t\t<" + caption + ">" + Escape(currentLine) + "</" + caption + ">\n");
                    }

                    _valueCount++;
                }
            }
        }

        private void CheckTokenCount(string[] tokens, int requiredCount, string line)
        {
            if (tokens.Length < requiredCount)
            {
                throw new InvalidDataException("Error in line " + _lineNumber + ": " + line);
            }
        }

        private static string Escape(string text)
        {
            string result = text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

            return result;
        }
    }
}
