using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.Train;
using NeuralBotBase.Storage;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.Neural.Networks.Training.Propagation.Resilient;

namespace NeuralBotBase.Helpers
{
    public class NeuronHelper
    {
        public BasicNetwork NeuralNetwork;
        private StorageDBRepository storageDbRepository;
        private LearnDBRepository learnDbRepository;

        public NeuronHelper()
        {
            storageDbRepository = new StorageDBRepository();
            learnDbRepository= new LearnDBRepository();
            NeuralNetwork = new BasicNetwork();
            NeuralNetwork.AddLayer(new BasicLayer(null, true, 10));
            NeuralNetwork.AddLayer(new BasicLayer(new ActivationSigmoid(), true, 30));
            NeuralNetwork.AddLayer(new BasicLayer(new ActivationSigmoid(), true, 30));
            NeuralNetwork.AddLayer(new BasicLayer(new ActivationSigmoid(), true, 30));
            NeuralNetwork.AddLayer(new BasicLayer(new ActivationSigmoid(), false, 10));
            NeuralNetwork.Structure.FinalizeStructure();
            NeuralNetwork.Reset();
        }

        public string CreateMessage(string request)
        {
            string resulttext = string.Empty;
            var inputs = createDoubles(request);
            IMLData data = new BasicMLData(inputs);
            var output = NeuralNetwork.Compute(data);
            for (int i = 0; i < output.Count; i++)
            {
                var id = Convert.ToInt32(output[i] * 1000000000);
                var row = storageDbRepository.GetStorageRowFromID(id);
                if (!string.IsNullOrEmpty(row.Text))
                {
                    resulttext += $"{row.Text} ";
                }
            }
            return resulttext;
        }

        private double[] createDoubles(string request)
        {
            List<int> ints = new List<int>();
            var mass = request.Split(' ');
            if (mass.Length>10)
            {
                Array.Resize(ref mass,10);
            }
            foreach (var slovo in mass)
            {
                if (!string.IsNullOrEmpty(slovo))
                {
                    storageDbRepository.AddStorageRow(slovo.ToLower());
                    var row = storageDbRepository.GetStorageRowFromText(slovo.ToLower());
                    ints.Add(row.ID);
                }
            }
            double[] inputs = new double[10];
            for (int i = 0; i < inputs.Length; i++)
            {
                inputs[i] = 0;
            }
            for (int i = 0; i < ints.Count; i++)
            {
                inputs[i] = Convert.ToDouble(ints[i]) / 1000000000;
            }
            return inputs;
        }
        public void LearnBot()
        {
            var rows = learnDbRepository.GetStorageRow();
            foreach (var row in rows)
            {
                var inputs = createDoubles(row.Request);
                var outputs = createDoubles(row.Responce);
                IMLDataSet trainingSet = new BasicMLDataSet(new double[][] { inputs }, new double[][] { outputs });
                IMLTrain train = new ResilientPropagation(NeuralNetwork, trainingSet);
                int epoch = 1;
                do
                {
                    train.Iteration();
                    epoch++;
                } while (train.Error > 0.0000000001);
            }
        }

        public void LearnFromString(string request, string responce)
        {
            var inputs = createDoubles(request);
            var outputs = createDoubles(responce);
            IMLDataSet trainingSet = new BasicMLDataSet(new double[][] { inputs }, new double[][] { outputs });
            IMLTrain train = new ResilientPropagation(NeuralNetwork, trainingSet);
            int epoch = 1;
            do
            {
                train.Iteration();
                epoch++;
            } while (train.Error > 0.00000000000001);
        }
    }
}
