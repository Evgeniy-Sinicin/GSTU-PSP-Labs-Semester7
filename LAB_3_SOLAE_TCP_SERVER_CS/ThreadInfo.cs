using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAB_3_SOLAE_TCP_SERVER_CS
{
    public class ThreadInfo
    {
        private bool _isWorking = false;

        public bool IsWorking { get => _isWorking; }
        public string Name { get; }

        public ThreadInfo(string name)
        {
            _isWorking = true;
            Name = name;
        }

        public void CompleteWork()
        {
            if (_isWorking)
            {
                _isWorking = false;
            }
        }
    }
}
