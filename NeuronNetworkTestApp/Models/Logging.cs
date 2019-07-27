using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronNetworkTestApp.Models
{
    public class Logging
    {
        const string path = @"log.txt";
        public void LogString(string text)
        {
            FileInfo file = new FileInfo(path);
            if (!file.Exists) file.Create().Close();
            using (StreamWriter sw = new StreamWriter(path, true, System.Text.Encoding.Default))
            {
                sw.WriteLine(text);
            }
        }

    }
}
