using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
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

        public static List<MapItem> CreateRandomWall(List<MapItem> map, int count, int mapSize)
        {
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                map = ChangeItemType(map, random.Next(0,mapSize-1), random.Next(0, mapSize - 1), MapItemType.Wall);
            }
            return map;
        }
        public static List<MapItem> CreateRandomFinish(List<MapItem> map, int mapSize)
        {
            Random random = new Random();
            var res = true;
            while (res)
            {
                var x = random.Next(1, mapSize - 2);
                var y = random.Next(1, mapSize - 2);
                var item = Moving.GetItemFromCoord(map, x, y);
                if (item.MapItemType==MapItemType.FreePlace)
                {
                    map = ChangeItemType(map, x, y, MapItemType.Finish);
                    res = false;
                }
                else
                {
                    res = true;
                }
            }
            return map;
        }
    }
}
