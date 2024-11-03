using ReactiveUI;
using System.Collections.ObjectModel;

namespace UndirectedGraphConnectivityAnalyzer.Models
{
    public class Link : ReactiveObject
    {
        private int _connectivityComponent;

        public int Id { get; set; }
        public ObservableCollection<Node> Nodes { get; set; }
        public int ConnectivityComponent
        {
            get => _connectivityComponent;
            set => this.RaiseAndSetIfChanged(ref _connectivityComponent, value);
        }

        public Link(Node nodeLeft, Node nodeRight, int id)
        {
            ConnectivityComponent = 0;
            Id = id;
            Nodes = new ObservableCollection<Node>(new Node[2]);
            Nodes[0] = nodeLeft;
            Nodes[1] = nodeRight;
        }

        public static void BindLinks(ObservableCollection<Node> nodes, ObservableCollection<Link> links)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                for (var j = 0; j < links.Count; j++)
                {
                    if (nodes[i].Name == links[j].Nodes[0].Name)
                        links[j].Nodes[0] = nodes[i];
                    if (nodes[i].Name == links[j].Nodes[1].Name)
                        links[j].Nodes[1] = nodes[i];
                }
            }
        }
        public static void UnbindLinks(ObservableCollection<Link> links)
        {
            for (var i = 0; i < links.Count; i++)
            {
                links[i].Nodes[0] = new Node(0, links[i].Nodes[0].Name);
                links[i].Nodes[1] = new Node(0, links[i].Nodes[1].Name);
                links[i].ConnectivityComponent = 0;
            }
        }
    }
}
