using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessingSystem.Producer.Options
{
    public class ProducerOptions
    {
        public int MinDelayMilliseconds { get; set; }
        public int MaxDelayMilliseconds { get; set; }
    }
}
