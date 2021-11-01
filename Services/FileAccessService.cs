using System.Collections.Generic;
using System.IO;

namespace Task4.Services
{
    public interface IFileAccessService
    {
        public string[] ReadLines(string path);
        public void WriteLines(string path, string[] lines);
    }

    public class FileAccessService: IFileAccessService
    {
        public string[] ReadLines(string path)
        {
            List<string> lines = new List<string>();
            try
            {
                using var fs = new FileStream(path, FileMode.Open);
                using var s = new StreamReader(fs);
                while (!s.EndOfStream)
                {
                    lines.Add(s.ReadLine());
                }
            }
            catch { }

            return lines.ToArray();
        }

        public void WriteLines(string path, string[] lines)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open);
                using var s = new StreamWriter(fs);
                foreach (var line in lines)
                    s.WriteLine(line);
            }
            catch { }
        }
    }
}