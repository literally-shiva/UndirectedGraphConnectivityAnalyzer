using System.Collections.Generic;

namespace UndirectedGraphConnectivityAnalyzer.Models
{
    public class Node
    {
        public int Id { get; set; }
        public string Name { get; set; }
        List<Link> Links { get; set; }
        int ConnectivityComponent { get; set; } = 0;

        public Node(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public void AddLink(Link link)
        {
            this.Links.Add(link);
        }

        public static List<List<Node>> GetConnectedComponents(List<Node> nodes)
        {
            int connectivityComponent = 1;
            List<List<Node>> components = new List<List<Node>>();

            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].ConnectivityComponent == 0)
                {
                    List<Node> visitedNodes = new List<Node>();
                    Queue<Node> nodeQueue = new Queue<Node>();
                    nodeQueue.Enqueue(nodes[i]);
                    visitedNodes.Add(nodes[i]);
                    nodes[i].ConnectivityComponent = connectivityComponent;

                    while (nodeQueue.Count > 0)
                    {
                        Node node = nodeQueue.Dequeue();
                        foreach (var link in node.Links)
                        {
                            if (!visitedNodes.Contains(link.Nodes[1]))
                            {
                                link.Nodes[1].ConnectivityComponent = connectivityComponent;
                                visitedNodes.Add(link.Nodes[1]);
                                nodeQueue.Enqueue(link.Nodes[1]);
                            }
                        }
                    }
                    components.Add(visitedNodes);
                    connectivityComponent++;
                }
            }

            return components;
        }
    }
}
