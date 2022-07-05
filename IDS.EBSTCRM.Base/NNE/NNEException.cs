using System;
using System.Collections.Generic;
using System.Text;

namespace IDS.EBSTCRM.Base.NNE
{
    public class NNEException : Exception 
    {
        public NNEException()
        {

        }

        public NNEException(string Message)
            :
            base(Message)
        {

        }
    }
}
