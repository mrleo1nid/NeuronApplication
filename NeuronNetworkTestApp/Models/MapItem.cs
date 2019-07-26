using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronNetworkTestApp.Models
{
    public class MapItem
    {
        public Guid ID { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string Icon { get; set; }

        public MapItemType MapItemType { get; set; }

        public MapItem(Guid id, double x, double y, MapItemType type = MapItemType.FreePlace)
        {
            ID = id;
            X = x;
            Y = y;
            MapItemType = type;
            switch (MapItemType)
            {
                case MapItemType.FreePlace:
                    Icon = " ";
                    break;
                case MapItemType.Player:
                    Icon = "P";
                    break;
                case MapItemType.Wall:
                    Icon = "#";
                    break;
                case MapItemType.Finish:
                    Icon = "F";
                    break;
            }
        }
    }
}
