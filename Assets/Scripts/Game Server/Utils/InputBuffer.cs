using System.Collections.Generic;

namespace GameServer
{
    public class InputBuffer
    {
        LinkedList<_Input> buffer;

        public InputBuffer()
        {
            buffer = new LinkedList<_Input>();
        }

        public void Add(float horizontal, float vertical)
        {
            buffer.AddLast(new _Input(horizontal, vertical));
        }
        public _Input Get()
        {
            _Input i = buffer.First.Value;
            buffer.RemoveFirst();
            return i;
        }
        public void Clear()
        {
            buffer.Clear();
        }
        public bool hasInput => buffer.Count > 0;
    }

    [System.Serializable]
    public class _Input
    {
        public float horizontal;
        public float vertical;

        public _Input(float horizontal, float vertical)
        {
            this.horizontal = horizontal;
            this.vertical = vertical;
        }
    }
}