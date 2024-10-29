namespace UndirectedGraphConnectivityAnalyzer.Models
{
    public class Link
    {
        public int Id { get; set; }
        public Node[] Nodes { get; set;} = new Node[2];
        int ConnectivityComponent { get; set; } = 0;

        public Link (Node nodeLeft, Node nodeRight, int id)
        {
            Nodes[0] = nodeLeft;
            Nodes[1] = nodeRight;
            Id = id;
        }
    }
}
