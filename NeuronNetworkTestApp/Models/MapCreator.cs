using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuronNetworkTestApp.Models
{
    public static class MapCreator
    {
        public static List<MapItem> CreateDefaultMap(int mapSize)
        {
            List<MapItem> list = new List<MapItem>();
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (i==0 || i==mapSize-1)
                    {
                        MapItem item = new MapItem(Guid.NewGuid(), i,j,MapItemType.Wall);
                        list.Add(item);
                    }
                    else if (i !=0 && j==0)
                    {
                        MapItem item = new MapItem(Guid.NewGuid(), i, j, MapItemType.Wall);
                        list.Add(item);
                    }
                    else if (i != 0 && j == mapSize-1)
                    {
                        MapItem item = new MapItem(Guid.NewGuid(), i, j, MapItemType.Wall);
                        list.Add(item);
                    }
                    else
                    {
                        MapItem item = new MapItem(Guid.NewGuid(), i, j, MapItemType.FreePlace);
                        list.Add(item);
                    }
                }
            }
            return list;
        }

        public static List<MapItem> ChangeItemType(List<MapItem> itemslist, int x, int y, MapItemType type = MapItemType.FreePlace)
        {
            var element = itemslist.Where(item => item.X == x && item.Y == y).FirstOrDefault();
            if (element!=null)
            {
                itemslist.Remove(element);
                MapItem item = new MapItem(Guid.NewGuid(),x,y,type);
                itemslist.Add(item);
            }
            return itemslist;
        }
    }
}
