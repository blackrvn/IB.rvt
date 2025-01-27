using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nice3point.Revit.Toolkit.External;

namespace Core.Management
{
    internal class ErrorHander
    {
        private Result _result;
        public ErrorHander(Result result)
        {
            _result = result;
        }

        public void HandleError(Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                _result = Result.Cancelled;
            }
            else
            {
                _result = Result.Failed;
            }
        }


    }
}
