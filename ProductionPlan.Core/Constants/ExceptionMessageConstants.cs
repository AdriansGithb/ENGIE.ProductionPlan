using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionPlan.Core.Constants
{
    public static class ExceptionMessageConstants
    {
        public const string LoadTooHigh = "Target load is higher than maximum producible power";
        public const string LoadTooLow = "Target load is less than minimum producible power";
    }
}
