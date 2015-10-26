using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.ML;
using Emgu.CV.ML.MlEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Search4Match.PrepSearcher
{
    public class Prep_BOFMaster
    {/// <summary>
     /// Generating a two-level Bag-Of-Feature SVM.
     /// </summary>
        #region params
        string prefix;
        float[] labels;
        List<string> trainingSet = new List<string>();
        int L1number = 0;
        List<Prep_L2BOF> secondbof = new List<Prep_L2BOF>();
        Prep_L1BOF LayerOneBag;
        #endregion
        public Prep_BOFMaster(string loc)
        ///<summary>calculate class number</summary>
        ///<param name="loc">location of the image folders(place where folder"c-x" is stored).)</param>
        {
            prefix = loc;
            L1number = new DirectoryInfo(prefix).GetDirectories().Length-1;
            labels = new float[L1number];
            //allocate uris
            for (int i = 0; i < L1number; i++)
            {
                labels[i] = i + 1;
                trainingSet.Add(prefix + "c" + labels[i]);
            }
            Console.WriteLine("Generating Layer2 SVM");
            //train layer2 bofs, add to list
            foreach (string src in trainingSet)
            {

                Prep_L2BOF bag;
                int clusternum2 = 50;
                l1:
                try
                {
                    bag = null;
                    bag = new Prep_L2BOF(clusternum2, src);
                    bag.TrainEach(src);
                    secondbof.Add(bag);
                    bag.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("trying k = " + (++clusternum2));
                    goto l1;
                }
            }
            Console.WriteLine("\nGenerating Layer1 SVM");

            //train layer1 bof.
            LayerOneBag = null;
            int clusternum1 = 33;
            l2:
            try
            {
                LayerOneBag = null;
                LayerOneBag = new Prep_L1BOF(clusternum1,prefix);
                LayerOneBag.TrainByClass(trainingSet, labels);
            }
            catch (Exception ex)
            {
                Console.WriteLine("trying k = " + (++clusternum1));
                goto l2;
            }

            LayerOneBag.Save();
        }
        public string BOFPredict(Image<Bgr, byte> target)
        {///<summary>tell which image in the refs are the most similar one to the input image.</summary>
         ///<param name="target">image to search</param>
            float classid = LayerOneBag.L1Predict(target);
            float id = secondbof[Convert.ToInt32(classid) - 1].L2Predict(target);
            string result = classid.ToString() + "-" + id.ToString();
            return result;
        }

    }


}
