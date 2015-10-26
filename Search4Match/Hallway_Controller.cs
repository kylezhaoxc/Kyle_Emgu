using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System;

namespace Search4Match
{
    public class Hallway_Controller : IDisposable
    { ///<summary>Contains 2 modules, the BOF search module and SURF match module.
      ///The Surf module will start when BOF module indicates indexes near the end of image serie.
      ///Stop signal will be sent according to the SURF matched area.</summary>
        string prefix;
        static PrepSearcher.Prep_BOFMaster p_searcher=null;
        static LoadSearcher.Load_BOFMaster l_searcher=null;
        Image<Gray, byte> model;
        private readonly Matcher.SurfProcessor _cpu = new Matcher.SurfProcessor();
        private double _area;
        private readonly int _areathreshold = 500;
        private Point _center;
        private  Matcher.CenterPositionChecker _centerQ;
        private  Matcher.StatusQueueChecker _statusQ;
        private double _flag = -5;
        int finishVote;
        public bool SURF_Started = false;
        public Image<Bgr, byte> Result {  get; set; }
        public static Hallway_Controller GetPrepController(string loc)
            ///<summary>Called when the reference vedio stream is specified, before the robot starts, in order to generate Support Vector Machines and Dictionaries.</summary>
            /// <param name="loc">url where the image series are stored.</param>
        {
            Console.WriteLine("Start Making SVMs");
            Hallway_Controller con = new Hallway_Controller();
            p_searcher= new Search4Match.PrepSearcher.Prep_BOFMaster(loc);l_searcher = null;
            con.prefix = loc;
            return con;
        }
       
        public static Hallway_Controller GetLoadController(string loc)
            ///<summary>Called when the SVMs and dictionaries are prepared and stored, in order to predict and trigger SURF module.</summary>
            /// <param name="loc">url where the image series are stored.</param>
        {
            Console.WriteLine("Loading Prepared SVMs for prediction");
            Hallway_Controller con = new Hallway_Controller();
            int num = loc.Split('_').Length;
            string status = loc.Split('_')[num - 1];
            bool isBack = status == "back\\" ? true : false;
            l_searcher = new LoadSearcher.Load_BOFMaster(isBack);p_searcher = null;
            con.prefix = loc;
            con._statusQ = new Matcher.StatusQueueChecker(4);
            con._centerQ = new Matcher.CenterPositionChecker(10, 600, 400);
            con.finishVote = 0;
            return con;
        }
       
        public string GetDirection(Image<Bgr, byte> queryframe,out string ind)
        ///<param name="ind"> out param, returns the most possible index of the incoming image</param>
        /// 
        /// <param name="queryframe">incoming frame </param>
        {
            string result = "go";
            
            string pred_index = l_searcher.BOFPredict(queryframe);
            ind = pred_index;
            if (!SURF_Started)
            {
                //if surf module
                if (pred_index == "2-9" || pred_index == "2-8" || pred_index == "2-7" || pred_index == "2-6") finishVote += 6;
                else finishVote =finishVote*3/4;
            }
           

            if (finishVote >=5)
            {
                SURF_Started = true;
                #region PossibleEnd
                model = new Image<Gray, byte>(prefix + "c" + "2\\2-9.jpg");
                Image<Gray, byte> mG = new Image<Gray, byte>(model.ToBitmap());
                Image<Gray, byte> oG = new Image<Gray, byte>(queryframe.ToBitmap());

                Image<Bgr, byte> res = _cpu.DrawResult(mG, oG, out _area, _areathreshold, out _center);
                Result = res;
                // The matching result already comes out now, but I need to make it more stable
                // And trigger the stop signal at the right time.

                _statusQ.EnQ(_area);
                if (_statusQ.CheckMatch(_areathreshold))
                {
                    #region PositiveMatch
                    result = "go";
                    //use area
                    if (_area > 125000)
                    {
                        _flag += 1;
                        _flag = _flag > 5 ? 5 : _flag;
                    }
                    else
                    {
                        _flag -= 1;
                        _flag = _flag < -5 ? -5 : _flag;
                    }


                    if (_flag > -3)
                    {
                        // _flag = -5;
                        //this shows there are enough positive match in the queue
                        // and there are enough matching area with the proper size
                        // which shows the object is close enough to the camera.
                        result = "stop";
                    }
                    #endregion
                }
                else
                {
                    #region NoPositiveMatch
                    finishVote = 0;
                    #endregion
                }
                #endregion
            }
            else
            {
                SURF_Started = false;
            }
            return result;
        }

        public void Dispose()
        {
            if (p_searcher != null)
                Console.WriteLine("\n\n SVMs ARE READY NOW!!\n\n");
        }
    }
} 
