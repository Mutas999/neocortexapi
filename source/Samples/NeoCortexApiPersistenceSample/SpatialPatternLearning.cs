﻿using NeoCortexApi.Encoders;
using NeoCortexApi.Entities;
using NeoCortexApi.Network;
using NeoCortexApi.Utility;
using NeoCortexApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCortexApiPersistenceSample
{
    public class SpatialPatternLearning
    {
        public CortexLayer<object, object> Initialize(double max, List<double> inputValues)
        {
            // Used as a boosting parameters
            // that ensure homeostatic plasticity effect.
            double minOctOverlapCycles = 1.0;
            double maxBoost = 5.0;

            // We will use 200 bits to represent an input vector (pattern).
            int inputBits = 200;

            // We will build a slice of the cortex with the given number of mini-columns
            int numColumns = 2048;

            //
            // This is a set of configuration parameters used in the experiment.
            HtmConfig cfg = new HtmConfig(new int[] { inputBits }, new int[] { numColumns })
            {
                CellsPerColumn = 10,
                MaxBoost = maxBoost,
                DutyCyclePeriod = 100,
                MinPctOverlapDutyCycles = minOctOverlapCycles,

                GlobalInhibition = false,
                NumActiveColumnsPerInhArea = 0.02 * numColumns,
                PotentialRadius = (int)(0.15 * inputBits),
                LocalAreaDensity = -1,
                ActivationThreshold = 10,

                MaxSynapsesPerSegment = (int)(0.01 * numColumns),
                Random = new ThreadSafeRandom(42),
                StimulusThreshold = 10,
            };

            //
            // This dictionary defines a set of typical encoder parameters.
            Dictionary<string, object> settings = new Dictionary<string, object>()
            {
                { "W", 15},
                { "N", inputBits},
                { "Radius", -1.0},
                { "MinVal", 0.0},
                { "Periodic", false},
                { "Name", "scalar"},
                { "ClipInput", false},
                { "MaxVal", max}
            };


            EncoderBase encoder = new ScalarEncoder(settings);

            // Creates the htm memory.
            var mem = new Connections(cfg);

            //bool isInStableState = false;

            //
            // HPC extends the default Spatial Pooler algorithm.
            // The purpose of HPC is to set the SP in the new-born stage at the begining of the learning process.
            // In this stage the boosting is very active, but the SP behaves instable. After this stage is over
            // (defined by the second argument) the HPC is controlling the learning process of the SP.
            // Once the SDR generated for every input gets stable, the HPC will fire event that notifies your code
            // that SP is stable now.
            HomeostaticPlasticityController hpa = new HomeostaticPlasticityController(mem, inputValues.Count * 40,
                //(isStable, numPatterns, actColAvg, seenInputs) =>
                //{
                //    // Event should only be fired when entering the stable state.
                //    // Ideal SP should never enter unstable state after stable state.
                //    if (isStable == false)
                //    {
                //        Debug.WriteLine($"INSTABLE STATE");
                //        // This should usually not happen.
                //        isInStableState = false;
                //    }
                //    else
                //    {
                //        Debug.WriteLine($"STABLE STATE");
                //        // Here you can perform any action if required.
                //        isInStableState = true;
                //    }
                //});
            null);

            // It creates the instance of Spatial Pooler Multithreaded version.
            SpatialPooler sp = new SpatialPooler(hpa);

            // Initializes the 
            sp.Init(mem, new DistributedMemory() { ColumnDictionary = new InMemoryDistributedDictionary<int, NeoCortexApi.Entities.Column>(1) });

            // mem.TraceProximalDendritePotential(true);

            // It creates the instance of the neo-cortex layer.
            // Algorithm will be performed inside of that layer.
            CortexLayer<object, object> cortexLayer = new CortexLayer<object, object>("L1");

            // Add encoder as the very first module. This model is connected to the sensory input cells
            // that receive the input. Encoder will receive the input and forward the encoded signal
            // to the next module.
            cortexLayer.HtmModules.Add("encoder", encoder);

            // The next module in the layer is Spatial Pooler. This module will receive the output of the
            // encoder.
            cortexLayer.HtmModules.Add("sp", sp);

            return cortexLayer;
        }
        
        //public CortexLayer<object, object> Train(double max, List<double> inputValues)
        //{
            

        //    cortexLayer.Train(inputValues, 1000, "sp");

        //    //double[] inputs = inputValues.ToArray();

        //    //// Will hold the SDR of every inputs.
        //    //Dictionary<double, int[]> prevActiveCols = new Dictionary<double, int[]>();

        //    //// Will hold the similarity of SDKk and SDRk-1 fro every input.
        //    //Dictionary<double, double> prevSimilarity = new Dictionary<double, double>();

        //    ////
        //    //// Initiaize start similarity to zero.
        //    //foreach (var input in inputs)
        //    //{
        //    //    prevSimilarity.Add(input, 0.0);
        //    //    prevActiveCols.Add(input, new int[0]);
        //    //}

        //    //// Learning process will take 1000 iterations (cycles)
        //    //int maxSPLearningCycles = 1000;

        //    //for (int cycle = 0; cycle < maxSPLearningCycles; cycle++)
        //    //{
        //    //    Debug.WriteLine($"Cycle  ** {cycle} ** Stability: {isInStableState}");

        //    //    //
        //    //    // This trains the layer on input pattern.
        //    //    foreach (var input in inputs)
        //    //    {
        //    //        double similarity;

        //    //        // Learn the input pattern.
        //    //        // Output lyrOut is the output of the last module in the layer.
        //    //        // 
        //    //        var lyrOut = cortexLayer.Compute((object)input, true) as int[];

        //    //        // This is a general way to get the SpatialPooler result from the layer.
        //    //        var activeColumns = cortexLayer.GetResult("sp") as int[];

        //    //        var actCols = activeColumns.OrderBy(c => c).ToArray();

        //    //        similarity = MathHelpers.CalcArraySimilarity(activeColumns, prevActiveCols[input]);

        //    //        Debug.WriteLine($"[cycle={cycle.ToString("D4")}, i={input}, cols=:{actCols.Length} s={similarity}] SDR: {Helpers.StringifyVector(actCols)}");

        //    //        prevActiveCols[input] = activeColumns;
        //    //        prevSimilarity[input] = similarity;
        //    //    }

        //    //    if (isInStableState)
        //    //        break;
        //    //}

        //    return cortexLayer;
        //}
    }
}
