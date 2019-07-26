using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NeuralNetworks;

namespace NeuronNetworkTestApp.Models
{
    public static class Moving
    {
        public static MapItem GetPlayer(List<MapItem> Map)
        {
            var element = Map.Where(item => item.MapItemType == MapItemType.Player).FirstOrDefault();
            if (element != null)
            {
                return element;
            }
            return null;
        }
        public static MapItem GetFinish(List<MapItem> Map)
        {
            var element = Map.Where(item => item.MapItemType == MapItemType.Finish).FirstOrDefault();
            if (element != null)
            {
                return element;
            }
            return null;
        }
        public static double[] GetSensorData(List<MapItem> map, MapItem player)
        {
            double[] result = new double[4];
            //forward
            if (CanMove(player,MoveType.Forward,map))
            {
                result[0] = 1.0;
            }
            else
            {
                result[0] = 0.0;
            }
            //back
            if (CanMove(player, MoveType.Back, map))
            {
                result[1] = 1.0;
            }
            else
            {
                result[1] = 0.0;
            }
            //left
            if (CanMove(player, MoveType.Left, map))
            {
                result[2] = 1.0;
            }
            else
            {
                result[2] = 0.0;
            }
            //right
            if (CanMove(player, MoveType.Right, map))
            {
                result[3] = 1.0;
            }
            else
            {
                result[3] = 0.0;
            }

            return result;
        }
        public static Tuple<double, double> ConvertMovingTypeToCoord(MoveType type, MapItem player)
        {
            switch (type)
            {
                case MoveType.Forward:
                    return new Tuple<double, double>(player.X-1,player.Y);
                case MoveType.Back:
                    return new Tuple<double, double>(player.X + 1, player.Y);
                case MoveType.Left:
                    return new Tuple<double, double>(player.X, player.Y-1);
                case MoveType.Right:
                    return new Tuple<double, double>(player.X, player.Y+1);
                default:
                    return new Tuple<double, double>(player.X, player.Y);
            }
        }
        public static bool CanMove(MapItem player, MoveType moveType, List<MapItem> map)
        {
            var coord = ConvertMovingTypeToCoord(moveType, player);
            var finditem = GetItemFromCoord(map, coord.Item1, coord.Item2);
            if (finditem!=null)
            {
                if (finditem.MapItemType == MapItemType.FreePlace || finditem.MapItemType == MapItemType.Finish)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public static MapItem GetItemFromCoord(List<MapItem> map, double x, double y)
        {
            var element = map.Where(item => item.X == x && item.Y == y).FirstOrDefault();
            if (element != null)
            {
                return element;
            }
            return null;
        }
        public static List<MapItem> Move(MapItem player, MoveType moveType, List<MapItem> map)
        {
            if (CanMove(player,moveType,map))
            {
                var oldX = player.X;
                var oldY = player.Y;
                var newXY = ConvertMovingTypeToCoord(moveType, player);
                map = MapCreator.ChangeItemType(map, oldX, oldY);
                map = MapCreator.ChangeItemType(map, newXY.Item1, newXY.Item2,MapItemType.Player);
                return map;
            }
            return map;
        }
        public static MoveType ConvertBotResultToMove(double result)
        {
            if (result<0.25)
            {
                return MoveType.Forward;
            }
            else if (result<0.5)
            {
                return MoveType.Back;
            }
            else if (result<0.75)
            {
                return MoveType.Left;
            }
            else
            {
                return MoveType.Right;
            }
        }
    }
}
