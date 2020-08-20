using System;
using System.Collections.Generic;
using System.Linq;

namespace GoGen
{
    class Program
    {
        static void Main()
        {
            var nodes = CreateEmptyBoard();
            nodes[0, 0].Letter = 'P';
            nodes[2, 0].Letter = 'F';
            nodes[4, 0].Letter = 'S';
            nodes[0, 2].Letter = 'Q';
            nodes[2, 2].Letter = 'C';
            nodes[4, 2].Letter = 'R';
            nodes[0, 4].Letter = 'L';
            nodes[2, 4].Letter = 'G';
            nodes[4, 4].Letter = 'Y';

            var words = new List<string>()
            {
                "BIRDS",
                "BREACH",
                "CHUMP",
                "FAXING",
                "JUNK",
                "LOCH",
                "LOQUACITY",
                "WAVER"
            };

            var connections = BuildRequiredConnectionsFromWords(words);

            var allLetters = Enumerable.Range(0, 25)
                                       .Select(i => (char)('A' + i))
                                       .ToList();

            while (Solve(nodes, connections, allLetters))
            {
            //    Display(nodes);
            }

            Display(nodes);
        }

        private static HashSet<string> BuildRequiredConnectionsFromWords(List<string> words)
        {
            var connections = new HashSet<string>();
            foreach (var word in words)
            {
                for (int i = 0; i < word.Length - 1; i++)
                {
                    connections.Add($"{word[i]}-{word[i + 1]}");
                    connections.Add($"{word[i + 1]}-{word[i]}");
                }
            }

            return connections;
        }

        private static Node[,] CreateEmptyBoard()
        {
            var nodes = new Node[5, 5];
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    nodes[c, r] = new Node(c, r);
                }
            }

            return nodes;
        }

        private static void Display(Node[,] nodes)
        {
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    Console.Write(nodes[c, r].Letter); ;
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        private static bool Solve(Node[,] nodes, HashSet<string> connections, List<char> allLetters)
        {
            var solvedLetters = AllSolvedNodes(nodes).Select(n => n.Letter).ToList();

            var remainingLetters = allLetters.Except(solvedLetters).ToList();

            foreach (var node in AllUnsolvedNodes(nodes))
            {
                node.PossibleLetters = remainingLetters.ToList();
            }

            foreach (var letter in remainingLetters)
            {
                foreach (var solvedNode in AllSolvedNodes(nodes))
                {
                    var neighbours = solvedNode.Neighbours(nodes).ToList();

                    // If a connection is needed for solvedNode.Letter to Letter,
                    // then it can only be in a neighour of solvedNode.Letter
                    if (connections.Contains($"{letter}-{solvedNode.Letter}"))
                    {
                        foreach (var node in AllUnsolvedNodes(nodes).Except(neighbours))
                        {
                            node.PossibleLetters.Remove(letter);
                        }
                    }

                    // Same thing but for length 3.
                    //      A-B  B-C   nodes A and C must been neighbours of the neighbours
                    var directConnections = connections.Where(c => c.StartsWith(letter)).Select(c => c[^1]).Distinct().ToList();
                    var neighboursOfneighbours = FindNeighboursOfNeighbours(nodes, neighbours).ToList();
                    foreach (var indirectConnection in directConnections)
                    {
                        if (connections.Contains($"{indirectConnection}-{solvedNode.Letter}"))
                        {
                            foreach (var node in AllUnsolvedNodes(nodes).Except(neighboursOfneighbours))
                            {
                                node.PossibleLetters.Remove(letter);
                            }
                        }
                    }
                }
            }

            // Are there any letters which can only go in one node?
            foreach (var letter in remainingLetters)
            {
                var possibles = AllUnsolvedNodes(nodes).Where(n => n.PossibleLetters.Contains(letter)).ToArray();
                if (possibles.Count() == 1)
                {
                    var solved = possibles.First();
                    Console.WriteLine($"{solved.Column},{solved.Row} : {letter} is the only letter than can go here.");
                    solved.Letter = letter;
                    solved.PossibleLetters.Clear();
                    return true;
                }
            }

            // Are there any nodes where only one letter can go?
            var solveables = AllUnsolvedNodes(nodes).Where(n => n.PossibleLetters.Count() == 1);
            foreach (var solveable in solveables)
            {
                Console.WriteLine($"{solveable.Column},{solveable.Row} : Only place that {solveable.PossibleLetters.First()} can go.");
                solveable.Letter = solveable.PossibleLetters.First();
                solveable.PossibleLetters.Clear();
                return true;
            }

            return false;
        }

        private static IEnumerable<Node> FindNeighboursOfNeighbours(Node[,] nodes, IEnumerable<Node> neighbours)
        {
            return neighbours.SelectMany(n => n.Neighbours(nodes)).Distinct();
        }

        private static IEnumerable<Node> AllUnsolvedNodes(Node[,] nodes)
        {
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    if (!nodes[c, r].Solved)
                    {
                        yield return nodes[c, r];
                    }
                }
            }
        }

        private static IEnumerable<Node> AllSolvedNodes(Node[,] nodes)
        {
            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 5; c++)
                {
                    if (nodes[c, r].Solved)
                    {
                        yield return nodes[c, r];
                    }
                }
            }
        }

    }

    class Node
    {
        public bool Solved => Letter != ' ';
        public char Letter { get; set; } = ' ';
        public List<char> PossibleLetters { get; set; }
        public int Column { get; }
        public int Row { get; }

        public Node(int column, int row)
        {
            Column = column;
            Row = row;
        }
        internal IEnumerable<Node> Neighbours(Node[,] nodes)
        {
            //  123
            //  4 5 
            //  678
            if (Column > 0 && Row > 0) yield return nodes[Column - 1, Row - 1];
            if (Row > 0) yield return nodes[Column, Row - 1];
            if (Column <= 3 && Row > 0) yield return nodes[Column + 1, Row - 1];

            if (Column > 0) yield return nodes[Column - 1, Row];
            if (Column <= 3) yield return nodes[Column + 1, Row];

            if (Column > 0 && Row <= 3) yield return nodes[Column - 1, Row + 1];
            if (Row <= 3) yield return nodes[Column, Row + 1];
            if (Column <= 3 && Row <= 3) yield return nodes[Column + 1, Row + 1];
        }
    }
}
