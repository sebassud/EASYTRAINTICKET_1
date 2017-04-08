using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using System;
using System.Collections.Generic;
using System.Data.Linq.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTrainTickets.Desktop.GraphHelpers
{
    public class Path
    {
        private IEasyTrainTicketsDbEntities dbContext;
        public List<Route> path = new List<Route>();

        public int Count
        {
            get
            {
                return path.Count;
            }
        }

        public Path(IEasyTrainTicketsDbEntities _dbContext)
        {
            dbContext = _dbContext;
        }
        public void AddPart(Route route)
        {
            path.Add(route);
        }

        public List<ConnectionPath> SecondSearch(DateTime userTime, IEasyTrainTicketsDbEntities _dbContext)
        {
            List<ConnectionPath> conPaths = new List<ConnectionPath>();
            List<ConnectionPart> startParts = ListOfConnections(0, userTime, _dbContext);
            foreach (var startPart in startParts)
            {
                userTime = startPart.EndTime;
                ConnectionPath conPath = new ConnectionPath();
                conPath.Add(startPart);
                for (int i = 1; i < Count; i++)
                {
                    int minTime = Min(i, userTime, _dbContext);
                    if (minTime == int.MaxValue || minTime > 300 || conPath.Change > 3)
                    {
                        conPath = null;
                        break;
                    }
                    Route route = path[i];
                    var part = _dbContext.ConnectionParts.Where(cp =>
                    cp.Route.Id == route.Id).ToArray();
                    ConnectionPart conpart = part.Where(cp =>
                    SqlMethods.DateDiffMinute(userTime, cp.StartTime) == minTime).First();
                    conPath.Add(conpart);
                    Connection con = conpart.Connection;
                    List<ConnectionPart> list = con.Parts.ToList();
                    int k = list.FindIndex(cp => cp.Id == conpart.Id);
                    while (i + 1 < Count && k + 1 < list.Count && list[k + 1].Route.Id == path[i + 1].Id)
                    {
                        conPath.Add(list[k + 1]);
                        k++;
                        i++;
                    }
                    userTime = list[k].EndTime;
                }
                if (conPath != null)
                {
                    conPath.Initialize();
                    if (TimeSpan.Compare(conPath.EndTime - conPath.StartTime, new TimeSpan(15, 0, 0)) < 0)
                        conPaths.Add(conPath);
                }
            }
            return conPaths;
        }

        private int Min(int i, DateTime userTime, IEasyTrainTicketsDbEntities _dbContext)
        {
            Route route = path[i];
            return _dbContext.ConnectionParts.Where(cp =>
            cp.Route.Id == route.Id).ToList().Min(cp => SqlMethods.DateDiffMinute(userTime, cp.StartTime) >= 0 ? SqlMethods.DateDiffMinute(userTime, cp.StartTime) : int.MaxValue);

        }
        private List<ConnectionPart> ListOfConnections(int i, DateTime userTime, IEasyTrainTicketsDbEntities _dbContext)
        {
            Route route = path[i];
            return _dbContext.ConnectionParts.Where(cp => cp.Route.Id == route.Id && (cp.StartTime.Day == userTime.Day || cp.StartTime.Day == userTime.Day + 1) && cp.StartTime.Month == userTime.Month).ToList().
               Where(cp => SqlMethods.DateDiffMinute(userTime, cp.StartTime) >= 0 && SqlMethods.DateDiffHour(userTime, cp.StartTime) < 20).ToList();
        }
    }
}
