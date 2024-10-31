using ReactiveUI;

namespace UndirectedGraphConnectivityAnalyzer.Models
{
    public class Link : ReactiveObject
    {
        private int connectivityComponent;

        public int Id { get; set; }
        public Node[] Nodes { get; set; } = new Node[2];
        public int ConnectivityComponent
        {
            get => connectivityComponent;
            set => this.RaiseAndSetIfChanged(ref connectivityComponent, value);
        }

        public Link(Node nodeLeft, Node nodeRight, int id)
        {
            Nodes[0] = nodeLeft;
            Nodes[1] = nodeRight;
            Id = id;
            ConnectivityComponent = 0;
        }
    }
}
