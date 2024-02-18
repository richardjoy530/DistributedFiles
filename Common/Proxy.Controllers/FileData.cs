using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Proxy.Controllers
{
    public class FileData
    {
        public required string FileName { get; set; }

        public required byte[] Content { get; set; }
    }
}
