//  __  _  __    __   ___ __  ___ ___
// |  \| |/__\ /' _/ / _//__\| _ \ __|
// | | ' | \/ |`._`.| \_| \/ | v / _|
// |_|\__|\__/ |___/ \__/\__/|_|_\___|
// -----------------------------------

using System.Collections.Generic;

namespace NosCore.PathFinder.Brushfire
{
    internal class MinHeap
    {
        private readonly List<Node> _array = new List<Node>();

        public int Count => _array.Count;

        public Node Pop()
        {
            var ret = _array[0];
            _array[0] = _array[^1];
            _array.RemoveAt(_array.Count - 1);

            var c = 0;
            while (c < _array.Count)
            {
                var min = c;
                if (2 * c + 1 < _array.Count && _array[2 * c + 1].CompareTo(_array[min]) == -1)
                {
                    min = 2 * c + 1;
                }
                if (2 * c + 2 < _array.Count && _array[2 * c + 2].CompareTo(_array[min]) == -1)
                {
                    min = 2 * c + 2;
                }

                if (min == c)
                {
                    break;
                }

                var tmp = _array[c];
                _array[c] = _array[min];
                _array[min] = tmp;
                c = min;
            }

            return ret;
        }

        public void Push(Node element)
        {
            _array.Add(element);
            var c = _array.Count - 1;
            var parent = (c - 1) >> 1;
            while (c > 0 && _array[c].CompareTo(_array[parent]) < 0)
            {
                var tmp = _array[c];
                _array[c] = _array[parent];
                _array[parent] = tmp;
                c = parent;
                parent = (c - 1) >> 1;
            }
        }
    }
}
