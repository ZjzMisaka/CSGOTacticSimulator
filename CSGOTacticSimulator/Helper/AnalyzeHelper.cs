using CSGOTacticSimulator.Global;
using Steamworks.ServerList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CSGOTacticSimulator.Helper
{
    public static class AnalyzeHelper
    {
        public static Point DemoPointToMapPoint(DemoInfo.Vector demoPoint, string mapName)
        {
            string[] splitListStr = null;
            string[] mapCalibrationDatas = GlobalDictionary.mapCalibrationDatas;
            foreach (string mapData in mapCalibrationDatas)
            {
                string[] mapCalibrationData = mapData.Split(':');
                if (mapName.ToLowerInvariant().Contains(mapCalibrationData[0].ToLowerInvariant()))
                {
                    string mapInfo;
                    if (mapCalibrationData.Count() >= 2)
                    {
                        mapInfo = mapCalibrationData[1];
                    }
                    else
                    {
                        mapInfo = mapCalibrationData[0];
                    }
                    splitListStr = mapInfo.Split(',');
                    break;
                }
            }

            double[] splitList = new double[3];
            for (int i = 0; i < splitListStr.Count(); ++i)
            {
                splitList[i] = double.Parse(splitListStr[i]);
            }
            return new Point((demoPoint.X - splitList[0]) / splitList[2], (demoPoint.Y * (-1) + splitList[1]) / splitList[2]);
        }
    }
}
