using CSGOTacticSimulator.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace CSGOTacticSimulator.Controls
{
    public class TSImage : Image
    {
        private string tagStr;
        private string type;
        private Point mapPoint;
        private Point startMapPoint;
        private Point endMapPoint;
        private ImgType imgType;

        public string TagStr { get => tagStr; set => tagStr = value; }
        public string Type { get => type; set => type = value; }
        public Point MapPoint { get => mapPoint; set => mapPoint = value; }
        public Point StartMapPoint { get => startMapPoint; set => startMapPoint = value; }
        public Point EndMapPoint { get => endMapPoint; set => endMapPoint = value; }
        public ImgType ImgType { get => imgType; set => imgType = value; }
    }
}
