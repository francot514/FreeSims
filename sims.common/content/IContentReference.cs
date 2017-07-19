using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TSO.Common.content
{
    public interface IContentReference <T>
    {
        T Get();
    }

    public interface IContentReference
    {
        object GetGeneric();
        object GetThrowawayGeneric();
    }

}
