using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronNetworkTestApp.Models
{
    public static class TrainHelper
    {
        const string path = @"dataset.txt";
        public static void SaveTrainData(List<double[]> trainData)
        {
            FileInfo file = new FileInfo(path);
            if (!file.Exists) file.Create().Close();
            foreach (var row in trainData)
            {
                var text = String.Empty;
                text += row[0].ToString();
                for (int i = 1; i < row.Length; i++)
                {
                    text += $",{row[i]}";
                }
                using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine(text);
                }
            }
        }
        public static List<double[]> GetTrainData()
        {
            List<double[]> traindata = new List<double[]>();
            using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var mass = line.Split(',');
                    double[] data = new double[8];
                    for (int i = 0; i < mass.Length; i++)
                    {
                        data[i] = Convert.ToDouble(mass[i]);
                    }
                    traindata.Add(data);
                }
            }
            return traindata;
        }
    }
}
