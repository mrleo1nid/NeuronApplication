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
            using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default))
            {
                foreach (var row in trainData)
                {
                  var text = String.Empty;
                  text += row[0].ToString();
                  for (int i = 1; i < row.Length; i++)
                  {
                      text += $";{row[i]}";
                  }
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
                    var mass = line.Split(';');
                    double[] data = new double[9];
                    for (int i = 0; i < mass.Length; i++)
                    {
                        data[i] = Convert.ToDouble(mass[i]);
                    }
                    traindata.Add(data);
                }
            }
            return traindata;
        }

        public static double[,] CreateMatrixToArray(double[][] array)
        {
            double[,] matrix = new double[array.Length, array[0].Length];
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < array[i].Length; j++)
                {
                    matrix[i, j] = array[i][j];
                }
            }
            return matrix;
        }

        public static void ClearDataSet()
        {
            FileInfo file = new FileInfo(path);
            if (file.Exists)
            {
                file.Delete();
                file.Create().Close();
            }
            else
            {
                file.Create().Close();
            }
        }
    }
}
