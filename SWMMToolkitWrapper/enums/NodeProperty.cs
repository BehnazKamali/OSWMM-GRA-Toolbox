using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMMToolkitWrapper.enums
{
    public enum NodeProperty : byte
    {
        INVERTEL = 0,  /**< Invert Elevation */
        FULLDEPTH = 1,  /**< Full Depth */
        SURCHDEPTH = 2,  /**< Surcharge Depth */
        PONDAREA = 3,  /**< Ponding Area */
        INITDEPTH = 4,  /**< Initial Depth */
    }
}
