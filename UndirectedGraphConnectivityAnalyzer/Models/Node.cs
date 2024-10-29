﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UndirectedGraphConnectivityAnalyzer.Models
{
    public class Node
    {
        public int Id { get; set; }
        public string Name { get; set; }
        List<Link> Links { get; set; }
        public int ConnectivityComponent { get; set; }

        public Node(int id, string name)
        {
            Id = id;
            Name = name;
            Links = new List<Link>();
            ConnectivityComponent = 0;
        }

        public void AddLink(Link link)
        {
            this.Links.Add(link);
        }

        public static List<List<Node>> GetConnectedComponents(ObservableCollection<Node> nodes)
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