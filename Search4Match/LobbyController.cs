using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Search4Match
{
    public class LobbyController
    {
        private Image<Gray, byte> model;
        private readonly Matcher.SurfProcessor _cpu = new Matcher.SurfProcessor();
        private double _area;
        private readonly int _areathreshold = 10000;
        private Point _center;
        private Matcher.CenterPositionChecker _centerQ;
        private Matcher.StatusQueueChecker _statusQ;
        private double _flag = -5;
        public Image<Bgr, byte> Result { get; set; }
        public Image<Bgr, byte> GetImg()
        {
            return Result;
        }
        public void SelectRef(Image<Bgr, byte> modelcandidate)
        {
            model = new Image<Gray, byte>( modelcandidate.ToBitmap());
            _statusQ = new Matcher.StatusQueueChecker(4);
            _centerQ = new Matcher.CenterPositionChecker(10, 600, 400);
        }
        public bool CheckMatch(Image<Bgr, byte> queryframe)
        {
            Image<Gray, byte> mG = new Image<Gray, byte>(model.ToBitmap());
            Image<Gray, byte> oG = new Image<Gray, byte>(queryframe.ToBitmap());

            Image<Bgr, byte> res = _cpu.DrawResult(mG, oG, out _area, _areathreshold, out _center);
            Result = res;
            // The matching result already comes out now, but I need to make it more stable
            // And trigger the stop signal at the right time.
            _statusQ.EnQ(_area);
            return _statusQ.CheckMatch(500);
        }
        public string CheckDirection(Image<Bgr, byte> queryframe)
        {
            _centerQ.EnQ(_center);
            string result = _centerQ.CheckPosition();
            return result;
        }
        public bool DetectStop()
        {
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
                // anbd there are enough matching area with the proper size
                // which shows the object is close enough to the camera.
                return true;
            }
            return false;
        }
    }
}
