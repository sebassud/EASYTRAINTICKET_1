using EasyTrainTickets.Domain.Data;
using EasyTrainTickets.Domain.Model;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyTrainTickets.Desktop.GraphHelpers
{
    public class Graph
    {
        private IEasyTrainTicketsDbEntities dbocontext;
        private List<Edge>[] matrix;
        private List<string> stations;
        private List<Route> routes;
        private int count;

        private bool[] visited;
        private int actualdistance;
        private Stack<int> stack;
        private int maxdistance;
        private List<List<int>> paths;
        private int target;

        public List<String> Stations
        {
            get
            {
                return stations.OrderBy(s => s).ToList();
            }
        }

        private class Edge
        {
            public int To { get; set; }
            public int Weight { get; set; }

            public Edge(int to, int weight)
            {
                To = to;
                Weight = weight;
            }
        }


        public Graph(IEasyTrainTicketsDbEntities _dbcontext)
        {
            dbocontext = _dbcontext;
            stations = dbocontext.Routes.Select(r => r.From).Distinct().ToList();
            routes = dbocontext.Routes.ToList();
            matrix = new List<Edge>[stations.Count];
            count = stations.Count;

            for (int i = 0; i < count; i++) matrix[i] = new List<Edge>();

            foreach (var r in routes)
            {
                matrix[FromStringToInt(r.From)].Add(new Edge(FromStringToInt(r.To), r.Distance));
            }
        }

        private int Dijkstry(int from, int to)
        {
            int[] distance = new int[count];
            int[] prev = new int[count];
            SimplePriorityQueue<int> queue = new SimplePriorityQueue<int>();

            for (int i = 0; i < count; i++)
            {
                distance[i] = int.MaxValue;
                queue.Enqueue(i, float.MaxValue);
            }
            distance[from] = 0;
            queue.UpdatePriority(from, 0);

            while (queue.Count > 0)
            {
                int v = queue.Dequeue();
                foreach (var e in matrix[v])
                {
                    if (distance[e.To] > distance[v] + e.Weight)
                    {
                        distance[e.To] = distance[v] + e.Weight;
                        prev[e.To] = v;
                        queue.UpdatePriority(e.To, distance[e.To]);
                    }
                }
                if (v == target) break;
            }

            return distance[to];
        }

        public List<Path> SearchPaths(string from, string to)
        {
            List<Path> list = new List<Path>();
            int s = FromStringToInt(from);

            target = FromStringToInt(to);
            maxdistance = (int)(1.35 * Dijkstry(s, target));
            visited = new bool[count];
            stack = new Stack<int>();
            stack.Push(s);
            visited[s] = true;
            actualdistance = 0;
            paths = new List<List<int>>();
            DFS(s);

            foreach (var p in paths)
            {
                list.Add(CreatePath(p));
            }

            return list;
        }

        private void DFS(int cur)
        {
            if (actualdistance > maxdistance) return;
            if (cur == target)
            {
                List<int> list = stack.ToList();
                list.Reverse();
                paths.Add(list);
                return;
            }

            foreach (var e in matrix[cur])
            {
                if (visited[e.To]) continue;
                stack.Push(e.To);
                visited[e.To] = true;
                actualdistance += e.Weight;

                DFS(e.To);

                stack.Pop();
                visited[e.To] = false;
                actualdistance -= e.Weight;
            }
        }

        private Path CreatePath(List<int> list)
        {
            Path path = new Path(dbocontext);

            for (int i = 0; i < list.Count - 1; i++)
            {
                Edge e = matrix[list[i]].Find(p => p.To == list[i + 1]);
                var route = routes.Find(r => r.From == stations[list[i]] && r.To == stations[list[i + 1]]);
                path.AddPart(route);
            }

            return path;
        }

        private int FromStringToInt(string s)
        {
            return stations.FindIndex(p => p == s);
        }
    }
}
