using System.Collections.Generic;
using System.Linq;

namespace CommonPart
{
    public class SocketsList<T>
    {
        private readonly Dictionary<int, T> _sockets = new Dictionary<int, T>();


        private T[] _all = new T[0];


        public void Add(int socketId, T socket)
        {
            lock (_sockets)
            {
                if (_sockets.ContainsKey(socketId))
                    _sockets[socketId] = socket;
                else
                    _sockets.Add(socketId, socket);


                _all = _sockets.Values.ToArray();
            }
        }


        public T Remove(int socketId)
        {

            var result = default(T);
            
            lock (_sockets)
            {
                try
                {
                    if (_sockets.ContainsKey(socketId))
                    {
                        result = _sockets[socketId];
                        _sockets.Remove(socketId);
                    }

                }
                finally
                {
                    _all = _sockets.Values.ToArray();
                }                
            }


            return result;
        }


        public T Get(int socketId)
        {
            lock (_sockets)
            {
                return _sockets.ContainsKey(socketId) ? _sockets[socketId] : default(T);
            }
        }


        public T[] GetAll()
        {
            return _all;
        }

        public int Count()
        {
            lock(_sockets)
                return _sockets.Count;
        }
        
    }
}