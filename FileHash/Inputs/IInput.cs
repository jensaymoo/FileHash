using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileHash.Inputs
{
    internal interface IInput
    {
        long GetBytesCount();
        Task<byte[]> GetNextBatchBytesAsync();
    }
}
