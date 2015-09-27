using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tugas
{
    /* description
     * Both figures illustrate the input-output mechanism for a neural network that has three inputs,
     * a first hidden layer ("A") with four neurons, a second hidden layer ("B") with five neurons and two outputs. 
     * "There are several different meanings for exactly what a deep neural network is, but one is just a neural network 
     * with two (or more) layers of hidden nodes." 3-4-5-2 neural network requires a total of (3 * 4) + 4 + (4 * 5) + 5 + (5 * 2) + 2 = 53 
     * weights and bias values. In the demo, the weights and biases are set to dummy values of 0.01, 0.02, . . . , 0.53. 
     * The three inputs are arbitrarily set to 1.0, 2.0 and 3.0. Behind the scenes, 
     * the neural network uses the hyperbolic tangent activation function when computing the outputs of the two hidden layers,
     * and the softmax activation function when computing the final output values. The two output values are 0.4881 and 0.5119.
     * http://visualstudiomagazine.com/articles/2014/06/01/deep-neural-networks.aspx
     * */
    public class DnnStaticTwoHl
    {
        //using 1 input layer,2 hidden layer and 1 output layer
        private int numInput;
        private int numFirstHidden;
        private int numSecondHidden;
        private int numOutput;

        //value inside input neuron
        private double[] inputs;

        //get weight value for each network
        private double[][] inputToFhWeight;
        private double[][] fhToShWeight;
        private double[][] shToOutWeight;

        private double[] fhBias;
        private double[] shBias;
        private double[] outBias;

        //value after input processed
        private double[] fhOutputs;
        private double[] shOutputs;
        private double[] finalOutputs;

        //set random variable, sometimes usefull for generating random weight
        private static Random rnd;

        //number all weight
        private int numWeights;

        //constructor
        public DnnStaticTwoHl(int numInput,int numFirstHidden, int numSecondHidden, int numOutput)
        {
            //make instance of all variable
            this.numInput = numInput;
            this.numFirstHidden = numFirstHidden;
            this.numSecondHidden = numSecondHidden;
            this.numOutput = numOutput;

            inputs = new double[numInput];

            inputToFhWeight = MakeMatrix(numInput, numFirstHidden);
            fhToShWeight = MakeMatrix(numFirstHidden, numSecondHidden);
            shToOutWeight = MakeMatrix(numSecondHidden, numOutput);

            fhBias = new double[numFirstHidden];
            shBias = new double[numSecondHidden];
            outBias = new double[numOutput];

            fhOutputs = new double[numFirstHidden];
            shOutputs = new double[numSecondHidden];
            finalOutputs = new double[numOutput];

            rnd = new Random(0);
            this.numWeights = (numInput * numFirstHidden) + numFirstHidden + (numFirstHidden * numSecondHidden) + numSecondHidden + (numSecondHidden * numOutput) + numOutput;

            InitWeight();
        }

        //making metrices of network
        private static double[][] MakeMatrix(int rows, int cols) // helper for ctor
        {
            double[][] result = new double[rows][];
            for (int r = 0; r < result.Length; ++r)
                result[r] = new double[cols];
            return result;
        }

        //helper for random range
        private double RandomRange(double min, double max)
        {
            return (min - max) * rnd.NextDouble() + min; 
        }

        //initialize all wight value;
        private void InitWeight()
        {
            double[] weight = new double[numWeights];
            double min = 0.0001;
            double max = 0.001;
            for (int i = 0; i < weight.Length; ++i)
                weight[i] = RandomRange(min, max);

            this.SetWeights(weight);
        }

        //store all weight for each neuron
        public void SetWeights(double[] weights)
        {
           if (weights.Length != numWeights)
                throw new Exception("Bad weights length");

            int k = 0;

            for (int i = 0; i < numInput; ++i)
                for (int j = 0; j < numFirstHidden; ++j)
                    inputToFhWeight[i][j] = weights[k++];

            for (int i = 0; i < numFirstHidden; ++i)
                fhBias[i] = weights[k++];

            for (int i = 0; i < numFirstHidden; ++i)
                for (int j = 0; j < numSecondHidden; ++j)
                    fhToShWeight[i][j] = weights[k++];

            for (int i = 0; i < numSecondHidden; ++i)
                shBias[i] = weights[k++];

            for (int i = 0; i < numSecondHidden; ++i)
                for (int j = 0; j < numOutput; ++j)
                    shToOutWeight[i][j] = weights[k++];

            for (int i = 0; i < numOutput; ++i)
                outBias[i] = weights[k++];
        }

        //get all weight for each neuron
        public double[] GetWeights()
        {
            double[] result = new double[numWeights];
            int k = 0;
            for (int i = 0; i < inputToFhWeight.Length; ++i)
                for (int j = 0; j < inputToFhWeight[0].Length; ++j)
                    result[k++] = inputToFhWeight[i][j];
            for (int i = 0; i < fhBias.Length; ++i)
                result[k++] = fhBias[i];

            for (int i = 0; i < fhToShWeight.Length; ++i)
                for (int j = 0; j < fhToShWeight[0].Length; ++j)
                    result[k++] = fhToShWeight[i][j];
            for (int i = 0; i < shBias.Length; ++i)
                result[k++] = shBias[i];

            for (int i = 0; i < shToOutWeight.Length; ++i)
                for (int j = 0; j < shToOutWeight[0].Length; ++j)
                    result[k++] = shToOutWeight[i][j];

            for (int i = 0; i < outBias.Length; ++i)
                result[k++] = outBias[i];

            return result;
        }
        //training for DNN
        public double[] Train(double[][] trainData, int maxEpochs, double learningRate, double momentum)
        {
            // train using back-prop
            // back-prop specific arrays
            double[][] shoGrads = MakeMatrix(numSecondHidden, numOutput, 0.0); // second-hidden-to-output weight gradients
            double[] shobGrads = new double[numOutput];                   // output bias gradients

            double[][] fhshGrads = MakeMatrix(numFirstHidden, numSecondHidden, 0.0); // first-hidden-to-second-hidden weight gradients
            double[] fhshbGrads = new double[numSecondHidden];                   // second hidden bias gradients

            double[][] ifhGrads = MakeMatrix(numInput, numFirstHidden, 0.0);  // input-to-first-hidden weight gradients
            double[] fhbGrads = new double[numFirstHidden];                   // first hidden bias gradients

            double[] oSignals = new double[numOutput];                  // local gradient output signals - gradients w/o associated input terms
            double[] fhSignals = new double[numFirstHidden];                  // local gradient first hidden node signals
            double[] shSignals = new double[numSecondHidden];                  // local gradient second hidden node signals

            // back-prop momentum specific arrays 
            double[][] ifhPrevWeightsDelta = MakeMatrix(numInput, numFirstHidden, 0.0);
            double[] fhPrevBiasesDelta = new double[numFirstHidden];
            double[][] fhshPrevWeightsDelta = MakeMatrix(numFirstHidden, numSecondHidden, 0.0);
            double[] shPrevBiasesDelta = new double[numSecondHidden];
            double[][] shoPrevWeightsDelta = MakeMatrix(numSecondHidden, numOutput, 0.0);
            double[] oPrevBiasesDelta = new double[numOutput];

            int epoch = 0;
            double[] xValues = new double[numInput]; // inputs
            double[] tValues = new double[numOutput]; // target values
            double derivative = 0.0;
            double errorSignal = 0.0;

            //sequece of data training
            int[] sequence = new int[trainData.Length];
            for (int i = 0; i < sequence.Length; ++i)
                sequence[i] = i;

            int errInterval = maxEpochs / 10; // interval to check error
            while (epoch < maxEpochs)
            {
                ++epoch;
                if (epoch % errInterval == 0 && epoch < maxEpochs)
                {
                    //get error from MSE
                    double trainErr = Error(trainData);
                    Console.WriteLine("epoch = " + epoch + "  MSE error = " +
                      trainErr.ToString("F4"));
                    //Console.ReadLine();
                }

                Shuffle(sequence); // visit each training data in random order
                for (int ii = 0; ii < trainData.Length; ++ii)
                {
                    int idx = sequence[ii];
                    Array.Copy(trainData[idx], xValues, numInput);
                    Array.Copy(trainData[idx], numInput, tValues, 0, numOutput);
                    ComputeOutputs(xValues); // copy xValues in, compute outputs

                    // indices: i = inputs, j1 = first hiddens,j2 = second hiddens, k = outputs

                    // 1. compute output node signals (assumes softmax)
                    for (int k = 0; k < numOutput; ++k)
                    {
                        errorSignal = tValues[k] - finalOutputs[k];  // Wikipedia uses (o-t)
                        derivative = (1 - finalOutputs[k]) * finalOutputs[k]; // for softmax
                        oSignals[k] = errorSignal * derivative;
                    }

                    // 2. compute second-hidden-to-output weight gradients using output signals
                    for (int j2 = 0; j2 < numSecondHidden; ++j2)
                        for (int k = 0; k < numOutput; ++k)
                            shoGrads[j2][k] = oSignals[k] * shOutputs[j2];

                    // 2b. compute output bias gradients using output signals
                    for (int k = 0; k < numOutput; ++k)
                        shobGrads[k] = oSignals[k] * 1.0; // dummy assoc. input value

                    // 3. compute output second-hidden node signals (assumes tanh)
                    for (int j2 = 0; j2 < numSecondHidden; ++j2)
                    {
                        derivative = (1 + shOutputs[j2]) * (1 - shOutputs[j2]); // for tanh
                        double sum = 0.0; // need sums of output signals times second-hidden-to-output weights
                        for (int k = 0; k < numOutput; ++k)
                        {
                            sum += oSignals[k] * shToOutWeight[j2][k]; // represents error signal
                        }
                        shSignals[j2] = derivative * sum;
                    }

                    // 4. compute first-hidden-to-second-hidden weight gradients using output signals
                    for (int j1 = 0; j1 < numFirstHidden; ++j1)
                        for (int j2 = 0; j2 < numSecondHidden; ++j2)
                            fhshGrads[j1][j2] = shSignals[j2] * fhOutputs[j1];

                    // 4b. compute second-hidden bias gradients using output signals
                    for (int j2 = 0; j2 < numSecondHidden; ++j2)
                        fhshbGrads[j2] = shSignals[j2] * 1.0; // dummy assoc. input value

                    // 5. compute  output first-hidden hidden node signals (assumes tanh)
                    for (int j1 = 0; j1 < numFirstHidden; ++j1)
                    {
                        derivative = (1 + fhOutputs[j1]) * (1 - fhOutputs[j1]); // for tanh
                        double sum = 0.0; // need sums of output signals times hidden-to-output weights
                        for (int j2 = 0; j2 < numSecondHidden; ++j2)
                        {
                            sum += shSignals[j2] * fhToShWeight[j1][j2]; // represents error signal
                        }
                        fhSignals[j1] = derivative * sum;
                    }

                    // 6. compute input-first hidden weight gradients
                    for (int i = 0; i < numInput; ++i)
                        for (int j1 = 0; j1 < numFirstHidden; ++j1)
                            ifhGrads[i][j1] = fhSignals[j1] * inputs[i];

                    // 7b. compute first-hidden node bias gradients
                    for (int j1 = 0; j1 < numFirstHidden; ++j1)
                        fhbGrads[j1] = fhSignals[j1] * 1.0; // dummy 1.0 input

                    // == update weights and biases ===============/

                    // update input-to-first-hidden weights
                    for (int i = 0; i < numInput; ++i)
                    {
                        for (int j1 = 0; j1 < numFirstHidden; ++j1)
                        {
                            double delta = ifhGrads[i][j1] * learningRate;
                            inputToFhWeight[i][j1] += delta; // would be -= if (o-t)
                            inputToFhWeight[i][j1] += ifhPrevWeightsDelta[i][j1] * momentum;
                            ifhPrevWeightsDelta[i][j1] = delta; // save for next time
                        }
                    }

                    // update first-hidden biases
                    for (int j1 = 0; j1 < numFirstHidden; ++j1)
                    {
                        double delta = fhbGrads[j1] * learningRate;
                        fhBias[j1] += delta;
                        fhBias[j1] += fhPrevBiasesDelta[j1] * momentum;
                        fhPrevBiasesDelta[j1] = delta;
                    }

                    // update first-hidden-to-second-hidden weights
                    for (int j1 = 0; j1 < numFirstHidden; ++j1)
                    {
                        for (int j2 = 0; j2 < numSecondHidden; ++j2)
                        {
                            double delta = fhshGrads[j1][j2] * learningRate;
                            fhToShWeight[j1][j2] += delta;
                            fhToShWeight[j1][j2] += fhshPrevWeightsDelta[j1][j2] * momentum;
                            fhshPrevWeightsDelta[j1][j2] = delta;
                        }
                    }

                    // update second-hidden biases
                    for (int j2 = 0; j2 < numSecondHidden; ++j2)
                    {
                        double delta = fhshbGrads[j2] * learningRate;
                        shBias[j2] += delta;
                        shBias[j2] += shPrevBiasesDelta[j2] * momentum;
                        shPrevBiasesDelta[j2] = delta;
                    }

                    // update second-hidden-to-output weights
                    for (int j2 = 0; j2 < numSecondHidden; ++j2)
                    {
                        for (int k = 0; k < numOutput; ++k)
                        {
                            double delta = shoGrads[j2][k] * learningRate;
                            shToOutWeight[j2][k] += delta;
                            shToOutWeight[j2][k] += shoPrevWeightsDelta[j2][k] * momentum;
                            shoPrevWeightsDelta[j2][k] = delta;
                        }
                    }

                    // update output node biases
                    for (int k = 0; k < numOutput; ++k)
                    {
                        double delta = shobGrads[k] * learningRate;
                        outBias[k] += delta;
                        outBias[k] += oPrevBiasesDelta[k] * momentum;
                        oPrevBiasesDelta[k] = delta;
                    }

                } // each training item
            }// while
            double[] bestWts = GetWeights();
            return bestWts;
        }
        
        //computeoutput
        public double[] ComputeOutputs(double[] xValues)
        {
            double[] firstSums = new double[numFirstHidden]; // first hidden nodes sums scratch array
            double[] secondSums = new double[numSecondHidden]; // second hidden nodes sums scratch array
            double[] outSums = new double[numOutput]; // output nodes sums

            for (int i = 0; i < xValues.Length; ++i) // copy x-values to inputs
                this.inputs[i] = xValues[i];

            for (int j = 0; j < numFirstHidden; ++j)  // compute sum of (ia) weights * inputs
                for (int i = 0; i < numInput; ++i)
                    firstSums[j] += this.inputs[i] * this.inputToFhWeight[i][j]; // note +=

            for (int i = 0; i < numFirstHidden; ++i)  // add biases to a sums
                firstSums[i] += this.fhBias[i];

            //Console.WriteLine("\nInternal aSums:");
            //ShowVector(firstSums, firstSums.Length, 4, true);

            for (int i = 0; i < numFirstHidden; ++i)   // apply activation
                this.fhOutputs[i] = HyperTanFunction(firstSums[i]); // hard-coded

           // Console.WriteLine("\nInternal first hidden Outputs:");
            //ShowVector(fhOutputs, fhOutputs.Length, 4, true);

            for (int j = 0; j < numSecondHidden; ++j)  // compute sum of (ab) weights * a outputs = local inputs
                for (int i = 0; i < numFirstHidden; ++i)
                    secondSums[j] += fhOutputs[i] * this.fhToShWeight[i][j]; // note +=

            for (int i = 0; i < numSecondHidden; ++i)  // add biases to b sums
                secondSums[i] += this.shBias[i];

            //Console.WriteLine("\nInternal bSums:");
            //ShowVector(secondSums, secondSums.Length, 4, true);

            for (int i = 0; i < numSecondHidden; ++i)   // apply activation
                this.shOutputs[i] = HyperTanFunction(secondSums[i]); // hard-coded

           // Console.WriteLine("\nInternal bOutputs:");
          //  ShowVector(shOutputs, shOutputs.Length, 4, true);

            for (int j = 0; j < numOutput; ++j)   // compute sum of (bo) weights * b outputs = local inputs
                for (int i = 0; i < numSecondHidden; ++i)
                    outSums[j] += shOutputs[i] * shToOutWeight[i][j];

            for (int i = 0; i < numOutput; ++i)  // add biases to input-to-hidden sums
                outSums[i] += outBias[i];

           // Console.WriteLine("\nInternal oSums:");
           // ShowVector(outSums, outSums.Length, 4, true);

            double[] softOut = Softmax(outSums); // softmax activation does all outputs at once for efficiency
            Array.Copy(softOut, finalOutputs, softOut.Length);

            double[] retResult = new double[numOutput]; // could define a GetOutputs method instead
            Array.Copy(this.finalOutputs, retResult, retResult.Length);
            return retResult;

        }

         private static double SigmoidFunction(double x)
        {
            if (x < -20.0) return 0.0;
            else if (x > 20.0) return 1.0;
            else return 1.0 / (1.0 + Math.Exp(-x));
        }

        //Hyperbolic Tangent Activation Function
        private static double HyperTanFunction(double x)
        {
            if (x < -20.0) return -1.0; // hardcoded approximation is correct to 30 decimals
            else if (x > 20.0) return 1.0;
            else return Math.Tanh(x);
        }

        //Softmax activation function
        private static double[] Softmax(double[] oSums)
        {
            // determine max output sum
            // does all output nodes at once so scale doesn't have to be re-computed each time
            double max = oSums[0];
            for (int i = 0; i < oSums.Length; ++i)
                if (oSums[i] > max) max = oSums[i];

            // determine scaling factor -- sum of exp(each val - max)
            double scale = 0.0;
            for (int i = 0; i < oSums.Length; ++i)
                scale += Math.Exp(oSums[i] - max);

            double[] result = new double[oSums.Length];
            for (int i = 0; i < oSums.Length; ++i)
                result[i] = Math.Exp(oSums[i] - max) / scale;

            return result; // now scaled so that xi sum to 1.0
        }

        //all helper function method
        static public void ShowVector(double[] vector, int valsPerRow, int decimals, bool newLine)
        {
            for (int i = 0; i < vector.Length; ++i)
            {
                if (i % valsPerRow == 0) Console.WriteLine("");
                Console.Write(vector[i].ToString("F" + decimals).PadLeft(decimals + 4) + " ");
            }
            if (newLine == true) Console.WriteLine("");
        }

        //helper for creating matrix
        private static double[][] MakeMatrix(int rows,
         int cols, double v) // helper for ctor, Train
        {
            double[][] result = new double[rows][];
            for (int r = 0; r < result.Length; ++r)
                result[r] = new double[cols];
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    result[i][j] = v;
            return result;
        }

        //mean squared error
        private double Error(double[][] trainData)
        {
            // average squared error per training item
            double sumSquaredError = 0.0;
            double[] xValues = new double[numInput]; // first numInput values in trainData
            double[] tValues = new double[numOutput]; // last numOutput values

            // walk thru each training case. looks like (6.9 3.2 5.7 2.3) (0 0 1)
            for (int i = 0; i < trainData.Length; ++i)
            {
                Array.Copy(trainData[i], xValues, numInput);
                Array.Copy(trainData[i], numInput, tValues, 0, numOutput); // get target values
                double[] yValues = this.ComputeOutputs(xValues); // outputs using current weights
                for (int j = 0; j < numOutput; ++j)
                {
                    double err = tValues[j] - yValues[j];
                    sumSquaredError += err * err;
                }
            }
            return sumSquaredError / trainData.Length;
        } // MeanSquaredError

        //random order of sequence
        private void Shuffle(int[] sequence) // instance method
        {
            for (int i = 0; i < sequence.Length; ++i)
            {
                int r = rnd.Next(i, sequence.Length);
                int tmp = sequence[r];
                sequence[r] = sequence[i];
                sequence[i] = tmp;
            }
        } // Shuffle

        public double Accuracy(double[][] testData)
        {
            // percentage correct using winner-takes all
            int numCorrect = 0;
            int numWrong = 0;
            double[] xValues = new double[numInput]; // inputs
            double[] tValues = new double[numOutput]; // targets
            double[] yValues; // computed Y

            for (int i = 0; i < testData.Length; ++i)
            {
                Array.Copy(testData[i], xValues, numInput); // get x-values
                Array.Copy(testData[i], numInput, tValues, 0, numOutput); // get t-values
                yValues = this.ComputeOutputs(xValues);
                int maxIndex = MaxIndex(yValues); // which cell in yValues has largest value?
                int tMaxIndex = MaxIndex(tValues);

                if (maxIndex == tMaxIndex)
                    ++numCorrect;
                else
                    ++numWrong;
            }
            return (numCorrect * 1.0) / (numCorrect + numWrong);
        }

        private static int MaxIndex(double[] vector) // helper for Accuracy()
        {
            // index of largest value
            int bigIndex = 0;
            double biggestVal = vector[0];
            for (int i = 0; i < vector.Length; ++i)
            {
                if (vector[i] > biggestVal)
                {
                    biggestVal = vector[i];
                    bigIndex = i;
                }
            }
            return bigIndex;
        }
    }
}
